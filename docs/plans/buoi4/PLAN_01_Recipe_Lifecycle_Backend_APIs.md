# KẾ HOẠCH PHÁT TRIỂN (PLAN 01): BACKEND RECIPE LIFECYCLE & READINESS PROBE
> **Người thực hiện:** Tạ Nhật Nguyên (MSSV: 2312704 — Nhóm trưởng)  
> **Phân hệ:** Vòng đời Công thức (`FR-RCP-003..007`) & Giám sát Hệ thống (`FR-OBS-001`)  
> **Tài liệu căn cứ:** [SRS.md](../../requirements/SRS.md) Chương 3, 7, 8 và [RESOLVED-CONFLICTS](../../decisions/RESOLVED-CONFLICTS.md)

---

## 🎯 MỤC TIÊU & PHẠM VI CHỨC NĂNG

Xây dựng và hoàn thiện **11 REST API Minimal Endpoints** tại Backend phục vụ việc quản lý toàn bộ vòng đời bài viết công thức nấu ăn từ khi tạo nháp, cập nhật, xuất bản, lưu trữ, xóa mềm vào thùng rác cho đến khi quản trị viên khôi phục hoặc xóa vĩnh viễn, cùng 1 cổng giám sát độ sẵn sàng của PostgreSQL:

1. `POST /api/v1/recipes` — Tạo bài viết mới ở trạng thái `Draft` (`FR-RCP-003`).
2. `PUT /api/v1/recipes/{id}` — Cập nhật thông tin cơ bản có kiểm soát Concurrency `xmin` & `If-Match` (`FR-RCP-004`).
3. `PATCH /api/v1/recipes/{id}/publish` (và `POST`) — Xuất bản công thức sang `Published` (`FR-RCP-005`).
4. `PATCH /api/v1/recipes/{id}/unpublish` (và `POST`) — Hủy xuất bản về `Draft` (`FR-RCP-005`).
5. `PATCH /api/v1/recipes/{id}/archive` (và `POST`) — Lưu trữ công thức sang `Archived` (`FR-RCP-006`).
6. `PATCH /api/v1/recipes/{id}/unarchive` (và `POST`) — Mở lưu trữ về `Draft` (`FR-RCP-006`).
7. `DELETE /api/v1/recipes/{id}` — Xóa mềm công thức đưa vào thùng rác 30 ngày (`FR-RCP-007`).
8. `GET /api/v1/recipes/trash` (và `/api/v1/admin/recipes/trash`) — Lấy danh sách bài trong thùng rác cho Admin (`FR-RCP-007`).
9. `POST /api/v1/recipes/{id}/restore` (và `/api/v1/admin/recipes/{id}/restore`) — Khôi phục bài viết (`FR-RCP-007`).
10. `DELETE /api/v1/recipes/{id}/purge` (và `/api/v1/admin/recipes/{id}/purge`) — Xóa vĩnh viễn khỏi Database (`FR-RCP-007`).
11. `GET /health/ready` — Readiness Probe kiểm tra kết nối Database PostgreSQL (`FR-OBS-001`).

---

## 📋 CÁC BƯỚC THỰC HIỆN CHI TIẾT (6 BƯỚC CHUẨN)

### Bước 1: Phân tích Nghiệp vụ & Thiết kế State Machine
- **Ma trận Chuyển trạng thái (State Transition):**
  - `Draft` ➔ `Published`: Chỉ được phép khi đã có mô tả, ít nhất 1 nguyên liệu, ít nhất 1 bước làm, và danh mục `CategoryId` hợp lệ.
  - `Published` ➔ `Draft` (Unpublish): Chuyển về nháp để chỉnh sửa lớn, ẩn khỏi kết quả tìm kiếm public.
  - `Published` hoặc `Draft` ➔ `Archived`: Lưu trữ bài viết cũ, chỉ xem được ở trang cá nhân.
  - `Archived` ➔ `Draft` (Unarchive): Phục hồi về nháp để cập nhật lại.
  - Mọi trạng thái ➔ `Deleted (IsDeleted = true)`: Đưa vào thùng rác mềm, lưu `DeletedAt = DateTimeOffset.UtcNow`.
