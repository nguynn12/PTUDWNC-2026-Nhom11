# PTUDWNC-2026-Nhom11 - Culinary Blog

Dự án nhóm môn **Phát triển Ứng dụng Web Nâng cao**, xây dựng nền tảng blog ẩm thực và chia sẻ công thức nấu ăn.

## Phân công bài tập Lab - Buổi 1

**Mục tiêu:** Đọc và phân tích tài liệu SRS, tìm hiểu các chức năng cần xây dựng, xác định mô hình dữ liệu và các mối quan hệ chính của hệ thống; đồng thời rà soát các điểm chưa rõ hoặc mâu thuẫn trong đặc tả để chuẩn bị cho giai đoạn triển khai.

| Liêng Hót Ha Luyến | Trần Quốc Quân | Tạ Nhật Nguyên | Nguyễn Phú Quý |
|---|---|---|---|
| Phân tích nhóm chức năng **Tài khoản và xác thực**. | Phân tích nhóm chức năng **Danh mục và tra cứu công thức**. | Phân tích nhóm chức năng **Vòng đời công thức**. | Phân tích nhóm chức năng **Nội dung chi tiết và hình ảnh công thức**. |
| Xác định các dữ liệu chính liên quan đến `ApplicationUser`, `RefreshToken` và vai trò người dùng. | Xác định các dữ liệu chính liên quan đến `Category` và các yêu cầu tìm kiếm, lọc, sắp xếp Recipe. | Xác định các dữ liệu chính liên quan đến `Recipe`, trạng thái Recipe, thông tin dinh dưỡng và lịch sử Slug. | Xác định các dữ liệu chính liên quan đến `RecipeIngredient`, `RecipeStep`, `RecipeImage`. |
| Phân tích mối quan hệ giữa User và Recipe; đối chiếu các yêu cầu xác thực, phân quyền trong SRS. | Phân tích mối quan hệ giữa Category và Recipe; đối chiếu các yêu cầu truy vấn dữ liệu trong SRS. | Phân tích quan hệ giữa Recipe với User, Category và các dữ liệu con; xác định các quy tắc lifecycle của Recipe. | Phân tích quan hệ giữa Recipe với Ingredient, Step và Image; xác định các ràng buộc dữ liệu cần thiết. |
| Ghi nhận các điểm chưa rõ hoặc mâu thuẫn liên quan đến Identity, Role và cấu trúc User. | Ghi nhận các điểm chưa rõ hoặc mâu thuẫn liên quan đến Category và Search. | Ghi nhận các điểm chưa rõ hoặc mâu thuẫn liên quan đến Recipe, concurrency, slug và lifecycle. | Ghi nhận các điểm chưa rõ hoặc mâu thuẫn liên quan đến Ingredient, Step, Image và File Storage. |

### Công việc chung của cả nhóm

- Đọc và rà soát toàn bộ tài liệu SRS.
- Thống nhất cách phân chia các nhóm chức năng giữa 4 thành viên.
- Xác định các Entity chính và mối quan hệ giữa các Entity.
- Đối chiếu yêu cầu chức năng với Data Model và API được mô tả trong SRS.
- Tổng hợp các điểm mâu thuẫn, chưa rõ hoặc cần nhóm thống nhất trước khi code.
- Tìm hiểu kiến trúc **Clean Architecture** và cấu trúc các project `Domain`, `Application`, `Infrastructure`, `API`.
- Chuẩn bị môi trường phát triển gồm .NET SDK, Git, VS Code và các công cụ cần thiết cho những buổi tiếp theo.

## Phân công bài tập Lab - Buổi 2

**Mục tiêu:** Bắt đầu triển khai hệ thống từ tài liệu SRS đã phân tích ở Buổi 1. Hoàn thiện cấu trúc Backend theo Clean Architecture, xây dựng mô hình dữ liệu, tạo Migration và tạo cơ sở dữ liệu PostgreSQL có dữ liệu mẫu theo yêu cầu Lab 2.

