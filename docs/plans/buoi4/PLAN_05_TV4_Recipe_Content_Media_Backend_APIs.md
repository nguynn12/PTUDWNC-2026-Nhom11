# KẾ HOẠCH TRIỂN KHAI BACKEND APIS: NGUYÊN LIỆU, BƯỚC NẤU, UPLOAD ẢNH MINIO & SCALAR UI
> **Mã Kế hoạch:** `PLAN-05`  
> **Thành viên phụ trách:** Thành viên 4 — Nguyễn Phú Quý (MSSV: `2312731`)  
> **Phân hệ:** Quản lý Chi tiết Công thức (Nguyên liệu, Bước nấu, Dinh dưỡng) & Quản lý Hình ảnh (MinIO Storage)  
> **Tích hợp kiểm thử API:** Scalar API Reference (`/scalar/v1`, `/scalar`) thay thế Swagger UI

---

## 1. BẢNG ĐẶC TẢ 10 API ENDPOINTS CỦA THÀNH VIÊN 4 (THEO SRS MỤC 8.3)

| STT | HTTP Method & Route | Chức năng nghiệp vụ | Request / Ràng buộc | HTTP Status |
|:---:|---|---|---|---|
| **1** | `POST /api/v1/recipes/{id}/images` | Upload hình ảnh món ăn lên MinIO Object Storage (`multipart/form-data`) | `file` ($\le 5\text{MB}$, JPEG/PNG/WebP/AVIF), `altText`, `isPrimary` | `201 Created`, `403`, `404`, `422` |
| **2** | `PATCH /api/v1/recipes/{id}/images/{imageId}` | Cập nhật metadata ảnh (`AltText`, `SortOrder`) và chuyển cờ `IsPrimary` | `UpdateRecipeImageRequest` | `200 OK`, `403`, `404` |
| **3** | `DELETE /api/v1/recipes/{id}/images/{imageId}` | Xóa hình ảnh khỏi DB và gọi `IStorageService.DeleteAsync` trên MinIO | Tự động đôn ảnh kế tiếp lên làm `IsPrimary` nếu xóa ảnh chính | `204 No Content`, `403`, `404` |
| **4** | `POST /api/v1/recipes/{id}/ingredients` | Thêm nguyên liệu mới kèm định lượng số thực (`Quantity`, `Unit`) | Hỗ trợ gia vị "vừa đủ" (`Quantity = null`, `Unit = null` theo E1 & E2) | `201 Created`, `403`, `404`, `422` |
| **5** | `PUT /api/v1/recipes/{id}/ingredients/{ingredientId}` | Cập nhật thông tin & định lượng nguyên liệu | `UpdateRecipeIngredientRequest` | `200 OK`, `403`, `404`, `422` |
| **6** | `DELETE /api/v1/recipes/{id}/ingredients/{ingredientId}` | Xóa nguyên liệu khỏi công thức | Kiểm tra quyền sở hữu `EnsureOwnershipOrAdmin` | `204 No Content`, `403`, `404` |
| **7** | `POST /api/v1/recipes/{id}/steps` | Thêm bước chế biến mới kèm thời gian hẹn giờ (`DurationMinutes`) | `CreateRecipeStepRequest` | `201 Created`, `403`, `404`, `422` |
| **8** | `PUT /api/v1/recipes/{id}/steps/{stepId}` | Cập nhật tiêu đề, hướng dẫn và hẹn giờ của bước làm | `UpdateRecipeStepRequest` | `200 OK`, `403`, `404`, `422` |
| **9** | `PUT /api/v1/recipes/{id}/steps/reorder` | Sắp xếp lại thứ tự toàn bộ các bước làm trong 1 Transaction nguyên tử | `ReorderRecipeStepsRequest` (`StepIds` khớp 100% tập bước hiện tại) | `200 OK`, `403`, `404`, `422` |
| **10** | `DELETE /api/v1/recipes/{id}/steps/{stepId}` | Xóa bước làm và tự động đánh lại số thứ tự liên tục `1..N` | `NormalizeStepNumbersAsync` trong 1 lần `SaveChangesAsync` | `204 No Content`, `403`, `404` |

---

## 2. TÍCH HỢP SCALAR UI THAY THẾ SWAGGER UI
- Cài đặt gói `Scalar.AspNetCore` (v2.17.11) kết hợp `Microsoft.AspNetCore.OpenApi` (v10.0.12) trong `CulinaryBlog.API.csproj`.
- Cấu hình trong `Program.cs`:
  - `builder.Services.AddOpenApi();`
  - `app.MapOpenApi();` (xuất đặc tả chuẩn OpenAPI 3.1 tại `/openapi/v1.json`)
  - `app.MapScalarApiReference(...)` tại đường dẫn `/scalar/v1` và redirect `/scalar` $\rightarrow$ `/scalar/v1` với giao diện `ScalarTheme.Purple`, tích hợp sẵn trình thử nghiệm REST Client (`Test Request`) và hỗ trợ điền Bearer Token.

---

## 3. KIỂM THỬ ĐƠN VỊ (UNIT TESTS)
- Tổng cộng **70 Unit Tests** cho phân hệ của TV4 (và **244 Unit Tests** toàn bộ Solution):
  - `RecipeIngredientsControllerTests.cs` & `RecipeIngredientServiceTests.cs`
  - `RecipeStepsControllerTests.cs` & `RecipeStepServiceTests.cs`
  - `RecipeImagesControllerTests.cs` & `RecipeImageServiceTests.cs`
  - `CreateUpdateRecipeNutritionValidatorsTests.cs`, `DomainExceptionsAndProblemDetailsTests.cs`, `MinioStorageServiceTests.cs`