- **Ràng buộc Concurrency (Optimistic Concurrency Control):**
  - Sử dụng trường hệ thống `xmin` của PostgreSQL làm token phiên bản. Client gửi header `If-Match: "{xmin}"`.
  - Nếu token không khớp ➔ ném `RecipeConcurrencyConflictException` (HTTP `409 Conflict`).
- **Quy tắc Slug & SEO:**
  - Ở trạng thái `Draft`: sửa tiêu đề sẽ tự động cập nhật lại `Slug`.
  - Sau khi `Published`: `Slug` cố định; nếu có đổi slug thì lưu vết vào bảng `RecipeSlugHistory` để hỗ trợ 301 redirect.

### Bước 2: Thiết kế Tầng Domain (Domain Layer)
- **Entity `Recipe`:**
  - Bổ sung/kiểm tra các phương thức nội tại: `Publish()`, `Unpublish()`, `Archive()`, `Unarchive()`, `SoftDelete()`, `Restore()`, `UpdateBasicInfo()`.
  - Đảm bảo tính đóng gói (Encapsulation), không cho phép gán trực tiếp thuộc tính trạng thái từ bên ngoài.
- **Domain Exceptions:** Tạo các lớp ngoại lệ kế thừa `DomainException`:
  - `RecipeNotFoundException` (Code: `RECIPE_NOT_FOUND`)
  - `OwnershipViolationException` (Code: `RECIPE_FORBIDDEN`)
  - `RecipeIncompletePublishException` (Code: `RECIPE_PUBLISH_INCOMPLETE`)
  - `RecipeConcurrencyConflictException` (Code: `RECIPE_CONCURRENCY_CONFLICT`)
  - `InvalidRecipeStateTransitionException` (Code: `RECIPE_INVALID_STATUS_TRANSITION`)

### Bước 3: Xây dựng Tầng Application (CQRS with MediatR)
- **Commands & Handlers:**
  - `CreateRecipeCommand`: Dùng `IRepository<Recipe>.AddAsync()` và `IUnitOfWork.SaveChangesAsync()`.
  - `UpdateRecipeCommand`: So khớp `ConcurrencyToken` với `recipe.ConcurrencyToken` (`xmin`), gọi `recipe.UpdateBasicInfo()`.
  - `PublishRecipeCommand`: Kiểm tra điều kiện hoàn chỉnh `recipe.Ingredients.Count > 0 && recipe.Steps.Count > 0`, chuyển trạng thái.
  - `UnpublishRecipeCommand`, `ArchiveRecipeCommand`, `UnarchiveRecipeCommand`.
  - `DeleteRecipeCommand`: Gọi `recipe.SoftDelete()`.
  - `RestoreRecipeCommand`: Kiểm tra thời hạn 30 ngày (`DeletedAt >= UtcNow - 30 days`), gọi `recipe.Restore()`.
  - `PurgeRecipeCommand`: Gọi `IRepository<Recipe>.Delete(recipe)` xóa cứng khỏi DB.
- **Queries & Handlers:**
  - `GetTrashedRecipesQuery`: Sử dụng `IApplicationDbContext.Recipes.IgnoreQueryFilters().Where(r => r.IsDeleted)` với phân trang chuẩn `PagedResult<TrashedRecipeDto>`.
- **Validation Pipeline:**
  - Viết `CreateRecipeCommandValidator` và `UpdateRecipeCommandValidator` bằng FluentValidation: Tiêu đề (3-150 ký tự), thời gian nấu/chuẩn bị (≥ 0), khẩu phần (1-100), độ khó (1-3).
  - Tích hợp tự động qua `ValidationBehavior`.

### Bước 4: Xây dựng Tầng Presentation (API Minimal Endpoints)
- **Đăng ký Endpoints trong `RecipeLifecycleEndpoints.cs`:**
  - Khai báo Route Group `/api/v1/recipes`.
  - Trích xuất `[FromHeader(Name = "If-Match")]` và gán vào Command.
  - Thiết lập Header `ETag: "{result.ConcurrencyToken}"` trong Response.