| Liêng Hót Ha Luyến | Trần Quốc Quân | Tạ Nhật Nguyên | Nguyễn Phú Quý |
|---|---|---|---|
| Kiểm tra và hoàn thiện cấu trúc các project `Domain`, `Application`, `Infrastructure`, `API`; kiểm tra project reference đúng theo Clean Architecture. | Tạo entity `Category` với đầy đủ các thuộc tính theo SRS. | Tạo entity `Recipe` với đầy đủ các thuộc tính chính theo SRS. | Tạo entity `RecipeIngredient` với đầy đủ các thuộc tính và quan hệ với `Recipe`. |
| Cài đặt các package dùng chung: EF Core, Npgsql PostgreSQL, Identity EF Core và Bogus. | Tạo lớp `CategoryConfiguration` để cấu hình tên bảng, kiểu dữ liệu, độ dài trường, index và unique constraint. | Tạo các enum `RecipeStatus`, `RecipeDifficulty`, `NutritionSource`. | Tạo entity `RecipeStep` với `StepNumber`, `Title`, `Description`, `TimerMinutes`, `ImageUrl`. |
| Tạo lớp `ApplicationUser` và các thuộc tính mở rộng của người dùng. | Cấu hình quan hệ `Category 1-N Recipe`. | Tạo `RecipeNutrition` và cấu hình dữ liệu dinh dưỡng của Recipe. | Tạo entity `RecipeImage` với các thuộc tính URL, `AltText`, `IsPrimary`, `OrderIndex`. |
| Tạo entity `RefreshToken` và cấu hình quan hệ với `ApplicationUser`. | Cấu hình index cho `Name`, `Slug` và các ràng buộc liên quan đến soft delete của Category. | Tạo entity `RecipeSlugHistory` để lưu lịch sử Slug của Recipe. | Tạo `RecipeIngredientConfiguration`, `RecipeStepConfiguration`, `RecipeImageConfiguration`. |
| Cấu hình `ApplicationUser`, `RefreshToken` và các ràng buộc liên quan đến Identity. | Tạo `CategorySeeder` bằng Bogus, sinh tối thiểu **20 Categories**. | Tạo `RecipeConfiguration` để cấu hình kiểu dữ liệu, index, soft delete, `xmin` và các quan hệ chính của Recipe. | Cấu hình các quan hệ `Recipe 1-N Ingredient`, `Recipe 1-N Step`, `Recipe 1-N Image`. |
| Tạo dữ liệu mẫu cho tài khoản `Author` và `Admin`. | Kiểm tra các Category sinh ra không trùng Slug và có dữ liệu hợp lệ. | Tạo `RecipeSeeder` bằng Bogus, sinh tối thiểu **100 Recipes**. | Tạo lớp sinh dữ liệu mẫu Ingredient, đảm bảo mỗi Recipe có ít nhất **10 Ingredients**. |
| Kiểm tra quan hệ `ApplicationUser -> Recipe` và `ApplicationUser -> RefreshToken`. | Kiểm tra Recipe được gán đúng Category sau khi tích hợp dữ liệu. | Trong `RecipeSeeder`, gán `CategoryId`, `AuthorId`, trạng thái, độ khó và dữ liệu dinh dưỡng hợp lệ. | Tạo lớp sinh dữ liệu mẫu Step, đảm bảo mỗi Recipe có ít nhất **5 Steps**. |
| Phối hợp hoàn thiện `CulinaryBlogDbContext` và đăng ký các entity liên quan đến User. | Đăng ký `DbSet<Category>` và kiểm tra Configuration được EF Core nhận đúng. | Đăng ký `DbSet<Recipe>` và `DbSet<RecipeSlugHistory>`; kiểm tra Configuration được EF Core nhận đúng. | Đăng ký các `DbSet` của Ingredient, Step, Image và kiểm tra Configuration. |
| Kiểm tra build và hỗ trợ xử lý lỗi Identity/PostgreSQL khi merge. | Kiểm tra build và xử lý lỗi liên quan đến Category khi merge. | Kiểm tra build và xử lý lỗi liên quan đến Recipe khi merge. | Kiểm tra build và xử lý lỗi liên quan đến Recipe Content khi merge. |

### Công việc chung của cả nhóm

- Merge mã nguồn của 4 thành viên vào nhánh `develop`.
- Review các Entity và Configuration trước khi tạo Migration.
- Hoàn thiện lớp `CulinaryBlogDbContext`.
- Kiểm tra toàn bộ Primary Key, Foreign Key, quan hệ, index và constraint.
- Tạo Migration đầu tiên cho toàn bộ database.
- Chạy `database update` để tạo database PostgreSQL.
- Chạy toàn bộ Seeder.
- Kiểm tra database có tối thiểu:
  - **20 Categories**
  - **100 Recipes**
  - **1.000 RecipeIngredients**
  - **500 RecipeSteps**
- Kiểm tra mỗi Recipe có:
  - ít nhất **10 Ingredients**
  - ít nhất **5 Steps**
  - `CategoryId` hợp lệ
  - `AuthorId` hợp lệ
- Chạy `dotnet build` và đảm bảo project không có lỗi trước khi kết thúc Lab 2.

## Phân công bài tập Lab - Buổi 3

**Mục tiêu:** Triển khai các chức năng Backend chính dựa trên mô hình dữ liệu đã hoàn thành ở Buổi 2. Mỗi thành viên phụ trách một module xuyên suốt từ xử lý nghiệp vụ đến API. Cả nhóm phối hợp hoàn thiện Domain Exceptions, Repository, Unit of Work và middleware xử lý lỗi toàn cục.

