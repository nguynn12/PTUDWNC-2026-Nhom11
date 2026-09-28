# TÀI LIỆU NHÁNH BACKEND — PHÂN CÔNG & LỘ TRÌNH THỰC HIỆN

**Tên nhánh:** `2312731_NguyenPhuQuy_Recipe-details_backend`  
**Nhánh xuất phát:** `develop` (Commit `40ab20a`)  
**Thành viên phụ trách:** Nguyễn Phú Quý — MSSV: **2312731** (Thành viên 4)  
**Phân hệ chuyên trách:** **Recipe Content và Media** *(Quản lý Nguyên liệu, Các bước thực hiện, Hình ảnh công thức & Tích hợp lưu trữ tệp tin)*  
**Chủ đề nghiệp vụ xuyên suốt:** *"Nội dung chi tiết của một công thức nấu ăn được tạo lập, hướng dẫn và minh họa như thế nào"*

---

## 1. Căn cứ Phân công & Đặc tả Kỹ thuật
Tài liệu này được xác lập dựa trên các căn cứ chính thức của dự án:
1. **Phân công bài tập Lab - Buổi 3** trong [`README.md`](./README.md) của nhóm (Cột Thành viên 4: Nguyễn Phú Quý).
2. **Đặc tả Yêu cầu Phần mềm (SRS v1.2.0)** tại [`docs/requirements/SRS.md`](./docs/requirements/SRS.md):
   - `FR-RCP-008`: Quản lý Ảnh Recipe (Upload, Thumbnail, Primary, OrderIndex).
   - `FR-RCP-009`: Quản lý Nguyên liệu (CRUD RecipeIngredient, số lượng decimal, đơn vị đo, ghi chú).
   - `FR-RCP-010`: Quản lý Các bước thực hiện (CRUD RecipeStep, reorder, auto-renumber StepNumber).
   - `FR-FILE-001`, `FR-FILE-002`: Dịch vụ lưu trữ Object Storage tương thích S3/MinIO.
   - Mục 7.3, 7.4, 7.5: Cấu trúc thực thể `RecipeStep`, `RecipeIngredient`, `RecipeImage`.
   - Mục 8.3: Đặc tả REST API cho child resources của Recipe.
3. **Quyết định giải quyết mâu thuẫn** tại [`docs/decisions/RESOLVED-CONFLICTS.md`](./docs/decisions/RESOLVED-CONFLICTS.md):
   - `E1`: Số lượng `Quantity` giữ kiểu `decimal(10,3)`.
   - `E2`: `Quantity` và `Unit` đều nullable (hỗ trợ "vừa đủ"); validate có điều kiện (có cái này thì bắt buộc có cái kia).
   - `E5`: `RecipeStep` có `Description (text)`, đổi `TimerMinutes` → `DurationMinutes`.
   - `E6`: Cập nhật ảnh chính gộp chung vào `PATCH /api/v1/recipes/{id}/images/{imageId}`, bỏ route `/primary` riêng. Khi set `isPrimary=true`, set tất cả ảnh khác `=false` trong cùng transaction.
   - `E7`: Response Upload ảnh bắt buộc đủ 4 trường `{imageId, originalUrl, altText, isPrimary}`.
   - `E8`: `StepNumber` tuyến tính nguyên dương tự renumber, `ParentStepId` nullable phục vụ future-proof.
   - `C2`: Chuẩn hóa Response Envelope `{ "data": ... }` cho toàn bộ mutation/detail endpoint.
   - `C3`: Chuẩn hóa lỗi 400 (malformed/file invalid) vs 422 (validation rules).
   - `C6`: Concurrency conflict trả HTTP 409 Conflict.

---

## 2. Danh mục 10 Trách nhiệm Cốt lõi của Phân hệ