- **Đăng ký Readiness Probe trong `Program.cs`:**
  - `GET /health/ready`: Thực thi `dbContext.Database.CanConnectAsync()`. Trả về `200 OK {"status":"Ready"}` hoặc `503 Service Unavailable`.
- **Xử lý Ngoại lệ tại `GlobalExceptionHandler.cs`:**
  - Map chính xác `ValidationException` ➔ `422 Unprocessable Entity`.
  - Map `OwnershipViolationException` ➔ `403 Forbidden`.
  - Map `RecipeNotFoundException` ➔ `404 Not Found`.
  - Map `RecipeIncompletePublishException` ➔ `422 Unprocessable Entity`.
  - Map `RecipeConcurrencyConflictException` & `DbUpdateConcurrencyException` ➔ `409 Conflict`.

### Bước 5: Viết file Demo HTTP Request
- Tạo file `backend/src/CulinaryBlog.API/RecipeLifecycle.http` chứa sẵn 13 request mẫu đầy đủ method, header `If-Match`, body JSON để demo nhanh bằng VS Code REST Client hoặc Postman.

### Bước 6: Kiểm thử Đơn vị (Unit Testing)
- Viết 4 bộ test xUnit trong `backend/tests/CulinaryBlog.UnitTests/Recipes/`:
  - `RecipeLifecycleStateTransitionTests.cs`: Kiểm tra 100% quy tắc chuyển trạng thái và ném exception.
  - `RecipeLifecycleCommandsValidationTests.cs`: Kiểm tra FluentValidation với dữ liệu biên.
  - `AdminRecipeLifecycleTests.cs`: Kiểm tra Soft Delete, Restore, Purge và quyền hạn Admin.
  - `SlugHelperTests.cs`: Kiểm tra thuật toán sinh SEO slug Tiếng Việt không dấu.
- Chạy `dotnet test` đạt **33/33 tests Passed 100%**.

---

## 🎤 BỘ CÂU HỎI VẤN ĐÁP THƯỜNG GẶP (Q&A CHEATSHEET)

**Q1: Làm sao em ngăn chặn việc hai người cùng sửa một công thức rồi người lưu sau ghi đè mất dữ liệu của người trước?**
> *Trả lời:* "Em sử dụng kỹ thuật **Optimistic Concurrency Control**. Entity `Recipe` được cấu hình ánh xạ trường hệ thống `xmin` của PostgreSQL. Khi client đọc dữ liệu, backend trả về mã `xmin` qua header `ETag`. Khi gửi request cập nhật (`PUT /recipes/{id}`), client phải gửi kèm `If-Match: "{xmin}"`. Nếu bản ghi đã bị sửa đổi trước đó, backend lập tức ném lỗi `409 Conflict` (`RECIPE_CONCURRENCY_CONFLICT`) để client nạp lại dữ liệu mới nhất mà không làm mất thông tin."

**Q2: Tại sao em không dùng Controller mà lại dùng Minimal API?**
> *Trả lời:* "Minimal API trong .NET 10 giúp giảm thiểu overhead của MVC framework, khởi động nhanh hơn, tiêu tốn ít bộ nhớ hơn và rất phù hợp cho kiến trúc Clean Architecture khi kết hợp cùng MediatR (mỗi endpoint chỉ đóng vai trò map HTTP request vào một Command/Query tương ứng)."

**Q3: Tại sao cần phân biệt giữa `/health/live` và `/health/ready`?**
> *Trả lời:* "`/health/live` (Liveness) dùng để báo cho Docker/K8s biết tiến trình .NET còn chạy hay bị deadlock để restart container. Trong khi `/health/ready` (Readiness) do em phụ trách sẽ kiểm tra xem ứng dụng đã kết nối thông suốt tới Database PostgreSQL hay chưa để bộ cân bằng tải (Load Balancer) quyết định có điều hướng traffic người dùng vào hay không."