### Phân công theo thành viên

| Hạng mục | Liêng Hót Ha Luyến | Trần Quốc Quân | Tạ Nhật Nguyên | Nguyễn Phú Quý |
|---|---|---|---|---|
| **Module phụ trách** | Tài khoản và xác thực | Category và Recipe Discovery | Recipe Lifecycle | Recipe Content và Media |
| **Command, Validator và Handler** | Register, Login, Refresh Token, Logout và xác thực Email | Create, Update, Delete Category; Category List, Category Detail, Recipe List, Recipe Detail và Search | Create, Update, Publish, Unpublish, Archive, Unarchive và Delete Recipe | Add, Update, Delete Ingredient; Add, Update, Delete Recipe Step; quản lý Recipe Image |
| **Nghiệp vụ và truy vấn** | Cài đặt `IdentityService`; tạo JWT Access Token và quản lý Refresh Token | Cài đặt phân trang, lọc, sắp xếp và PostgreSQL Full-Text Search cho Recipe | Kiểm tra quyền sở hữu; xử lý quy tắc chuyển trạng thái; cập nhật `RecipeSlugHistory`; kiểm soát concurrency bằng `xmin` và ETag/`If-Match` | Tích hợp MinIO; quản lý ảnh chính `IsPrimary`, thứ tự `OrderIndex` và liên kết ảnh với Recipe |
| **Minimal API endpoints** | Tạo endpoints cho chức năng tài khoản và xác thực. Tối thiểu 2 endpoints, gồm Register và Login | Tạo endpoints cho Category, Recipe List, Recipe Detail và Search. Tối thiểu 2 endpoints | Tạo endpoints cho Recipe Lifecycle. Tối thiểu 2 endpoints | Tạo endpoints cho Ingredient, Step và Image. Tối thiểu 2 endpoints |
| **Kiểm tra chức năng** | Kiểm tra luồng Register → Login → Access Token → Refresh Token → API có phân quyền; kiểm tra token sai/hết hạn và tài khoản không hợp lệ | Kiểm tra Category, Recipe List/Detail, phân trang, lọc, sắp xếp và Full-Text Search bằng dữ liệu PostgreSQL | Kiểm tra Recipe không tồn tại, sai chủ sở hữu, sai trạng thái và concurrency conflict | Kiểm tra thêm/sửa/xóa Ingredient, Step; upload ảnh và liên kết dữ liệu với Recipe |

### Công việc chung của cả nhóm

| Hạng mục | Công việc cần hoàn thành |
|---|---|
| **Tích hợp và cấu trúc** | Đồng bộ mã nguồn Database từ Buổi 2; thống nhất cấu trúc `Domain`, `Application`, `Infrastructure`, `API`; kiểm tra PostgreSQL sau khi tích hợp |
| **Domain Exceptions** | Cài đặt các lớp Domain Exceptions cần thiết trong `Domain`; thống nhất exception cho lỗi nghiệp vụ và dữ liệu |
| **Repository và Unit of Work** | Khai báo interface; cài đặt Repository và Unit of Work trong `Infrastructure`; đăng ký Dependency Injection; bảo đảm các thao tác cần tính nhất quán dùng transaction phù hợp |
| **Middleware xử lý lỗi** | Tạo middleware toàn cục trong `API` để bắt exception, ánh xạ sang HTTP status code phù hợp và trả RFC 7807 Problem Details; không đưa stack trace hoặc thông tin nhạy cảm vào response |
| **Tích hợp Backend** | Thống nhất định dạng Request, Response và mã lỗi; cấu hình MediatR, FluentValidation, ASP.NET Core Identity, JWT Authentication và các package EF Core/PostgreSQL cần thiết |
| **Kiểm thử và review** | Kiểm tra API bằng Swagger, HTTP Client hoặc Postman; review chéo trước khi merge vào `develop`; chạy `dotnet build` và sửa lỗi sau tích hợp |

### Kết quả cần đạt cuối Buổi 3

| Nhóm chức năng | Kết quả cần đạt |
|---|---|
| **Yêu cầu tối thiểu của Lab** | Hoàn thành Domain Exceptions; Repository và Unit of Work; mỗi thành viên có ít nhất 2 API endpoints; middleware toàn cục trả RFC 7807 Problem Details |
| **Tài khoản và phân quyền** | Register, Login, JWT Access Token, Refresh Token và Authorization theo `Author`/`Admin` hoạt động |
| **Category và Discovery** | API Category, Recipe List, Recipe Detail và Search hoạt động; phân trang, lọc, sắp xếp và Full-Text Search được kiểm tra |
| **Recipe Lifecycle** | Create, Update, Publish, Unpublish, Archive, Unarchive và Delete hoạt động; kiểm tra quyền sở hữu và concurrency |
| **Recipe Content và Media** | API Ingredient, Recipe Step, Recipe Image hoạt động; upload file được tích hợp với MinIO |
| **Tích hợp và build** | Các API chính được kiểm thử; toàn bộ Backend build thành công sau khi merge |