| # | Chức năng nghiệp vụ | Tầng Domain & Database (Lab 2) | Tầng Application (Lab 3) | Tầng API & Tích hợp (Lab 3) |
|---|---|---|---|---|
| **1** | **Mô hình Hợp đồng DTOs & Requests** | Các Entity `RecipeIngredient`, `RecipeStep`, `RecipeImage` | DTOs phản hồi & Request contracts cho Ingredient, Step, Image | Định nghĩa Schema OpenAPI/Swagger, binding dữ liệu chuẩn hóa |
| **2** | **Thêm Nguyên liệu (Add Ingredient)** | Bảng `RecipeIngredients`, quan hệ 1-N với `Recipe` | `AddRecipeIngredientCommand`, validate Name, Quantity > 0, Unit nullable | `POST /api/v1/recipes/{id}/ingredients`, kiểm tra quyền Owner/Admin |
| **3** | **Cập nhật Nguyên liệu (Update Ingredient)** | Cột `Name`, `Quantity`, `Unit`, `Notes`, `OrderIndex` | `UpdateRecipeIngredientCommand`, cập nhật thông tin và thứ tự | `PUT /api/v1/recipes/{id}/ingredients/{ingredientId}`, kiểm tra If-Match |
| **4** | **Xóa Nguyên liệu (Delete Ingredient)** | Soft delete (`IsDeleted=true`) theo BaseEntity | `DeleteRecipeIngredientCommand`, xóa mềm nguyên liệu | `DELETE /api/v1/recipes/{id}/ingredients/{ingredientId}`, trả về 204 No Content |
| **5** | **Thêm Bước nấu (Add Step)** | Bảng `RecipeSteps`, Index `(RecipeId, StepNumber)` | `AddRecipeStepCommand`, tự động gán `StepNumber = max + 1` | `POST /api/v1/recipes/{id}/steps`, kiểm tra quyền tác giả |
| **6** | **Cập nhật Bước nấu (Update Step)** | Cột `Title`, `Description`, `DurationMinutes`, `ImageUrl` | `UpdateRecipeStepCommand`, cập nhật nội dung chi tiết bước | `PUT /api/v1/recipes/{id}/steps/{stepId}` |
| **7** | **Sắp xếp & Xóa Bước nấu (Reorder & Delete)** | Đảm bảo tính liên tục của `StepNumber` | `ReorderRecipeStepsCommand` & `DeleteRecipeStepCommand` tự động renumber 1..N | `PUT /api/v1/recipes/{id}/steps/reorder` & `DELETE /api/v1/recipes/{id}/steps/{stepId}` |
| **8** | **Dịch vụ Lưu trữ File (Storage Service)** | Cấu hình lưu trữ S3/MinIO bucket `culinary-blog` | `IFileStorageService`, validation MIME & Magic Bytes (JPEG/PNG/WebP/AVIF <= 5MB) | Service xử lý file an toàn, chống path traversal |
| **9** | **Upload & Quản lý Hình ảnh (Image Management)** | Bảng `RecipeImages`, cờ `IsPrimary`, `OrderIndex` | `UploadRecipeImageCommand`, `UpdateRecipeImageCommand`, `DeleteRecipeImageCommand` | `POST /api/v1/recipes/{id}/images`, `PATCH .../images/{imageId}`, `DELETE ...` |
| **10** | **Kiểm tra Quyền & Concurrency** | Trường `AuthorId`, `xmin` (PostgreSQL) | Service xác thực quyền sở hữu Recipe (`OwnerOrAdmin`) & concurrency check | Trả mã 403 Forbidden nếu không chính chủ, 409 Conflict nếu xung đột |

---

## 3. Lộ trình Commits Nguyên tử (Atomic Commits)

Nhánh được triển khai theo các commits độc lập, mỗi commit hoàn thành trọn vẹn một thành phần và bảo đảm giải pháp luôn **biên dịch thành công (Build Green)** và vượt qua 100% Unit Tests:

