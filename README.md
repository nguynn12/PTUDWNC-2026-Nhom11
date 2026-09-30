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

**Mục tiêu:** Triển khai các chức năng Backend chính dựa trên mô hình dữ liệu đã hoàn thành ở Buổi 2. Mỗi thành viên phụ trách một module xuyên suốt từ tầng `Application`, `Infrastructure` đến `API`. Cả nhóm phối hợp hoàn thiện các yêu cầu tối thiểu của Lab 3: Domain Exceptions, Repository, Unit of Work, và middleware xử lý lỗi toàn cục theo chuẩn RFC 7807 Problem Details.

### Phân công theo thành viên

| Hạng mục | Liêng Hót Ha Luyến | Trần Quốc Quân | Tạ Nhật Nguyên | Nguyễn Phú Quý |
|---|---|---|---|---|
| **Module phụ trách** | **Tài khoản và xác thực** | **Category và Recipe Discovery** | **Recipe Lifecycle** | **Recipe Content và Media** |
| **Command, Validator và Handler** | Tạo Command, Validator và Handler cho **Register**, **Login**, **Refresh Token**, **Logout** và xác thực Email | Tạo Command, Validator và Handler cho **Create, Update, Delete Category**; Query và Handler cho **Category List/Detail**, **Recipe List/Detail** và **Search** | Tạo Command, Validator và Handler cho **Create**, **Update**, **Publish**, **Unpublish**, **Archive**, **Unarchive** và **Delete Recipe** | Tạo Command, Validator và Handler cho **Add, Update, Delete Ingredient**; **Add, Update, Delete Recipe Step**; quản lý **Recipe Image** |
| **Nghiệp vụ và truy vấn** | Cài đặt `IdentityService`; tạo JWT Access Token và quản lý Refresh Token; phân quyền `Author` và `Admin`; xử lý Forgot/Reset Password | Cài đặt phân trang (Pagination), lọc (Filter), sắp xếp (Sort) đa tiêu chí và PostgreSQL **Full-Text Search** cho Recipe qua `SearchVector` và trigger | Kiểm tra quyền sở hữu tác giả (`AuthorId`); xử lý quy tắc chuyển trạng thái vòng đời; slug bất biến sau xuất bản và lưu `RecipeSlugHistory`; kiểm soát concurrency bằng `xmin` và `If-Match` | Tích hợp **MinIO** lưu trữ file; quản lý metadata hình ảnh; ràng buộc ảnh chính (`IsPrimary`), thứ tự bước/ảnh (`OrderIndex`) và liên kết với Recipe |
| **Minimal API endpoints** *(≥ 2 endpoints/TV)* | Đăng ký Minimal API endpoints cho Tài khoản & Xác thực (tối thiểu 2 endpoints: `Register` và `Login`) | Đăng ký Minimal API endpoints cho Category, Recipe List, Recipe Detail và Search (tối thiểu 2 endpoints) | Đăng ký Minimal API endpoints cho Recipe Lifecycle tại `/api/v1/recipes` (tối thiểu 2 endpoints: `Create`, `Update`, `Publish`, `Delete`...) | Đăng ký Minimal API endpoints cho Ingredient, Recipe Step và Recipe Image (tối thiểu 2 endpoints) |
| **Kiểm tra chức năng & Unit Test** | Kiểm tra luồng Register → Login → JWT Access Token → Refresh Token → Authorization; kiểm tra token sai/hết hạn và tài khoản không hợp lệ | Kiểm tra Category CRUD, Recipe List/Detail, phân trang, lọc, sắp xếp và Full-Text Search bằng dữ liệu thực trong PostgreSQL | Kiểm tra các trường hợp Recipe không tồn tại (404), sai chủ sở hữu (403), sai trạng thái, vi phạm điều kiện publish (422) và concurrency conflict (409) | Kiểm tra thêm/sửa/xóa Ingredient, Step; upload hình ảnh lên MinIO và liên kết chính xác dữ liệu với Recipe |

### Công việc chung của cả nhóm

