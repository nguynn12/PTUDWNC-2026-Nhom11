# KẾ HOẠCH TRIỂN KHAI BUỔI 5 — PHÂN HỆ VÒNG ĐỜI RECIPE, BẢO MẬT & CONCURRENCY
**Thành viên thực hiện:** Tạ Nhật Nguyên (MSSV: 2312704 — Nhóm 11)  
**Phân hệ:** Recipe Lifecycle, Admin Trash, Optimistic Concurrency (`xmin`/`If-Match`), TypeScript Types  
**Tài liệu căn cứ:** [docs/buoi5.md](file:///c:/Users/storm/Downloads/PTUDWNC-2026-Nhom11/docs/buoi5.md) & [docs/requirements/SRS.md](file:///c:/Users/storm/Downloads/PTUDWNC-2026-Nhom11/docs/requirements/SRS.md)

---

## 1. MỤC TIÊU CÔNG VIỆC BUỔI 5 CỦA TẠ NHẬT NGUYÊN

1. **Cài đặt Phân quyền Endpoint (Authorization)**:<br>•
   - Gắn `.RequireAuthorization("Author")` cho các thao tác tạo/sửa/xóa/lưu trữ công thức (`POST /recipes`, `PUT /recipes/{id}`, `DELETE /recipes/{id}`, `archive`, `unarchive`);<br>•
   - Gắn `.RequireAuthorization("VerifiedAuthor")` cho thao tác xuất bản (`publish`, `unpublish`) nhằm đảm bảo tác giả đã xác thực email hoặc có quyền Admin (theo FR-AUTH-008 & SRS 2.3);<br>•
   - Gắn `.RequireAuthorization("Admin")` cho toàn bộ nhóm `/admin/recipes` (`/trash`, `/restore`, `/purge`).

2. **Chặn triệt để truy cập ẩn danh trong các Handler**:<br>•
   - Loại bỏ đoạn code bỏ qua kiểm tra khi `UserId` rỗng;<br>•
   - Ném `UnauthorizedException` (HTTP 401) nếu người dùng chưa đăng nhập;<br>•
   - Ném `ForbiddenException` (HTTP 403 `RECIPE_FORBIDDEN`) nếu người thao tác không phải tác giả và không phải Admin.

3. **Hoàn thiện kiểm soát Concurrency Token (`xmin` & `If-Match`)**:<br>•
   - Bổ sung `uint? ConcurrencyToken` vào 5 Command: `Publish`, `Unpublish`, `Archive`, `Unarchive`, `Delete`;<br>•
   - Trích xuất header `If-Match` từ Minimal API và truyền vào MediatR;<br>•
   - So sánh với `xmin` của PostgreSQL trong Handler; ném `ConflictException` (HTTP 409 `RECIPE_CONCURRENCY_CONFLICT`) nếu dữ liệu bị thay đổi từ phiên khác;<br>•
   - Đính kèm header `ETag: "{xmin}"` trong response.

4. **Chuẩn hóa TypeScript Types trên Frontend**:<br>•
   - Gom các type trùng lặp (`RecipeStatus`, `RecipeDifficulty`, `RecipeNutritionDto`, `RecipeSummaryDto`, `RecipeFullDetailDto`, `DashboardMetrics`) về một nguồn duy nhất tại `src/types/api.ts`;<br>•
   - Re-export và dọn dẹp `src/lib/types.ts` để tránh cảnh báo phân mảnh kiểu dữ liệu.

5. **Viết Bộ Kiểm Thử Tự Động (Unit & Integration Tests)**:<br>•
   - Tạo bộ test chuyên sâu kiểm tra 401 (chưa đăng nhập), 403 (sai quyền), 409 (xung đột `xmin`), 422 (thiếu nguyên liệu/bước khi xuất bản);<br>•
   - Đảm bảo 100% tests pass.

6. **Lập Báo Cáo Buổi 5**:<br>•
   - Tạo file `docs/Lab05_2312704_TaNhatNguyen.md` theo mẫu bảng tiến độ của nhóm;<br>•
   - Tuân thủ quy chuẩn: mỗi dấu chấm phẩy xuống dòng (`<br>•`).

---

## 2. KẾ HOẠCH TRIỂN KHAI 6 BƯỚC

### BƯỚC 1: SỬA FILE ENDPOINT GẮN REQUIREAUTHORIZATION
* **Tệp cần sửa:** `backend/src/CulinaryBlog.API/Endpoints/RecipeLifecycleEndpoints.cs`<br>•
* **Nội dung:**
  - Bổ sung `using CulinaryBlog.Domain.Constants;`<br>•
  - Gắn chính sách `Roles.Author` cho các route tạo, sửa, xóa, lưu trữ;<br>•
  - Gắn chính sách `Policies.VerifiedAuthor` cho các route publish, unpublish;<br>•
  - Gắn chính sách `Roles.Admin` cho nhóm route `/admin/recipes`.

### BƯỚC 2: CẬP NHẬT KIỂM TRA QUYỀN SỞ HỮU TRONG COMMAND HANDLERS
* **Tệp cần sửa:**
  - `CreateRecipeCommandHandler.cs`<br>•
  - `UpdateRecipeCommandHandler.cs`<br>•
  - `PublishRecipeCommand.cs`<br>•
  - `UnpublishRecipeCommand.cs`<br>•
  - `ArchiveRecipeCommand.cs`<br>•
  - `UnarchiveRecipeCommand.cs`<br>•
  - `DeleteRecipeCommand.cs`<br>•
  - `RestoreRecipeCommand.cs`<br>•
  - `PurgeRecipeCommand.cs`<br>•
* **Logic áp dụng:**
  - Bắt buộc kiểm tra `currentUserService.UserId`; nếu `null` hoặc khoảng trắng thì ném `UnauthorizedException`;<br>•
  - Nếu `!currentUserService.IsAdmin && recipe.AuthorId != currentUserService.UserId` thì ném `ForbiddenException("Bạn không có quyền thao tác trên công thức này.", "RECIPE_FORBIDDEN")`.

### BƯỚC 3: MỞ RỘNG COMMAND & XỬ LÝ CONCURRENCY TOKEN
* **Cập nhật Command Record:**
  - `PublishRecipeCommand(Guid Id, uint? ConcurrencyToken = null)`<br>•
  - `UnpublishRecipeCommand(Guid Id, uint? ConcurrencyToken = null)`<br>•
  - `ArchiveRecipeCommand(Guid Id, uint? ConcurrencyToken = null)`<br>•
  - `UnarchiveRecipeCommand(Guid Id, uint? ConcurrencyToken = null)`<br>•
  - `DeleteRecipeCommand(Guid Id, uint? ConcurrencyToken = null)`<br>•
* **Cập nhật Handlers:**
  - Truy vấn giá trị `xmin` hiện tại từ Entity Framework Core;<br>•
  - Nếu `request.ConcurrencyToken.HasValue && request.ConcurrencyToken.Value != currentXmin` thì ném `ConflictException("Dữ liệu công thức đã bị thay đổi bởi phiên làm việc khác. Vui lòng tải lại trang.", "RECIPE_CONCURRENCY_CONFLICT")`.

### BƯỚC 4: GOM VÀ CHUẨN HÓA TYPESCRIPT TYPES
* **Tệp cần sửa:**
  - `frontend/src/types/api.ts`: Bổ sung `DashboardMetrics`, `RecipeFullDetailDto`, thống nhất `RecipeStatus` và `RecipeDifficulty`;<br>•
  - `frontend/src/lib/types.ts`: Re-export toàn bộ từ `@/types/api` và loại bỏ code trùng.

### BƯỚC 5: VIẾT TESTS MỚI & CHẠY BỘ KIỂM THỬ
* **Tệp test mới:** `backend/tests/CulinaryBlog.UnitTests/Recipes/RecipeLifecycleAuthorizationAndConcurrencyTests.cs`<br>•
* **Kiểm tra:** Chạy `dotnet test` và `npm run typecheck` + `npm run build`.

### BƯỚC 6: TẠO BÁO CÁO LAB 5 & CẬP NHẬT TIẾN ĐỘ
* **Tệp báo cáo:** `docs/Lab05_2312704_TaNhatNguyen.md`<br>•
* **Tệp phân công:** Đánh dấu hoàn thành trong `docs/buoi5.md`.
