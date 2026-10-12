# BÁO CÁO LAB - MÔN PHÁT TRIỂN ỨNG DỤNG WEB NÂNG CAO

**Lab:** 05  
**Thời gian:** 07/10/2026 đến 14/10/2026  
**MSSV:** 2312731  
**Họ và tên:** Nguyễn Phú Quý  
**Nhóm:** 11  

---

## Bảng phân công & Tiến độ công việc

| STT | Công việc được giao | Liên kết đến GitHub branch | Tiến độ |
|---:|---|---|---:|
| 1 | **Thiết lập và Cấu hình Upload Rate Limiting (5 requests / phút / IP) cho tác vụ tải ảnh công thức.**<br><br>**Đã hoàn thành:**<br>• Cài đặt lớp `UploadRateLimiting` tại `backend/src/CulinaryBlog.API/RateLimiting/UploadRateLimiting.cs` với Policy `"Upload"`, ngưỡng `PermitLimit = 5`, cửa sổ `Window = 1 phút`, phân vùng theo Client Remote IP;<br>• Triển khai hàm `OnRejectedAsync` xử lý phản hồi HTTP 429 chuẩn RFC 7807 Problem Details: mã lỗi `type = RATE_LIMIT_EXCEEDED`, tiêu đề `"Too Many Requests"`, nội dung chi tiết `"Bạn đã gửi quá 5 yêu cầu tải lên tệp tin mỗi phút. Vui lòng thử lại sau."`, và tự động gắn header `Retry-After`;<br>• Đăng ký policy vào `services.AddRateLimiter(...)` trong `DependencyInjection.cs`, xử lý phân biệt lỗi từ chối theo URL đường dẫn (Auth vs Upload);<br>• Gắn `.RequireRateLimiting(UploadRateLimiting.PolicyName)` vào endpoint `POST /api/v1/recipes/{recipeId}/images`;<br>• Xây dựng bộ Unit Tests `UploadRateLimitingTests.cs` kiểm tra cô lập IP partition, chấp thuận 5 request đầu, chặn request thứ 6 và trả về đầy đủ định dạng RFC 7807 kèm `Retry-After`.<br><br>**Chưa hoàn thành:** Không có. | [2312731_NguyenPhuQuy_Recipe-details_backend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312731_NguyenPhuQuy_Recipe-details_backend) | 100% |
| 2 | **Cấu hình Authentication & Authorization cho toàn bộ Endpoints phân hệ Recipe Content & Media.**<br><br>**Đã hoàn thành:**<br>• Gắn `.RequireAuthorization()` cho toàn bộ các endpoint thay đổi dữ liệu của Recipe Ingredients (`POST /recipes/{id}/ingredients`, `PUT /recipes/{id}/ingredients/{id}`, `DELETE /recipes/{id}/ingredients/{id}`);<br>• Gắn `.RequireAuthorization()` cho các endpoint thay đổi dữ liệu của Recipe Steps (`POST /recipes/{id}/steps`, `PUT /recipes/{id}/steps/reorder`, `PUT /recipes/{id}/steps/{id}`, `DELETE /recipes/{id}/steps/{id}`);<br>• Gắn `.RequireAuthorization()` cho các endpoint tải lên, cập nhật và xóa hình ảnh Recipe Images (`POST /recipes/{id}/images`, `PATCH /recipes/{id}/images/{id}`, `DELETE /recipes/{id}/images/{id}`);<br>• Giữ các endpoint đọc dữ liệu (`GET /...`) ở trạng thái công khai (Public Read) để khách vãng lai và độc giả xem nội dung bình thường;<br>• Bắt buộc xác thực quyền tác giả (Recipe Ownership Check): người dùng chưa đăng nhập nhận HTTP 401 Unauthorized; người dùng đã đăng nhập nhưng không phải tác giả và không phải Quản trị viên (`Admin`) bị từ chối với HTTP 403 Forbidden chuẩn RFC 7807.<br><br>**Chưa hoàn thành:** Không có. | [2312731_NguyenPhuQuy_Recipe-details_backend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312731_NguyenPhuQuy_Recipe-details_backend) | 100% |
| 3 | **Cập nhật kịch bản kiểm thử API chi tiết tại `RecipeContentMedia.http`.**<br><br>**Đã hoàn thành:**<br>• Bổ sung kịch bản `SEC-01`: Chặn HTTP 401 Unauthorized khi thêm nguyên liệu mà không gửi Bearer token;<br>• Bổ sung kịch bản `SEC-02`: Chặn HTTP 403 Forbidden khi người dùng lạ gửi token cố ý sửa nội dung bước làm của người khác;<br>• Bổ sung kịch bản `SEC-03`: Chặn HTTP 401 Unauthorized khi xóa ảnh minh họa không kèm token;<br>• Bổ sung kịch bản kiểm thử Rate Limiting `RATE-01` đến `RATE-05`: Gửi 5 yêu cầu upload ảnh hợp lệ thành công (đếm 1..5/5);<br>• Bổ sung kịch bản `RATE-06`: Gửi yêu cầu thứ 6 trong cùng 1 phút, ghi nhận phản hồi HTTP 429 Too Many Requests kèm header `Retry-After` và JSON Problem Details;<br>• Duy trì đầy đủ và đồng bộ toàn bộ 10 API cốt lõi của phân hệ phục vụ Scalar API Reference và Visual Studio / VS Code REST Client.<br><br>**Chưa hoàn thành:** Không có. | [2312731_NguyenPhuQuy_Recipe-details_backend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312731_NguyenPhuQuy_Recipe-details_backend) | 100% |
| 4 | **Chuẩn hóa và gom toàn bộ Kiểu dữ liệu TypeScript (TS types) của phân hệ về `frontend/src/types/`.**<br><br>**Đã hoàn thành:**<br>• Tạo file trung tâm `frontend/src/types/recipe-content.ts` định nghĩa toàn diện hệ thống DTO và Request payload theo đúng SRS v1.3.0:<br>  - Thông tin dinh dưỡng: `RecipeNutritionDto`;<br>  - Phân hệ Nguyên liệu: `RecipeIngredientDto`, `CreateRecipeIngredientRequest`, `UpdateRecipeIngredientRequest`;<br>  - Phân hệ Bước chế biến: `RecipeStepDto`, `CreateRecipeStepRequest`, `UpdateRecipeStepRequest`, `ReorderRecipeStepsRequest`;<br>  - Phân hệ Hình ảnh & Media: `RecipeImageDto`, `UpdateRecipeImageRequest`;<br>  - Payload tạo/sửa tổng thể: `CategoryOptionDto`, `CreateRecipePayload`, `UpdateRecipePayload`, `RecipeFullDetailDto`;<br>  - Mã lỗi phân hệ: `RecipeContentErrorCode`, `RecipeContentProblemDetails`;<br>• Tạo file `frontend/src/types/index.ts` xuất khẩu chuẩn mực các types của toàn hệ thống, loại trừ xung đột duplicate types với `api.ts`;<br>• Cập nhật `frontend/src/lib/types.ts`, `frontend/src/lib/api-client.ts`, và `frontend/src/components/recipes/RecipeWizardEditor.tsx` chuyển sang import từ `@/types/recipe-content`;<br>• Kiểm tra biên dịch dự án Next.js 16 (`npm run build`) đạt 100% thành công, không có lỗi typecheck hay build warning.<br><br>**Chưa hoàn thành:** Không có. | [2312731_NguyenPhuQuy_Recipe-details_frontend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312731_NguyenPhuQuy_Recipe-details_frontend) | 100% |
| 5 | **Nghiệm thu toàn diện, chạy Kiểm thử tự động (Unit Tests) và đồng bộ Git.**<br><br>**Đã hoàn thành:**<br>• Chạy toàn bộ bộ kiểm thử tự động của backend: **247/247 Unit Tests passed (100% Pass)**, bao gồm 3 tests kiểm tra chặt chẽ Upload Rate Limiting;<br>• Chạy quy trình đóng gói frontend: **14/14 static pages generated**, biên dịch thành công trong 1.9s với Next.js Turbopack;<br>• Commit và đẩy thành công cả 2 nhánh lên GitHub remote:<br>  - Nhánh Backend: `160f724 feat: them UploadRateLimiting 5 req/phut va Authorization cho Ingredient, Step, Image endpoints`<br>  - Nhánh Frontend: `2331375 refactor: gom TS types cua module recipe content va media ve src/types/recipe-content.ts`<br><br>**Chưa hoàn thành:** Không có. | [2312731_NguyenPhuQuy_Recipe-details_backend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312731_NguyenPhuQuy_Recipe-details_backend) | 100% |