## Phân công bài tập Lab - Buổi 4

**Mục tiêu:** Hoàn thiện toàn bộ hệ thống 41 REST API Backend theo chuẩn đặc tả SRS Chương 8. Áp dụng Clean Architecture, CQRS với MediatR, FluentValidation, tối ưu hóa truy vấn PostgreSQL, MinIO Storage, phân quyền đa tầng và kiểm soát đồng thời (Optimistic Concurrency với `xmin`/`If-Match`).

### Phân công theo thành viên

| Hạng mục | Liêng Hót Ha Luyến (2312682) | Trần Quốc Quân (2312726) | Tạ Nhật Nguyên (2312704) | Nguyễn Phú Quý (2312731) |
|---|---|---|---|---|
| **Phân hệ phụ trách** | Xác thực & Quản lý Người dùng | Danh mục, Khám phá Công thức & Giám sát | Vòng đời Công thức & Thùng rác Quản trị | Nội dung Chi tiết Công thức & Media Storage |
| **Số lượng API** | **10 Endpoints** | **10 Endpoints** | **11 Endpoints** | **10 Endpoints** |
| **Danh sách API chính** | • `POST /api/v1/auth/register`<br>• `POST /api/v1/auth/login`<br>• `POST /api/v1/auth/google`<br>• `POST /api/v1/auth/refresh`<br>• `POST /api/v1/auth/logout`<br>• `POST /api/v1/auth/email/confirm`<br>• `POST /api/v1/auth/email/resend`<br>• `GET /api/v1/auth/me`<br>• `PATCH /api/v1/auth/me`<br>• `PATCH /api/v1/admin/users/{id}/status` | • `GET /api/v1/categories`<br>• `GET /api/v1/categories/{slug}`<br>• `POST /api/v1/categories`<br>• `PUT /api/v1/categories/{id}`<br>• `DELETE /api/v1/categories/{id}`<br>• `GET /api/v1/recipes`<br>• `GET /api/v1/recipes/search`<br>• `GET /api/v1/recipes/{slug}`<br>• `GET /health`<br>• `GET /health/live` | • `POST /api/v1/recipes`<br>• `PUT /api/v1/recipes/{id}`<br>• `PATCH /api/v1/recipes/{id}/publish`<br>• `PATCH /api/v1/recipes/{id}/unpublish`<br>• `PATCH /api/v1/recipes/{id}/archive`<br>• `PATCH /api/v1/recipes/{id}/unarchive`<br>• `DELETE /api/v1/recipes/{id}`<br>• `GET /api/v1/admin/recipes/trash`<br>• `POST /api/v1/admin/recipes/{id}/restore`<br>• `DELETE /api/v1/admin/recipes/{id}/purge`<br>• `GET /health/ready` | • `POST /api/v1/recipes/{id}/images`<br>• `PATCH /api/v1/recipes/{id}/images/{imageId}`<br>• `DELETE /api/v1/recipes/{id}/images/{imageId}`<br>• `POST /api/v1/recipes/{id}/ingredients`<br>• `PUT /api/v1/recipes/{id}/ingredients/{id}`<br>• `DELETE /api/v1/recipes/{id}/ingredients/{id}`<br>• `POST /api/v1/recipes/{id}/steps`<br>• `PUT /api/v1/recipes/{id}/steps/{id}`<br>• `PUT /api/v1/recipes/{id}/steps/reorder`<br>• `DELETE /api/v1/recipes/{id}/steps/{id}` |
| **Kỹ thuật chuyên sâu** | Token Rotation & Reuse Detection; Google ID Token OAuth2; Hash mật khẩu chuẩn Identity; Quản lý Lockout trạng thái user | PostgreSQL FTS với unaccent & GIN Index; Phân trang & lọc đa tiêu chí; ETag & Cache; Liveness Health Check | Optimistic Concurrency Control qua `If-Match` và `xmin`; Validate xuất bản công thức nghiêm ngặt; Thùng rác Soft Delete / Restore / Hard Delete; Readiness Probe | Upload file MinIO Storage với giới hạn 5MB và format JPEG/PNG/WebP/AVIF; Reorder các bước nấu trong Transaction nguyên tử |

### Công việc chung của cả nhóm