```text
develop (40ab20a)
   │
   ├── Commit 01: tài liệu: bổ sung README_BRANCH phân công công việc và lộ trình nhánh backend TV4
   ├── Commit 02: thêm: DTOs và Request Contracts cho phân hệ Recipe Content và Media
   ├── Commit 03: thêm: dịch vụ trừu tượng IFileStorageService và kiểm tra Magic Bytes file ảnh
   ├── Commit 04: thêm: Commands và Handlers quản lý Nguyên liệu (RecipeIngredient)
   ├── Commit 05: thêm: Commands và Handlers quản lý Bước nấu (RecipeStep) kèm logic tự động renumber
   ├── Commit 06: thêm: Commands và Handlers quản lý Hình ảnh (RecipeImage) và logic xử lý ảnh chính
   ├── Commit 07: thêm: Minimal API Endpoints cho phân hệ Recipe Content (Ingredients, Steps, Images)
   ├── Commit 08: kiểm-thử: bổ sung Unit Tests cho phân hệ Recipe Content và Media
   └── Commit 09: hoàn thiện: tổng kết và rà soát nghiệm thu toàn diện phân hệ Backend TV4
         │
         ▼
   Tạo Pull Request vào nhánh develop
```

---

## 4. Danh mục API Endpoints Quản lý trên Nhánh

| HTTP Method | Endpoint Route | Quyền truy cập | Trách nhiệm xử lý |
|---|---|---|---|
| `GET` | `/api/v1/recipes/{id:guid}/ingredients` | Public | Đọc danh sách nguyên liệu của công thức sắp xếp theo `OrderIndex` |
| `POST` | `/api/v1/recipes/{id:guid}/ingredients` | Owner/Admin | Thêm nguyên liệu mới vào công thức |
| `PUT` | `/api/v1/recipes/{id:guid}/ingredients/{ingredientId:guid}` | Owner/Admin | Cập nhật thông tin nguyên liệu |
| `DELETE` | `/api/v1/recipes/{id:guid}/ingredients/{ingredientId:guid}` | Owner/Admin | Xóa nguyên liệu khỏi công thức (204 No Content) |
| `GET` | `/api/v1/recipes/{id:guid}/steps` | Public | Đọc danh sách các bước nấu sắp xếp theo `StepNumber` tăng dần |
| `POST` | `/api/v1/recipes/{id:guid}/steps` | Owner/Admin | Thêm bước nấu mới, tự động cấp `StepNumber` kế tiếp |
| `PUT` | `/api/v1/recipes/{id:guid}/steps/{stepId:guid}` | Owner/Admin | Cập nhật tiêu đề, mô tả, thời gian hoặc ảnh của bước nấu |
| `PUT` | `/api/v1/recipes/{id:guid}/steps/reorder` | Owner/Admin | Sắp xếp lại danh sách các bước theo thứ tự mảng IDs mới |
| `DELETE` | `/api/v1/recipes/{id:guid}/steps/{stepId:guid}` | Owner/Admin | Xóa bước nấu và tự động renumber các bước còn lại |
| `GET` | `/api/v1/recipes/{id:guid}/images` | Public | Đọc danh sách ảnh minh họa của công thức |
| `POST` | `/api/v1/recipes/{id:guid}/images` | Owner/Admin | Upload ảnh minh họa mới (multipart/form-data), lưu trữ MinIO |
| `PATCH` | `/api/v1/recipes/{id:guid}/images/{imageId:guid}` | Owner/Admin | Cập nhật `altText`, thứ tự `orderIndex` hoặc đặt ảnh chính `isPrimary` |
| `DELETE` | `/api/v1/recipes/{id:guid}/images/{imageId:guid}` | Owner/Admin | Xóa ảnh minh họa (tự động chuyển ảnh chính nếu xóa ảnh primary) |

---

## 5. Quy chuẩn Kỹ thuật & Nghiệm thu (Definition of Done)
- **Kiến trúc:** Tuân thủ Clean Architecture, `CulinaryBlog.Application` độc lập thông qua `IApplicationDbContext`.
- **Định dạng Envelope:** Response thành công trả về chuẩn `{ "data": ... }`.
- **Xử lý lỗi:** Lỗi nghiệp vụ trả về chuẩn **RFC 7807 Problem Details** (400, 403, 404, 409, 422).
- **Quyền hạn:** Kiểm tra quyền tác giả sở hữu công thức (`AuthorId == currentUserId` hoặc role `Admin`).
- **An toàn tệp tin:** Kiểm tra định dạng ảnh bằng Magic Bytes thực tế, chặn file độc hại, giới hạn 5MB.
- **Biên dịch & Kiểm thử:** `dotnet build` đạt 0 Warning, 0 Error; 100% Unit Tests passed.
