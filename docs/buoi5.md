## Phân công bài tập Lab - Buổi 5

**Yêu cầu tối thiểu của Lab 5 (giảng viên):**
1. Cài đặt bảo mật: CORS, Rate Limiting, Authentication/Authorization.
2. Sinh tài liệu đặc tả API, API versioning.
3. Cài đặt môi trường, xây dựng cấu trúc dự án Frontend.
4. Cài đặt các thư viện cần thiết.
5. Cài đặt các kiểu dữ liệu (TS types) dùng cho Frontend.

**Căn cứ:** Soát nhánh `develop` (đã merge Frontend buổi 4) đối chiếu SRS (`docs/requirements/SRS.md`). Đây là **sàn bắt buộc**; phần "Việc thêm" ở cuối mỗi người là việc nên làm để đúng tinh thần Lab 5 nhưng làm sau khi xong phần bắt buộc.

### Hiện trạng so với 5 yêu cầu của thầy

| # | Yêu cầu | Hiện trạng trên `develop` | Kết luận |
|---|---|---|---|
| 1a | CORS | Không có `AddCors`/`UseCors` ở đâu trong Backend. SRS: allowlist, không wildcard production | ❌ Chưa có |
| 1b | Rate Limiting | Chỉ có policy `Auth` (10 req/phút/IP). SRS còn yêu cầu **API chung 100/phút/IP** và **upload 5/phút/IP**, header `X-RateLimit-*`, mã `RATE_LIMIT_EXCEEDED` (429) | 🔶 Mới 1/3 |
| 1c | Authentication | JWT Bearer đã cấu hình (`AddJwtBearer`, `UseAuthentication`) | ✅ Có |
| 1d | Authorization | Policy `Admin`/`Author`/`VerifiedAuthor` đã định nghĩa, nhưng `RequireAuthorization` chỉ gắn ở **3 chỗ** (`/auth/me` GET/PATCH, nhóm `admin/users`). **Category CRUD, Recipe Lifecycle, `admin/recipes` (trash/restore/purge), Ingredient/Step/Image không có yêu cầu đăng nhập** → gọi ẩn danh được | ❌ Lỗ hổng chính |
| 2a | Sinh tài liệu API | `AddOpenApi` + `MapOpenApi` + Scalar UI đã có. Chưa khai báo security scheme Bearer (Scalar không có nút Authorize); `AddOpenApi()` bị gọi 2 lần (`Program.cs` và `API/DependencyInjection.cs`) | 🔶 Có, cần hoàn thiện |
| 2b | API versioning | Chỉ hard-code `MapGroup("/api/v1")`; không có package `Asp.Versioning` | 🔶 Đạt mức tối thiểu theo SRS (URL path), nên làm đúng chuẩn |
| 3 | Môi trường + cấu trúc Frontend | Cấu trúc `src/app, components, lib, hooks, store, types` đã có. `.env.example` thiếu `BACKEND_API_URL` (do `next.config.ts`, `lib/api/axios.ts`, `lib/api/health.ts` đọc); thư mục `features/` chỉ có README | 🔶 Gần đủ |
| 4 | Thư viện | Đã có `@tanstack/react-query`, `axios`, `react-hook-form`, `zod`, `zustand` | ✅ Đủ |
| 5 | TS types | Có 3 file (`types/api.ts`, `lib/types.ts`, `lib/auth/types.ts`) nhưng **khai báo trùng 8 tên**: `ApiResponse`, `RecipeDifficulty`, `RecipeStatus`, `RecipeImageDto`, `RecipeIngredientDto`, `RecipeNutritionDto`, `RecipeStepDto`, `RecipeSummaryDto` → dễ lệch nhau | 🔶 Có, cần gom |

### Liêng Hót Ha Luyến — CORS, Versioning, Tài liệu API, Auth + Types chung