- Thống nhất và tuân thủ định dạng chuẩn RFC 7807 Problem Details cho toàn bộ 41 API endpoints.
- Chuẩn hóa format trả về (ApiResponse bọc dữ liệu kèm metadata phân trang PagedMeta).
- Tích hợp tài liệu tương tác qua OpenAPI và Scalar UI tại `/scalar/v1`.
- Chạy kiểm thử toàn diện 41 API endpoints qua Postman và REST Client, kiểm soát mã trạng thái HTTP chuẩn (200, 201, 204, 400, 401, 403, 404, 409, 422).
- Đảm bảo toàn bộ project Backend build thành công (`0 Error, 0 Warning`).

## Phân công bài tập Lab - Buổi 5

**Mục tiêu:** Thiết lập bảo mật Backend nâng cao (CORS allowlist, Rate Limiting đa chính sách, Authorization bảo vệ toàn bộ API); Tích hợp API Versioning và tài liệu OpenAPI hoàn chỉnh; Khởi tạo môi trường và cấu trúc dự án Frontend Next.js 16 App Router với TypeScript; Chuẩn hóa tập trung hệ thống Types, dọn dẹp các thư mục thừa.

### Phân công theo thành viên

| Hạng mục | Liêng Hót Ha Luyến (2312682) | Trần Quốc Quân (2312726) | Tạ Nhật Nguyên (2312704) | Nguyễn Phú Quý (2312731) |
|---|---|---|---|---|
| **Bảo mật & Phân quyền** | Cấu hình CORS đọc từ `appsettings.json` (chặn wildcard); Rà soát `JwtBearerSetup` (thời hạn token, ClockSkew, Issuer/Audience); Bảo vệ endpoint `/overview` | Cài đặt Rate Limiting chung (100 req/phút/IP) áp dụng cho `/api/v1`; Gắn `RequireAuthorization("Admin")` cho `POST/PUT/DELETE /categories` | Gắn `RequireAuthorization("Author")` và policy `VerifiedAuthor` cho toàn bộ nhóm Recipe Lifecycle; Gắn `RequireAuthorization("Admin")` cho nhóm `/admin/recipes` | Cài đặt Rate Limiting upload (5 req/phút/IP) cho `POST /recipes/{id}/images`; Gắn `RequireAuthorization("Author")` cho Ingredient, Step, Image |
| **API Versioning & Tài liệu** | Cài đặt `Asp.Versioning.Http`, chuyển `MapGroup` sang `/api/v{version:apiVersion}`; Thêm security scheme Bearer vào OpenAPI Document để Scalar có nút Authorize | Kiểm tra và xử lý lỗi trả về 429 RFC 7807 với mã `RATE_LIMIT_EXCEEDED`, headers `X-RateLimit-*` và `Retry-After` | Loại bỏ nhánh bỏ qua kiểm tra sở hữu khi UserId rỗng trong các Handlers; Bổ sung `ConcurrencyToken` vào 5 Command Lifecycle | Chuyển đổi và nâng cấp `FileStorageService` sang MinIO Storage; Bổ sung `FileValidationHelper` kiểm tra magic bytes |
| **Cấu trúc & Types Frontend** | Tập trung hóa toàn bộ types của Auth và `ApiResponse`, `ProblemDetails` vào `src/types/`; Bổ sung biến môi trường vào `.env.example` | Gom `RecipeSummaryDto`, `CategoryDto`, `PagedMeta` về `src/types/`; Thống nhất thư mục `components/recipe` | Gom `RecipeStatus`, `RecipeDifficulty`, `RecipeNutritionDto`, `RecipeFullDetailDto` về `src/types/`, xóa bản khai báo trùng | Gom types cho `RecipeIngredientDto`, `RecipeStepDto`, `RecipeImageDto` về `src/types/`; Dọn dẹp loại bỏ thư mục `src/features/` |
| **Kiểm thử tích hợp** | Kiểm thử luồng xác thực: token hết hạn trả 401, refresh token thu hồi, bảo vệ route thành công | Khởi tạo project `CulinaryBlog.IntegrationTests`, viết test cho Categories, Recipes, Rate Limiting 429 | Viết integration test luồng chuyển đổi trạng thái: Publish → Unpublish → Archive → Delete; kiểm thử `If-Match` sai trả 409 | Kiểm thử upload ảnh vượt quá dung lượng cho phép hoặc sai định dạng; kiểm thử rate limit upload lần thứ 6 trả 429 |

### Công việc chung của cả nhóm

