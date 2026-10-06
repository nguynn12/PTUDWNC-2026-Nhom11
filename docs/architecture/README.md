# Kiến trúc dự án

## Runtime development

```text
Next.js (localhost:3000)
        |
        | HTTP/JSON
        v
.NET API (localhost:5000)
        |
        | Npgsql
        v
PostgreSQL 16 (Docker, localhost:5432)
```

PostgreSQL và công cụ quản trị pgAdmin được container hóa. Frontend và API chạy trực tiếp để có hot reload nhanh và dễ debug. pgAdmin truy cập tại `http://localhost:5050` và kết nối PostgreSQL bằng hostname nội bộ `postgres`.

## Module ownership

1. Identity & Profile.
2. Taxonomy & Discovery.
3. Recipe Lifecycle.
4. Recipe Composition & Media.

Mỗi module chịu trách nhiệm xuyên suốt từ giao diện đến test. Thành phần dùng chung cần pull request và ít nhất một reviewer ngoài module.

## Quyết định nền tảng

- Modular monolith, không dùng microservices trong phiên bản môn học.
- PostgreSQL 16 là nguồn dữ liệu chính.
- Entity Framework Core theo Code First.
- Truy cập dữ liệu (phía ghi/Command) qua **Repository + Unit of Work** theo quy ước chung của nhóm (yêu cầu Lab 3): `IRepository<T>` và `IUnitOfWork` ở `Application/Common/Interfaces`, hiện thực `EfRepository<T>` và `UnitOfWork` ở `Infrastructure/Persistence`. Module cần truy vấn riêng thì thêm interface kế thừa `IRepository<T>` (ví dụ `IRefreshTokenRepository` của module Auth). Phía đọc (Query) vẫn dùng `IApplicationDbContext`.
- Lỗi dùng exception có `ErrorCode` (SRS Phụ lục B): `Domain/Exceptions/DomainExceptions.cs` (`DomainException` và các lớp con) và `Application/Common/Exceptions` (`NotFoundException` → 404, `ConflictException` → 409, `ForbiddenException` → 403, `BusinessRuleValidationException` → 422). `ValidationBehavior` ném `FluentValidation.ValidationException` → 422. `API/Middlewares/GlobalExceptionHandler` chuyển thành RFC 7807; module Auth bổ sung `AuthExceptionHandler` (đăng ký trước) cho 401/423 và JSON sai cú pháp (400).
- Domain chỉ dùng .NET BCL, không có NuGet dependency (NFR-MAINT-004). `ApplicationUser` (kế thừa `IdentityUser`) đặt ở `Infrastructure/Identity`; entity trong Domain chỉ tham chiếu user qua `AuthorId`/`UserId`, lấy thông tin tác giả qua `IUserQueryService` — xem [`RESOLVED-CONFLICTS.md` D7](../decisions/RESOLVED-CONFLICTS.md).
- API version qua prefix `/api/v1`.
- Cấu hình local có giá trị development mặc định; secret thật không được commit.
- Migration được tạo sau khi chốt entity nền tảng, tránh migration rỗng hoặc schema giả định quá sớm.
- Mâu thuẫn phát hiện trong SRS và quyết định xử lý được ghi lại tại [`docs/decisions/`](../decisions/README.md) — đọc trước khi implement bất kỳ module nào.
