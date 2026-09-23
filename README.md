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

**Mục tiêu:** Hoàn thiện các chức năng Backend chính của hệ thống sau khi đã hoàn thành mô hình dữ liệu ở Buổi 2. Mỗi thành viên tiếp tục phụ trách module đã được phân công, triển khai từ tầng `Application`, `Infrastructure` đến `API`. Đồng thời hoàn thiện Repository, truy vấn dữ liệu và PostgreSQL Full-Text Search theo nội dung Lab 3.

| Liêng Hót Ha Luyến | Trần Quốc Quân | Tạ Nhật Nguyên | Nguyễn Phú Quý |
|---|---|---|---|
| Hoàn thiện Backend cho **Tài khoản và xác thực**. | Hoàn thiện Backend cho **Category và Recipe Discovery**. | Hoàn thiện Backend cho **Recipe Lifecycle**. | Hoàn thiện Backend cho **Recipe Content và Media**. |
| Tạo Command, Validator và Handler cho chức năng **Register**, **Login**, **Refresh Token**, **Logout** và xác thực Email. | Tạo Command, Validator và Handler cho các chức năng **Create, Update, Delete Category**. | Tạo Command, Validator và Handler cho **Create Recipe** và **Update Recipe**. | Tạo Command, Validator và Handler cho **Add, Update, Delete Ingredient**. |
| Cài đặt các chức năng quản lý tài khoản còn lại theo SRS như Forgot/Reset Password và thông tin tài khoản. | Tạo Query và Handler cho **Category List**, **Category Detail** và các truy vấn Category cần thiết. | Tạo Command, Validator và Handler cho **Publish**, **Unpublish**, **Archive**, **Unarchive** và **Delete Recipe**. | Tạo Command, Validator và Handler cho **Add, Update, Delete Recipe Step**. |
| Cài đặt `IdentityService` để xử lý các nghiệp vụ liên quan đến ASP.NET Core Identity. | Tạo Query và Handler cho **Recipe List** và **Recipe Detail**. | Cài đặt kiểm tra **quyền sở hữu Recipe** trước khi cho phép Author chỉnh sửa hoặc thay đổi trạng thái Recipe. | Tạo Command, Validator và Handler cho các chức năng quản lý **Recipe Image**. |
| Cài đặt service tạo **JWT Access Token** và quản lý **Refresh Token**; xử lý phân quyền `Author` và `Admin`. | Cài đặt chức năng **phân trang, lọc và sắp xếp Recipe** theo Category, trạng thái và các tiêu chí được hỗ trợ. | Cài đặt các quy tắc chuyển trạng thái `Draft`, `Published`, `Archived` của Recipe theo SRS. | Cài đặt service lưu trữ file và tích hợp **MinIO** để upload hình ảnh Recipe. |
| Tạo các Minimal API Endpoint cho Register, Login, Refresh Token, Logout và các chức năng Account được giao. | Cài đặt Repository/truy vấn đọc Recipe phục vụ Category và Recipe Discovery. | Cài đặt xử lý `RecipeSlugHistory` khi Slug của Recipe thay đổi theo quy tắc đã thống nhất. | Cài đặt logic quản lý metadata của hình ảnh và liên kết hình ảnh với Recipe. |
| Cấu hình Authentication và Authorization cho các API yêu cầu đăng nhập hoặc Role cụ thể. | Cài đặt PostgreSQL **Full-Text Search** cho Recipe và sử dụng `SearchVector`/index đã thiết kế để thực hiện tìm kiếm. | Cài đặt kiểm soát cập nhật đồng thời Recipe bằng PostgreSQL `xmin` và ETag/`If-Match`. | Cài đặt các quy tắc liên quan đến ảnh chính (`IsPrimary`) và thứ tự hình ảnh (`OrderIndex`). |
| Kiểm tra luồng **Register → Login → Access Token → Refresh Token → API có phân quyền**. | Tạo Minimal API Endpoint cho Category, Recipe List, Recipe Detail và Search. | Tạo Minimal API Endpoint cho Create, Update, Publish, Unpublish, Archive, Unarchive và Delete Recipe. | Tạo Minimal API Endpoint cho Ingredient, Step và Image. |
| Kiểm tra các trường hợp sai token, token hết hạn, không đủ quyền và tài khoản không hợp lệ. | Kiểm tra Recipe List, Detail, Pagination, Filter, Sort và Full-Text Search bằng dữ liệu thật trong PostgreSQL. | Kiểm tra các trường hợp Recipe không tồn tại, sai chủ sở hữu, sai trạng thái và xảy ra concurrency conflict. | Kiểm tra thêm/sửa/xóa Ingredient, Step, upload ảnh và bảo đảm dữ liệu được liên kết đúng với Recipe. |

### Công việc chung của cả nhóm

- Merge và đồng bộ toàn bộ mã nguồn Database của Buổi 2 vào nhánh `develop`.
- Chạy Migration và kiểm tra database PostgreSQL sau khi tích hợp code của 4 thành viên.
- Thống nhất cấu trúc code Backend theo các tầng:
  - `Domain`
  - `Application`
  - `Infrastructure`
  - `API`
- Thống nhất cách tổ chức feature gồm:
  - Command / Query
  - Handler
  - Validator
  - DTO / Request / Response
  - Repository / Service khi cần
  - Minimal API Endpoint
- Cài đặt và kiểm tra các package Backend cần sử dụng trong Buổi 3 như:
  - MediatR
  - FluentValidation
  - ASP.NET Core Identity
  - JWT Authentication
  - MinIO
  - các package hỗ trợ PostgreSQL/EF Core cần thiết.
- Hoàn thiện đăng ký Dependency Injection cho các service, repository và thành phần Application/Infrastructure.
- Cấu hình Authentication và Authorization dùng chung cho hệ thống.
- Hoàn thiện Repository và các truy vấn dữ liệu phục vụ nội dung Lab 3.
- Hoàn thiện PostgreSQL Full-Text Search và kiểm tra tìm kiếm trên dữ liệu Recipe thực tế.
- Thống nhất định dạng Request, Response và lỗi trả về từ API.
- Xử lý các lỗi cơ bản như:
  - dữ liệu không hợp lệ;
  - không tìm thấy dữ liệu;
  - chưa đăng nhập;
  - không đủ quyền;
  - xung đột dữ liệu.
- Kiểm tra từng API bằng Swagger, HTTP Client hoặc Postman trước khi merge.
- Review code chéo giữa các thành viên trước khi merge vào `develop`.
- Chạy `dotnet build` và đảm bảo Backend không có lỗi biên dịch sau khi merge.

### Kết quả cần đạt cuối Buổi 3

- API đăng ký, đăng nhập và xác thực hoạt động.
- JWT Access Token và Refresh Token hoạt động.
- Authorization theo `Author` / `Admin` hoạt động.
- API quản lý Category hoạt động.
- API Recipe List và Recipe Detail hoạt động.
- Pagination, Filter và Sort Recipe hoạt động.
- PostgreSQL Full-Text Search hoạt động.
- API Create và Update Recipe hoạt động.
- API Publish / Unpublish / Archive / Unarchive Recipe hoạt động.
- Soft Delete Recipe hoạt động.
- Kiểm tra quyền sở hữu Recipe hoạt động.
- Kiểm soát concurrency của Recipe hoạt động.
- API quản lý Ingredient hoạt động.
- API quản lý Recipe Step hoạt động.
- API quản lý Recipe Image và upload file hoạt động.
- Các API chính được kiểm thử trước khi bắt đầu Frontend ở Buổi 4.

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