- Thực hiện kiểm tra toàn diện chất lượng mã nguồn: `npm run typecheck`, `npm run lint`, `npm run build` trên Frontend và `dotnet build`, `dotnet test` trên Backend đều đạt 0 lỗi.
- Đảm bảo mọi endpoint nghiệp vụ nhạy cảm đều được bảo vệ chặt chẽ bởi đúng quyền (`Author`, `VerifiedAuthor`, `Admin`).
- Thống nhất cấu trúc dự án Frontend App Router phẳng, gọn gàng, định kiểu tĩnh mạnh mẽ bằng TypeScript.

## Phân công bài tập Lab - Buổi 6

**Mục tiêu:** Hiện đại hóa toàn diện giao diện UI/UX Frontend với Tailwind CSS v4 và thư viện thành phần shadcn/ui Base Nova; Cài đặt lớp tiện ích xử lý lỗi (`AppError`, `getErrorMessage`), hợp nhất HTTP Client duy nhất; Chuẩn hóa State Management với Zustand; Xây dựng thư viện component dùng chung (Loading, ErrorState, EmptyState, Pagination, Sonner Toast, Dialogs); Thiết lập Layout đa tầng và Route Boundaries (`loading.tsx`, `error.tsx`, `not-found.tsx`).

### Phân công theo thành viên

| Hạng mục | Liêng Hót Ha Luyến (2312682) | Trần Quốc Quân (2312726) | Tạ Nhật Nguyên (2312704) | Nguyễn Phú Quý (2312731) |
|---|---|---|---|---|
| **Hạ tầng & Tiện ích UI** | Hợp nhất 4 client gọi API thành 1 client duy nhất (`lib/api/axios.ts`) kèm auto-refresh token; Xây dựng `lib/errors.ts` (`AppError`, `getErrorMessage` Việt hóa); Tạo `lib/utils.ts` (`cn`, format ngày/thời gian/số) | Tích hợp Tailwind CSS v4 và khởi tạo shadcn/ui (`components.json`); Ánh xạ màu thương hiệu HSL cam đất (`#c85a17`) và xanh lá (`#2b7a4b`); Cài đặt các base component shadcn/ui | Xây dựng bộ component trạng thái dùng chung: `Loading.tsx`, `PageLoader.tsx`, `ErrorState.tsx`, `EmptyState.tsx`; Thay thế modal/toast cũ bằng `sonner` và shadcn `alert-dialog` | Thiết kế lại component `RecipeCard` bằng Card + Badge; Xây dựng `RecipeFilterSidebar` (Select, Checkbox), `RecipeToolbar` (sắp xếp), `RecipeListWrapper` |
| **Quản lý trạng thái (Store)** | Chuẩn hóa store Zustand: `store/auth-store.ts` (quản lý session, tokens, user profile), `store/ui-store.ts`, `store/auth-ui-store.ts` | Tái cấu trúc Layout chung (`Navbar`, `Footer`, Mobile Sheet Drawer, Container, PageHeader) | Đồng bộ trạng thái UI trong Dashboard và Admin; Giữ nguyên signature của các component dùng chung để không làm vỡ các module liên quan | Quản lý form state cho các bước trong Multi-step Wizard (`RecipeWizardEditor`) bằng React Hook Form & Zod |
| **Giao diện Màn hình UI** | Tái thiết kế giao diện tĩnh nhóm Auth bằng shadcn/ui: `/auth/login`, `/auth/register`, `/auth/forgot-password`, `/auth/reset-password`, `/profile`, `UserMenu` | Thiết kế giao diện Trang chủ (`app/page.tsx`), Danh mục công thức (`/categories`), Trang tìm kiếm (`/search`) | Thiết kế giao diện Dashboard quản lý bài viết (`/dashboard/recipes`) và Quản trị thùng rác (`/admin/recipes/trash`); Thêm Route Boundaries `app/loading.tsx`, `app/error.tsx`, `app/global-error.tsx` | Thiết kế lại trang Chi tiết công thức `RecipeDetail` (ảnh, khẩu phần, thời gian, nguyên liệu, các bước làm, bảng dinh dưỡng); Tái thiết kế Form tạo/sửa công thức Wizard |
| **Tối ưu hóa & Responsive** | Xử lý redirect trang Auth; Validate form với React Hook Form và Zod; Hiển thị thông báo lỗi thân thiện | Tối ưu hóa SEO với JSON-LD và Metadata; Kiểm tra hiển thị responsive toàn bộ các trang công khai | Kiểm tra kịch bản mất kết nối Backend hiển thị `ErrorState` có nút Thử lại; Xử lý trạng thái danh sách rỗng với `EmptyState` | Loại bỏ toàn bộ `style={{...}}` inline chuyển sang class Tailwind/shadcn; Kiểm tra responsive 3 độ phân giải (375px di động, 768px tablet, 1280px desktop) |

### Công việc chung của cả nhóm