---

## Chi tiết Minh chứng Triển khai

### 1. Cấu hình Rate Limiting Upload (5 req/phút/IP)
- **Tập tin nguồn:** `backend/src/CulinaryBlog.API/RateLimiting/UploadRateLimiting.cs`
- **Mã lỗi trả về khi vi phạm:**
  ```json
  {
    "type": "RATE_LIMIT_EXCEEDED",
    "title": "Too Many Requests",
    "status": 429,
    "detail": "Bạn đã gửi quá 5 yêu cầu tải lên tệp tin mỗi phút. Vui lòng thử lại sau.",
    "instance": "/api/v1/recipes/a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d/images"
  }
  ```
- **Header phản hồi:** `Retry-After: 60`

### 2. Cấu hình Authorization & Phân quyền Tác giả
- Toàn bộ các hành vi thay đổi dữ liệu (POST, PUT, DELETE, PATCH, reorder) tại:
  - `RecipeIngredientEndpoints.cs`
  - `RecipeStepEndpoints.cs`
  - `RecipeImageEndpoints.cs`
  đều yêu cầu Bearer JWT hợp lệ (`.RequireAuthorization()`).
- Bắt lỗi khi người dùng lạ thao tác trên công thức không thuộc sở hữu:
  ```json
  {
    "type": "FORBIDDEN",
    "title": "Forbidden",
    "status": 403,
    "detail": "Bạn không có quyền chỉnh sửa nội dung công thức của tác giả khác."
  }
  ```

### 3. Chuẩn hóa TypeScript Types tại `src/types/`
- **Đường dẫn:** `frontend/src/types/recipe-content.ts`
- Cung cấp đầy đủ interface cho 10 API phân hệ, đảm bảo typesafe 100% giữa API Client và UI Components (`RecipeWizardEditor.tsx`).
- Re-export sạch qua `frontend/src/types/index.ts`.

### 4. Kết quả Kiểm thử
- **Backend Unit Tests:** `247 Passed, 0 Failed, 0 Skipped` (Duration: 1s).
- **Frontend Build:** `Compiled successfully in 1.9s`, 14 routes statically optimized.