| # | Việc cần làm | Trạng thái |
|---|---|---|
| 1 | CORS: thêm `AddCors` đọc danh sách origin từ `appsettings` (`Cors:AllowedOrigins`), dev cho `http://localhost:3000`, **không dùng `AllowAnyOrigin`**; expose header `ETag`, `Location`, `X-RateLimit-*`; gọi `app.UseCors(...)` sau `UseRateLimiter()`, trước `UseAuthentication()` | ☐ |
| 2 | API versioning: cài `Asp.Versioning.Http`, `AddApiVersioning` (đọc version từ URL segment), chuyển `MapGroup("/api/v1")` sang `/api/v{version:apiVersion}` với version `1.0`; đường dẫn `/api/v1/...` phải giữ nguyên để Frontend không vỡ | ☐ |
| 3 | Tài liệu API: bỏ lần gọi `AddOpenApi()` trùng; thêm document transformer khai báo title/version/description + security scheme `Bearer` để Scalar có nút Authorize; kiểm tra `/scalar` hiển thị đủ 4 nhóm endpoint | ☐ |
| 4 | Authorization: bảo vệ endpoint `/api/v1/overview` (hiện trả số liệu DB cho người ẩn danh) — chuyển sang Admin hoặc chỉ bật ở Development | ☐ |
| 5 | Rà `JwtBearerSetup` (thời hạn Access Token, `ClockSkew`, kiểm tra Issuer/Audience/Lifetime) khớp SRS 5.2 | ☐ |
| 6 | `.env.example`: bổ sung `BACKEND_API_URL`; ghi chú từng biến (biến nào Server, biến nào `NEXT_PUBLIC_`); cập nhật `frontend/README.md` hướng dẫn chạy | ☐ |
| 7 | TS types: gom `ApiResponse`, `ProblemDetails` và type Auth về **một nơi duy nhất** `src/types/`; xóa bản trùng ở `lib/types.ts` | ☐ |
| 8 | Test tay: không có token gọi `GET /auth/me` → 401; token hết hạn → 401 | ☐ |
| **Việc thêm** | Integration test Auth (register, login sai mật khẩu 401, refresh, logout idempotent 204, vượt rate limit → 429) | ☐ |

### Trần Quốc Quân — Rate Limiting chung, Authorization Category, Types Category/Recipe Summary, Integration Test chung

| # | Việc cần làm | Trạng thái |
|---|---|---|
| 1 | Rate Limiting chung: thêm policy 100 request/phút/IP áp cho nhóm `/api/v1` (trừ `/health`) | ☐ |
| 2 | Khi vượt giới hạn: trả 429 theo RFC 7807 với mã `RATE_LIMIT_EXCEEDED`, kèm header `Retry-After`; thêm `X-RateLimit-Limit`/`X-RateLimit-Remaining` vào response | ☐ |
| 3 | Authorization Category: `POST/PUT/DELETE /categories` → `RequireAuthorization("Admin")`; các `GET` giữ công khai | ☐ |
| 4 | Test tay/Scalar: không token → 401; token Author → 403; token Admin → 2xx | ☐ |
| 5 | TS types: gom `RecipeSummaryDto`, `CategoryDto` và type phân trang (`PagedMeta`) về `src/types/`, xóa bản trùng | ☐ |
| 6 | Gom/xóa thư mục trùng cấu trúc: quyết định dùng `components/recipe` hay `components/recipes` (hiện có cả hai), thống nhất một | ☐ |
| **Việc thêm** | Tạo project `CulinaryBlog.IntegrationTests` (`WebApplicationFactory<Program>`) + test `GET /categories`, `GET /recipes`, `GET /recipes/search`, test 429 của rate limit chung | ☐ |

### Tạ Nhật Nguyên — Authorization Recipe Lifecycle, Types Recipe, `If-Match`

| # | Việc cần làm | Trạng thái |
|---|---|---|
| 1 | Authorization: nhóm `/recipes` trong `RecipeLifecycleEndpoints.cs` (Create/Update/Delete) → `RequireAuthorization("Author")`; Publish/Unpublish → policy `VerifiedAuthor` (đúng SRS: Verified Owner/Admin) | ☑ |
| 2 | Authorization: nhóm `/admin/recipes` (trash, restore, purge) → `RequireAuthorization("Admin")` | ☑ |
| 3 | Bỏ nhánh "bỏ qua kiểm tra chủ sở hữu khi `UserId` rỗng" trong các Handler (hiện cho phép ẩn danh lọt qua) — nếu không có user thì phải ném lỗi 401/403 | ☑ |
| 4 | Test: không token → 401; Author khác chủ sở hữu → 403 `RECIPE_FORBIDDEN`; chủ sở hữu → 2xx | ☑ |
| 5 | TS types: gom `RecipeStatus`, `RecipeDifficulty`, `RecipeNutritionDto` và `RecipeFullDetailDto` về `src/types/`, xóa bản trùng ở `lib/types.ts`/`types/api.ts` | ☑ |
| 6 | Thêm `ConcurrencyToken` vào 5 Command (Publish, Unpublish, Archive, Unarchive, Delete) và so khớp `xmin`; Frontend đã gửi sẵn `If-Match` nên Backend cần kiểm tra thật | ☑ |
| **Việc thêm** | Integration test luồng `Publish → Unpublish → Archive → Unarchive → Delete`; `If-Match` sai → 409; thiếu Ingredient/Step → 422 `RECIPE_PUBLISH_INCOMPLETE` | ☑ |