- Hợp nhất toàn bộ các nhánh tính năng của 4 thành viên vào nhánh `develop`.
- Đồng bộ giao diện toàn bộ ứng dụng theo hệ thống Design System chung (bảng màu, typography, khoảng cách, bo góc).
- Đảm bảo 100% các Route Boundaries xử lý trơn tru mọi tình huống dữ liệu đang tải, rỗng hoặc có lỗi phát sinh.
- Chạy kiểm thử tự động toàn bộ 29 endpoints/routes đạt 100% kết quả PASS.
- Kiểm tra toàn bộ mã nguồn đạt chuẩn nghiêm ngặt: `npm run typecheck`, `npm run lint`, `npm run build` trên Frontend và `dotnet build` trên Backend đạt 0 lỗi, 0 cảnh báo.

### Bảng tổng hợp phân công xuyên suốt 6 Buổi Lab

| Thành viên | MSSV | Vai trò chính & Phân hệ xuyên suốt (Backend ➔ Frontend) | Nhánh Git chính |
|---|---|---|---|
| **Liêng Hót Ha Luyến** | 2312682 | **Phân hệ Xác thực & Quản lý Người dùng**: Auth APIs, JWT/Refresh Token, Profile, Security CORS/Auth, API Client, Error Handling, Auth Pages & UI | `2312682_LiengHotHaLuyen_Auth_backend`<br>`2312682_LiengHotHaLuyen_Auth_frontend` |
| **Trần Quốc Quân** | 2312726 | **Phân hệ Danh mục, Khám phá & Giám sát**: Category CRUD, PostgreSQL FTS, Rate Limiting, Integration Tests, Tailwind v4, shadcn/ui, Layout & Public Pages | `2312726_TranQuocQuan_Category_Search_backend`<br>`2312726_TranQuocQuan_Category_Search_frontend` |
| **Tạ Nhật Nguyên** | 2312704 | **Phân hệ Vòng đời Công thức, Thùng rác & UI States**: Recipe Lifecycle, Concurrency `xmin`/`If-Match`, Trash/Purge, Route Boundaries, Common Components (Loading/Error/Empty), Dashboard & Admin Pages | `2312704_TaNhatNguyen_Recipe-lifecycle_backend`<br>`2312704_TaNhatNguyen_Recipe-lifecycle_frontend` |
| **Nguyễn Phú Quý** | 2312731 | **Phân hệ Nội dung Chi tiết, Media Storage & Form Wizard**: Recipe Content (Ingredients/Steps), MinIO Storage, RecipeDetail UI, RecipeWizardEditor Multi-step Form, Responsive Design | `2312731_NguyenPhuQuy_Recipe-details_backend`<br>`2312731_NguyenPhuQuy_Recipe-details_frontend` |

## Công nghệ sử dụng

- Backend: .NET 10 Minimal API, Entity Framework Core và PostgreSQL.
- Frontend: Next.js App Router, React và TypeScript.
- Database: PostgreSQL 16 chạy bằng Docker.
- Quản trị database: pgAdmin 4 chạy bằng Docker.
- Kiến trúc backend: Clean Architecture.

## Yêu cầu môi trường

Mỗi thành viên cần cài đặt:

- Git.
- Docker Desktop với Docker Compose.
- .NET SDK 10.0.100 hoặc phiên bản tương thích trong dòng 10.0.x.
- Node.js 22 và npm 10 trở lên.

Kiểm tra phiên bản:

```powershell
git --version
docker --version
docker compose version
dotnet --version
node --version
npm --version
```

## Khởi động dự án lần đầu

### 1. Lấy mã nguồn

```powershell
git clone https://github.com/nguynn12/PTUDWNC-2026-Nhom11.git
cd PTUDWNC-2026-Nhom11
```

Nếu đã clone repository:

```powershell
git pull origin main
```

### 2. Tạo cấu hình môi trường

```powershell
Copy-Item .env.example .env
```

File `.env` chỉ dùng trên máy cá nhân và không được commit lên Git.

### 3. Khởi động PostgreSQL và pgAdmin

```powershell
docker compose up -d
docker compose ps
```

Các container cần có trạng thái `Up` hoặc `Healthy`:

- `culinary-blog-postgres`
- `culinary-blog-pgadmin`

Xem log khi cần:

```powershell
docker compose logs -f postgres
docker compose logs -f pgadmin
```

### 4. Chạy backend

Mở terminal mới tại thư mục gốc:

```powershell
cd backend
dotnet restore
dotnet run --project src/CulinaryBlog.API
```

Kiểm tra backend:

- API: <http://localhost:5000/api/v1>
- Health check: <http://localhost:5000/health>
- Database health: <http://localhost:5000/health/database>

### 5. Chạy frontend

Mở terminal khác tại thư mục gốc:

