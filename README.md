# PTUDWNC-2026-Nhom11 - Culinary Blog

Dự án nhóm môn **Phát triển Ứng dụng Web Nâng cao**, xây dựng nền tảng blog ẩm thực và chia sẻ công thức nấu ăn.

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

## Phân công bài tập Lab - Buổi 1

**Mục tiêu:** Đọc tài liệu SRS, xác định cấu trúc dữ liệu và hoàn thành cơ sở dữ liệu ban đầu của hệ thống.

| Thành viên 1 | Thành viên 2 | Thành viên 3 | Thành viên 4 |
|---|---|---|---|
| Phân tích dữ liệu liên quan đến **người dùng và tài khoản**. | Phân tích dữ liệu liên quan đến **danh mục món ăn**. | Phân tích dữ liệu chính của **công thức**. | Phân tích dữ liệu chi tiết bên trong **công thức**. |
| Xây dựng bảng `ApplicationUser`. | Xây dựng bảng `Category`. | Xây dựng bảng `Recipe`. | Xây dựng bảng `RecipeIngredient`. |
| Xây dựng bảng `RefreshToken`. | Xác định mối liên hệ giữa `Category` và `Recipe`. | Xây dựng bảng `RecipeNutrition`. | Xây dựng bảng `RecipeStep`. |
| Xác định mối liên hệ giữa người dùng và công thức. | Kiểm tra các dữ liệu cần thiết để sau này tìm kiếm, lọc và sắp xếp công thức. | Liên kết `Recipe` với người dùng và danh mục. | Xây dựng bảng `RecipeImage` và liên kết với `Recipe`. |
| Chuẩn bị dữ liệu mẫu cho người dùng. | Chuẩn bị dữ liệu mẫu cho danh mục. | Chuẩn bị dữ liệu mẫu cho công thức. | Chuẩn bị dữ liệu mẫu cho nguyên liệu, bước nấu và hình ảnh. |

## Ghi chú hiện tại

Skeleton đã cấu hình kết nối PostgreSQL nhưng chưa tạo migration nghiệp vụ đầu tiên. Nhóm cần thống nhất entity nền tảng trước khi tạo và commit migration.