| Hạng mục | Công việc cần hoàn thành |
|---|---|
| **Tích hợp và cấu trúc** | Đồng bộ mã nguồn Database từ Buổi 2; thống nhất cấu trúc 4 tầng Clean Architecture (`Domain`, `Application`, `Infrastructure`, `API`); kiểm tra PostgreSQL sau khi tích hợp |
| **Domain Exceptions** *(Yêu cầu tối thiểu)* | Cài đặt các lớp Domain Exceptions cần thiết trong project `Domain` (`DomainException`, `EntityNotFoundException`, `OwnershipViolationException`, `RecipeIncompletePublishException`,...); thống nhất exception cho lỗi nghiệp vụ và dữ liệu |
| **Repository và Unit of Work** *(Yêu cầu tối thiểu)* | Khai báo interface `IRepository<T>`, `IUnitOfWork` trong `Application`; cài đặt generic `EfRepository<T>` và `UnitOfWork` trong `Infrastructure`; đăng ký DI; áp dụng cho toàn bộ thao tác ghi (Command), phía đọc (Query) dùng trực tiếp `IApplicationDbContext` theo CQRS |
| **Middleware xử lý lỗi** *(Yêu cầu tối thiểu)* | Cài đặt `GlobalExceptionHandler` toàn cục trong `API` theo chuẩn RFC 7807 Problem Details (`application/problem+json`), ánh xạ mã lỗi theo SRS Phụ lục B (`RECIPE_NOT_FOUND`, `RECIPE_FORBIDDEN`, `RECIPE_PUBLISH_INCOMPLETE`, `RECIPE_CONCURRENCY_CONFLICT`, `VALIDATION_ERROR`,...); không để lộ stack trace hoặc thông tin nhạy cảm |
| **Tích hợp Backend** | Thống nhất định dạng Request, Response và mã lỗi; cấu hình MediatR, FluentValidation pipeline (`ValidationBehavior`), ASP.NET Core Identity, JWT Authentication, MinIO và các package PostgreSQL/EF Core cần thiết |
| **Kiểm thử và review** | Kiểm tra từng API bằng Swagger, HTTP Client hoặc Postman; viết bộ Unit Tests kiểm thử tự động cho các module; review chéo code trước khi merge vào `develop`; chạy `dotnet build` và `dotnet test` đảm bảo Backend hoàn toàn sạch lỗi |

### Kết quả cần đạt cuối Buổi 3

| Nhóm chức năng | Kết quả cần đạt |
|---|---|
| **Yêu cầu tối thiểu của Lab** | Hoàn thành các lớp Domain Exceptions; cài đặt và sử dụng Generic Repository & Unit of Work; mỗi thành viên có ít nhất 2 Minimal API endpoints; middleware toàn cục trả chuẩn RFC 7807 Problem Details |
| **Tài khoản và phân quyền** | API Register, Login, Logout, Refresh Token hoạt động; JWT Access Token và Authorization theo role `Author`/`Admin` được bảo vệ chính xác |
| **Category và Discovery** | API Category CRUD, Recipe List, Recipe Detail và Search hoạt động; phân trang, lọc đa tiêu chí, sắp xếp và PostgreSQL Full-Text Search hoạt động trên dữ liệu thật |
| **Recipe Lifecycle** | API Create, Update, Publish, Unpublish, Archive, Unarchive, Soft Delete hoạt động; kiểm tra quyền tác giả, bất biến slug sau xuất bản, lưu `RecipeSlugHistory` và kiểm soát concurrency bằng `xmin`/`If-Match` |
| **Recipe Content và Media** | API Ingredient, Recipe Step, Recipe Image hoạt động; chức năng upload file tích hợp thành công với MinIO |
| **Tích hợp và build** | Toàn bộ API được kiểm thử; toàn bộ Backend build thành công (`0 Error`, `0 Warning`) và pass tất cả Unit Tests sau khi tích hợp |

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
│   │   ├── features/                     # Module nghiệp vụ theo lát cắt dọc
│   │   ├── components/                   # Component giao diện dùng chung
│   │   └── lib/                          # API client và tiện ích dùng chung
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