```powershell
cd frontend
npm ci
npm run dev
```

Truy cập frontend tại <http://localhost:3000>.

## Sử dụng pgAdmin 4

Truy cập <http://localhost:5050> và đăng nhập:

| Thuộc tính | Giá trị development mặc định |
|---|---|
| Email pgAdmin | `admin@culinaryblog.com` |
| Password pgAdmin | `culinary_blog_admin` |

Kết nối `Development/Culinary Blog PostgreSQL` được nạp sẵn. Khi pgAdmin yêu cầu mật khẩu PostgreSQL, nhập:

```text
culinary_blog_dev
```

Thông tin PostgreSQL mặc định:

| Thuộc tính | Giá trị |
|---|---|
| Host từ máy cá nhân | `localhost` |
| Host bên trong Docker | `postgres` |
| Port | `5432` |
| Database | `culinary_blog_dev` |
| Username | `culinary_blog` |
| Password | `culinary_blog_dev` |

Các giá trị trên có thể thay đổi trong file `.env`.

## Các lệnh Docker thường dùng

```powershell
docker compose up -d             # Khởi động database và pgAdmin
docker compose ps                # Xem trạng thái container
docker compose logs -f           # Theo dõi log tất cả dịch vụ
docker compose restart           # Khởi động lại dịch vụ
docker compose stop              # Dừng nhưng giữ container và dữ liệu
docker compose down              # Xóa container/network nhưng giữ volume dữ liệu
```

Không chạy `docker compose down -v` nếu chưa chủ động muốn xóa toàn bộ dữ liệu development.

## Cấu trúc thư mục

```text
PTUDWNC-2026-Nhom11/
├── backend/
│   ├── src/
│   │   ├── CulinaryBlog.Domain/          # Entity, value object và business rule
│   │   ├── CulinaryBlog.Application/     # Use case, interface, command/query và DTO
│   │   ├── CulinaryBlog.Infrastructure/  # EF Core, PostgreSQL và service implementation
│   │   └── CulinaryBlog.API/             # Minimal API, middleware và cấu hình ứng dụng
│   ├── tests/
│   │   └── CulinaryBlog.UnitTests/       # Unit test backend
│   ├── CulinaryBlog.slnx
│   ├── Directory.Build.props
│   └── Directory.Packages.props
├── frontend/
│   ├── src/
│   │   ├── app/                          # Next.js routes và layouts
│   │   ├── components/                   # Component giao diện dùng chung và shadcn/ui
│   │   ├── hooks/                        # Custom React hooks
│   │   ├── lib/                          # API client, utilities và auth helper
│   │   ├── store/                        # Quản lý trạng thái Zustand
│   │   └── types/                        # TypeScript types tập trung
│   ├── package.json
│   └── package-lock.json
├── infrastructure/
│   └── pgadmin/
│       └── servers.json                  # Kết nối PostgreSQL nạp sẵn cho pgAdmin
├── docs/
│   ├── adr/                              # Architecture Decision Records
│   ├── architecture/                     # Tài liệu kiến trúc
│   └── SETUP.md                          # Hướng dẫn thiết lập chi tiết
├── .env.example                          # Mẫu biến môi trường
├── .gitignore
├── docker-compose.yml                    # PostgreSQL và pgAdmin
├── global.json                           # Phiên bản .NET SDK
└── README.md
```

## Nguyên tắc phụ thuộc backend

```text
API -> Application
API -> Infrastructure
Infrastructure -> Application -> Domain
```

- `Domain` không phụ thuộc project khác.
- `Application` chỉ phụ thuộc `Domain`.
- `Infrastructure` hiện thực các interface của `Application`.
- `API` là điểm khởi động và cấu hình dependency injection.

## Quy ước làm việc với Git

Tạo nhánh cá nhân hoặc nhánh tính năng từ `main`:

```powershell
git switch main
git pull origin main
git switch -c <ma-sinh-vien>/<ten-ngan>
```

Ví dụ:

```powershell
git switch -c 2312682/ha_luyen
```

Tên nhánh tính năng có thể dùng dạng:

```text
feature/<module>-<ten-tinh-nang>
fix/<ten-loi>
```

Không commit các file sau:

- `.env` hoặc secret.
- `node_modules`, `.next`.
- `bin`, `obj`.
- Log và dữ liệu runtime.

Migration database phải được review trước khi merge để tránh xung đột schema giữa các thành viên.

## Kiểm tra dự án

Backend:

```powershell
dotnet build backend/CulinaryBlog.slnx
dotnet test backend/CulinaryBlog.slnx
```

Frontend:

```powershell
npm --prefix frontend run typecheck
npm --prefix frontend run lint
npm --prefix frontend run build
```