### Nguyễn Phú Quý — Rate Limiting upload, Authorization Content & Media, Types Content, MinIO

| # | Việc cần làm | Trạng thái |
|---|---|---|
| 1 | Rate Limiting upload: policy 5 request/phút/IP, áp cho `POST /recipes/{id}/images` bằng `RequireRateLimiting` | ☐ |
| 2 | Authorization: `RecipeIngredientEndpoints`, `RecipeStepEndpoints`, `RecipeImageEndpoints` → `RequireAuthorization("Author")` + giữ kiểm tra chủ sở hữu/Admin | ☐ |
| 3 | Test: không token → 401; người khác sở hữu → 403; chủ sở hữu → 2xx; upload lần thứ 6 trong 1 phút → 429 | ☐ |
| 4 | TS types: gom `RecipeIngredientDto`, `RecipeStepDto`, `RecipeImageDto` và request type của Ingredient/Step/Image về `src/types/`, xóa bản trùng | ☐ |
| 5 | Dọn thư mục `src/features/` (hiện chỉ có README): quyết định giữ hay bỏ, ghi rõ vào `frontend/README.md` | ☐ |
| **Việc thêm** | Chuyển `FileStorageService` từ `wwwroot/uploads` sang MinIO (`AWSSDK.S3` theo quyết định đã chốt): thêm service `minio` vào `docker-compose.yml`, `MinioSettings`, viết lại `UploadAsync`/`DeleteAsync`, tạo bucket; giữ `FileValidationHelper` chạy trước khi upload; integration test upload/xóa ảnh thật | ☐ |

### Việc chung của cả nhóm (cuối buổi)

| # | Việc | Trạng thái |
|---|---|---|
| 1 | Mỗi người chạy `npm ci`, `npm run typecheck`, `npm run lint`, `npm run build` ở `frontend/` — phải sạch sau khi gom type | ☐ |
| 2 | `dotnet build` + `dotnet test` sạch sau khi thêm `RequireAuthorization` (một số unit test/endpoint cũ có thể cần gắn token giả) | ☐ |
| 3 | Frontend: các trang `dashboard`, `admin` đã gửi token (qua `setAuthToken`) → thử lại bằng UI thật sau khi Backend bắt đăng nhập, xác nhận không bị 401 oan | ☐ |
| 4 | Review chéo trước khi merge `develop` | ☐ |

### Kết quả cần đạt cuối Buổi 5

| Yêu cầu của thầy | Kết quả cần đạt |
|---|---|
| Bảo mật | CORS allowlist hoạt động; 3 policy rate limit (Auth 10, chung 100, upload 5) trả 429 đúng chuẩn; mọi endpoint ghi/xóa đều yêu cầu đăng nhập và đúng role |
| Tài liệu API + Versioning | `/scalar` hiển thị đủ endpoint, có nút Authorize Bearer; `Asp.Versioning` hoạt động, đường dẫn `/api/v1` không đổi |
| Môi trường + cấu trúc Frontend | `.env.example` đủ biến, `README` hướng dẫn chạy, cấu trúc thư mục thống nhất |
| Thư viện | `npm ci` và `dotnet restore` chạy sạch trên máy mới |
| TS types | Mỗi type chỉ khai báo ở một nơi (`src/types/`), không còn khai báo trùng |

### Lưu ý

- Thứ tự ưu tiên: **Luyến làm CORS + versioning trước**; **Nguyên, Quân, Quý thêm `RequireAuthorization`** song song vì mỗi người sửa file endpoint riêng, ít đụng nhau.
- Sau khi bắt đăng nhập, chạy lại toàn bộ Frontend một lượt: nếu thấy 401 ở trang công khai, kiểm tra endpoint `GET` công khai không bị gắn nhầm `RequireAuthorization`.
- Redis/Cache phân tán thuộc Lab 7, không làm ở buổi này.
