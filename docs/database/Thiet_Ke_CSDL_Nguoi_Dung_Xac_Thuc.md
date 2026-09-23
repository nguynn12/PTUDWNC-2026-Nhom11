# Thiết kế Cơ sở Dữ liệu — Module Xác thực, Quản lý Người dùng & Phân quyền

**Dự án:** Culinary Blog — Nhóm 11, CTK47B
**Phạm vi:** FR-AUTH-001 → FR-AUTH-010 (SRS.md mục 3.1) và mô hình dữ liệu mục 7.7–7.8
**Phiên bản:** 1.0.0 — 22/09/2026
**Người thiết kế:** Luyen (phụ trách module Quản lý người dùng/Đăng nhập/Phân quyền)

---

## 1. Phạm vi và nguyên tắc thiết kế

Module này chịu trách nhiệm cho toàn bộ vòng đời tài khoản: đăng ký, đăng nhập (email/mật khẩu + Google OAuth), duy trì phiên bằng JWT + Refresh Token Rotation, quản lý hồ sơ cá nhân, xác nhận email, và các thao tác quản trị (Admin khoá/mở tài khoản).

Ba nguyên tắc bắt buộc tuân thủ theo SRS.md và tài liệu kiến thức nền tảng của dự án:

1. **Không tự chế bảng User/Role** — dùng **ASP.NET Core Identity** làm nền (bảng `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `AspNetUserLogins`, `AspNetUserClaims`, `AspNetUserTokens` sinh tự động bởi `IdentityDbContext`), chỉ mở rộng `ApplicationUser` với field nghiệp vụ riêng.
2. **Không lưu raw refresh token** — chỉ lưu SHA-256 hash (`TokenHash`), theo đúng SRS 7.8.
3. **Không dùng Repository/Unit of Work** — Handler dùng thẳng `IApplicationDbContext`/`UserManager<ApplicationUser>` (quy ước riêng của dự án, khác với ví dụ Repository/UoW trong giáo trình Chương 3 — xem `docs/references/Kien_Thuc_Nen_Tang_Bai_Giang.md` Phụ lục D).

## 2. Sơ đồ quan hệ thực thể (ERD)

```
┌─────────────────────────┐        1        N ┌──────────────────────────┐
│   AspNetRoles            │◄──────────────────┤   AspNetUserRoles         │
│  (Admin, Author)         │                    │  (UserId, RoleId)        │
└─────────────────────────┘                    └────────────┬─────────────┘
                                                              │ N
                                                              ▼ 1
┌───────────────────────────────────────────────────────────────────────┐
│                          AspNetUsers (ApplicationUser)                 │
│  Id (PK, string 450) · Email · NormalizedEmail · UserName             │
│  PasswordHash · SecurityStamp · ConcurrencyStamp                       │
│  EmailConfirmed · LockoutEnd · LockoutEnabled · AccessFailedCount      │
│  ── Field mở rộng ──                                                    │
│  DisplayName (100) · AvatarUrl (500)? · Bio (text)?                    │
│  IsActive (bool, default true) · CreatedAt                             │
└───────────────────────────┬─────────────────────────────┬─────────────┘
                             │ 1                            │ 1
                             ▼ N                             ▼ N
              ┌──────────────────────────┐    ┌──────────────────────────┐
              │       RefreshTokens       │    │    AspNetUserLogins       │
              │  Id (PK, uuid)            │    │  (Google OAuth — external  │
              │  UserId (FK)              │    │   login, sinh tự động     │
              │  FamilyId (indexed)       │    │   bởi Identity)           │
              │  TokenHash (unique,64)    │    └──────────────────────────┘
              │  ExpiresAt · RevokedAt?   │
              │  ReplacedByTokenHash?     │
              │  RevocationReason?        │
              │  CreatedAt/ByIp · RevokedByIp │
              └──────────────────────────┘
```

`AspNetUserClaims`, `AspNetRoleClaims`, `AspNetUserTokens` được Identity sinh tự động (dùng cho token xác nhận email/reset mật khẩu qua `DataProtectorTokenProvider`), không cần thiết kế thêm.

## 3. Chi tiết từng bảng

### 3.1 AspNetUsers (`ApplicationUser`)

| Cột | Kiểu | Ràng buộc | Ghi chú |
|---|---|---|---|
| Id | varchar(450) | PK | Identity tự sinh (GUID dạng string) |
| Email / NormalizedEmail | varchar(256) | unique (normalized) | FR-AUTH-001: duy nhất, case-insensitive |
| UserName / NormalizedUserName | varchar(256) | unique (normalized) | Nội bộ = email đã normalize |
| PasswordHash | text | nullable | PBKDF2-HMAC-SHA512, 600k vòng (mặc định Identity) |
| SecurityStamp, ConcurrencyStamp | text | — | Chuẩn Identity |
| EmailConfirmed | bool | default false | Gate cho Publish Recipe (FR-AUTH-008) |
| LockoutEnd | timestamptz | nullable | FR-AUTH-002 A3: khoá 15 phút sau 5 lần sai |
| LockoutEnabled | bool | default true | |
| AccessFailedCount | int | default 0 | |
| **DisplayName** | varchar(100) | **not null** | FR-AUTH-006/007 |
| **AvatarUrl** | varchar(500) | nullable | FR-AUTH-007: phải là URL hợp lệ (validate ở Application layer) |
| **Bio** | text | nullable, ≤1000 ký tự (validate ở Application layer) | FR-AUTH-007 |
| **IsActive** | bool | not null, default true | FR-AUTH-010 — Admin vô hiệu hoá chủ động |
| **CreatedAt** | timestamptz | not null | |

**Index bổ sung**: `IsActive` (Admin lọc danh sách user theo trạng thái).

> Lưu ý: `ApplicationUser` **không** kế thừa `BaseEntity` (không có `IsDeleted`/`DeletedAt` — đúng theo SRS mục 7.1: "ApplicationUser và RefreshToken không bắt buộc kế thừa base entity"). Vô hiệu hoá tài khoản dùng `IsActive`, không xoá mềm.

### 3.2 AspNetRoles

Seed cố định 2 role qua `HasData()` (GUID cố định, idempotent):

| Role | Id (cố định) | Gán khi nào |
|---|---|---|
| `Author` | `20000000-0000-0000-0000-000000000002` | Tự động khi đăng ký (FR-AUTH-001) |
| `Admin` | `20000000-0000-0000-0000-000000000001` | Thủ công qua database seeding |

`Guest` **không** phải một role trong DB — là trạng thái chưa xác thực (không có JWT / không có bản ghi `AspNetUsers`).

### 3.3 RefreshTokens (bảng tuỳ biến, không thuộc Identity)

| Cột | Kiểu | Ràng buộc |
|---|---|---|
| Id | uuid | PK |
| UserId | varchar(450) | FK → AspNetUsers.Id, `ON DELETE CASCADE` |
| FamilyId | uuid | not null, indexed |
| TokenHash | char(64) | not null, **unique** (SHA-256 hex) |
| ExpiresAt | timestamptz | not null |
| RevokedAt | timestamptz | nullable |
| ReplacedByTokenHash | char(64) | nullable |
| RevocationReason | varchar(100) | nullable |
| CreatedAt | timestamptz | not null |
| CreatedByIp | varchar(45) | nullable (đủ cho IPv6) |
| RevokedByIp | varchar(45) | nullable |

**Cờ suy ra (không map cột)**: `IsRevoked = RevokedAt != null`, `IsExpired = UtcNow >= ExpiresAt`, `IsActive = !IsRevoked && !IsExpired`.

**Index**: `TokenHash` (unique, tra cứu khi refresh), `FamilyId` (revoke cả family khi phát hiện reuse), `UserId` (revoke toàn bộ token của 1 user — FR-AUTH-010), `(UserId, RevokedAt)` composite (lọc token còn hiệu lực).

**Cơ chế Token Rotation** (FR-AUTH-002/004):

```
Login lần đầu  → RefreshToken.CreateNewFamily()   [FamilyId = Id của chính nó]
Mỗi lần refresh → RefreshToken.CreateRotated(familyId cũ)
                 → token cũ Revoke(reason: "rotated", replacedByTokenHash: hash mới)
Nếu token đã Revoked bị dùng lại (reuse) → revoke TOÀN BỘ token cùng FamilyId,
                                             reason: "reuse-detected"
Logout          → Revoke(reason: "logout")   [idempotent — luôn trả 204]
Admin deactivate → Revoke tất cả token của user, reason: "admin-deactivated"
```

## 4. Mô hình phân quyền (3 tầng — SRS mục 2.3)

| Tầng | Cơ chế | Áp dụng trong module này |
|---|---|---|
| **Role-Based** | JWT claim `role` | `Admin` toàn quyền; `Author` quyền trên tài nguyên của chính mình |
| **Resource-Based** | So `AuthorId == currentUserId` | Không thuộc phạm vi bảng User (áp dụng cho Recipe — xem module FR-RECIPE), nhưng cơ chế **Admin bypass** cần implement chung ở Authorization Handler |
| **Policy-Based** | `VerifiedAuthor` policy | Yêu cầu `EmailConfirmed = true` HOẶC role Admin — dùng field `EmailConfirmed` có sẵn từ Identity, không cần cột mới |

Hằng số role/policy đã tạo tại `CulinaryBlog.Domain.Constants.Roles` và `Policies` để tránh magic string khi viết `RequireRole(Roles.Admin)` / `RequireAuthorization(Policies.VerifiedAuthor)` ở Presentation layer sau này.

## 5. Cấu hình Identity (Password/Lockout Policy)

Theo đúng FR-AUTH-001/002, cấu hình trong `AddInfrastructure()`:

- Mật khẩu: tối thiểu 8 ký tự, bắt buộc hoa + thường + số + ký tự đặc biệt.
- Lockout: 5 lần sai → khoá 15 phút (`LockoutOptions`).
- Email duy nhất bắt buộc (`RequireUniqueEmail = true`).
- **Không** bật `RequireConfirmedEmail` toàn cục — FR-AUTH-002 cho phép login dù email chưa xác nhận; việc chặn tính năng (publish recipe) xử lý riêng qua policy `VerifiedAuthor`.
- Dùng `AddIdentityCore<ApplicationUser>()` (không phải `AddIdentity` đầy đủ) vì hệ thống dùng JWT thuần túy, không cần `SignInManager`/cookie authentication scheme dành cho MVC — giúp Infrastructure project không cần `FrameworkReference` tới `Microsoft.AspNetCore.App`.

## 6. Danh sách file đã hiện thực hoá

| File | Layer | Nội dung |
|---|---|---|
| `Infrastructure/Identity/ApplicationUser.cs` | Infrastructure | Entity mở rộng `IdentityUser`, factory `Create()`/`CreateFromGoogle()` — chuyển từ Domain sang ngày 2026-09-23 (RESOLVED-CONFLICTS D7) |
| `Domain/Entities/RefreshToken.cs` | Domain | Entity token rotation, factory `CreateNewFamily()`/`CreateRotated()`, `Revoke()` — chỉ giữ `UserId`, không navigation `User` (D7) |
| `Domain/Constants/Roles.cs` | Domain | Hằng số `Admin`, `Author` |
| `Domain/Constants/Policies.cs` | Domain | Hằng số `VerifiedAuthor` |
| `Infrastructure/Persistence/Configurations/ApplicationUserConfiguration.cs` | Infrastructure | Fluent API cho field mở rộng |
| `Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs` | Infrastructure | Fluent API đầy đủ bảng RefreshTokens + index |
| `Infrastructure/Persistence/Configurations/RoleConfiguration.cs` | Infrastructure | Seed 2 role cố định |
| `Infrastructure/Persistence/CulinaryBlogDbContext.cs` | Infrastructure | Đổi sang `IdentityDbContext<ApplicationUser, IdentityRole, string>` |
| `Application/Common/Interfaces/IApplicationDbContext.cs` | Application | Thêm `DbSet<RefreshToken> RefreshTokens` |
| `Infrastructure/DependencyInjection.cs` | Infrastructure | `AddIdentityCore` + policy mật khẩu/lockout |
| `Directory.Packages.props`, 3 file `.csproj` | — | Thêm `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore`. Từ 2026-09-23 `Domain.csproj` không còn `Microsoft.Extensions.Identity.Stores` (D7) |
| `Application/Common/Interfaces/IUserQueryService.cs`, `Infrastructure/Identity/UserQueryService.cs` | Application / Infrastructure | Lấy tên/ảnh tác giả theo danh sách `AuthorId` — thay cho navigation `Recipe.Author` (D7) |

## 7. Server database (Docker) — đã có sẵn trong repo

Repo đã đóng gói PostgreSQL bằng Docker Compose từ ADR-0001 (Sprint 0), không cần tạo lại:

| File | Vai trò |
|---|---|
| `docker-compose.yml` | Service `postgres` (image `postgres:16-alpine`, port `5432`) + `pgadmin` (port `5050`) |
| `.env.example` / `.env` | Biến môi trường: `POSTGRES_DB=culinary_blog_dev`, `POSTGRES_USER=culinary_blog`, `POSTGRES_PASSWORD=culinary_blog_dev` (`.env` đã được tạo sẵn từ `.env.example`, nằm trong `.gitignore` — không commit) |
| `backend/src/CulinaryBlog.API/appsettings.Development.json` | `ConnectionStrings:DefaultConnection` đã khớp sẵn với `.env` ở trên |
| `infrastructure/pgadmin/servers.json` | pgAdmin tự động có sẵn kết nối tới server "Culinary Blog PostgreSQL" khi mở `http://localhost:5050` |

**Khởi động server**:

```bash
cd PTUDWNC-2026-Nhom11
docker compose up -d postgres pgadmin
docker compose ps          # xác nhận cả 2 container đều "healthy"/"running"
```

pgAdmin: mở `http://localhost:5050`, đăng nhập bằng `PGADMIN_DEFAULT_EMAIL`/`PGADMIN_DEFAULT_PASSWORD` trong `.env` (mặc định `admin@culinaryblog.com` / `culinary_blog_admin`) — server "Culinary Blog PostgreSQL" đã tự động xuất hiện trong danh sách, chỉ cần nhập password của `culinary_blog` user khi được hỏi (không lưu sẵn vì lý do bảo mật).

## 8. Query tạo bảng database

File: **`backend/scripts/sql/001_init_identity_schema.sql`** — tạo đầy đủ 8 bảng (`AspNetRoles`, `AspNetUsers` mở rộng, `AspNetRoleClaims`, `AspNetUserClaims`, `AspNetUserLogins`, `AspNetUserRoles`, `AspNetUserTokens`, `RefreshTokens`) cùng toàn bộ index/FK, và seed sẵn 2 role `Admin`/`Author` với GUID cố định khớp `RoleConfiguration.cs`. Nội dung SQL khớp 1:1 với các Entity + Fluent API Configuration đã tạo ở mục 6 (kể cả kiểu `varchar(450)` cho khoá chính, đã đồng bộ hoá giữa C# và SQL).

**Cách chạy** (sau khi `docker compose up -d postgres` đã healthy):

```bash
cd PTUDWNC-2026-Nhom11
docker compose exec -T postgres psql -U culinary_blog -d culinary_blog_dev \
    < backend/scripts/sql/001_init_identity_schema.sql
```

Hoặc qua giao diện: mở pgAdmin → server "Culinary Blog PostgreSQL" → database `culinary_blog_dev` → **Query Tool** → mở file `001_init_identity_schema.sql` → **Execute (F5)**.

**Kiểm tra kết quả** sau khi chạy:

```bash
docker compose exec postgres psql -U culinary_blog -d culinary_blog_dev -c "\dt"
docker compose exec postgres psql -U culinary_blog -d culinary_blog_dev -c 'SELECT "Name" FROM "AspNetRoles";'
```

Kết quả mong đợi: `\dt` liệt kê đủ 8 bảng ở trên; câu SELECT trả về đúng 2 dòng `Admin` và `Author`.

> ⚠️ Script dùng `CREATE TABLE IF NOT EXISTS` và `ON CONFLICT DO NOTHING` nên chạy lại nhiều lần không lỗi (idempotent), nhưng đây là cách bootstrap **thủ công** — script không tự ghi vào bảng `__EFMigrationsHistory` của EF Core. Xem cảnh báo ở mục 9.

## 9. Các bước tiếp theo

1. **Ưu tiên**: khi đã cài được `dotnet-ef` (`dotnet tool install --global dotnet-ef`), nên chuyển hẳn sang EF Core Migrations làm nguồn chân lý duy nhất cho schema — đặc biệt từ khi thêm bảng `Recipe`/`Category` về sau, vì maintain 2 nguồn (SQL tay + migration) song song rất dễ lệch nhau. Cách chuyển tiếp gọn nhất: xoá database dev hiện tại (`docker compose down -v` rồi `up -d postgres` lại — mất dữ liệu dev, chấp nhận được ở giai đoạn này), sau đó chạy migration chuẩn:
   ```bash
   cd backend
   dotnet ef migrations add InitialIdentitySchema \
     --project src/CulinaryBlog.Infrastructure \
     --startup-project src/CulinaryBlog.API \
     --output-dir Persistence/Migrations
   dotnet ef database update \
     --project src/CulinaryBlog.Infrastructure \
     --startup-project src/CulinaryBlog.API
   ```
   Nếu muốn GIỮ dữ liệu đã tạo bằng SQL tay thay vì xoá: sau khi `dotnet ef migrations add` tạo ra file migration, đánh dấu migration đó "đã áp dụng" mà không chạy lại DDL bằng `dotnet ef database update --connection ... ` kết hợp chèn tay 1 dòng vào bảng `__EFMigrationsHistory` — phức tạp hơn, chỉ nên làm nếu dữ liệu dev cần giữ lại.
2. **Chưa nằm trong phạm vi "thiết kế database"** (bước triển khai API sau): JWT Bearer authentication scheme (`AddAuthentication().AddJwtBearer()`), `IJwtService`/`JwtService`, các `AuthorizationPolicy` cụ thể (`AdminOnly`, `VerifiedAuthor` Handler), seed tài khoản Admin đầu tiên (cần `UserManager.CreateAsync` để hash mật khẩu đúng chuẩn PBKDF2 — không thể tạo bằng SQL tay), và các CQRS Command/Query (`RegisterCommand`, `LoginCommand`, `RefreshTokenCommand`, ...) cho FR-AUTH-001→010.

---

*Tài liệu này là phần bổ sung chi tiết cho SRS.md mục 7.7–7.8 (vốn chỉ mô tả ở mức khung), dùng làm căn cứ khi triển khai và khi bảo vệ báo cáo module Quản lý người dùng/Đăng nhập/Phân quyền.*
