# Backend

Backend dùng .NET 10 Minimal API và Clean Architecture.

## Dependency rule

```text
API -> Application
API -> Infrastructure
Infrastructure -> Application -> Domain
```

`Domain` không tham chiếu project khác. `Application` không tham chiếu `Infrastructure` hoặc `API`.

## Database

`CulinaryBlogDbContext` nằm trong Infrastructure. Khi bắt đầu thêm entity, tạo migration từ thư mục `backend`:

```bash
dotnet ef migrations add InitialCreate \
  --project src/CulinaryBlog.Infrastructure \
  --startup-project src/CulinaryBlog.API \
  --output-dir Persistence/Migrations

dotnet ef database update \
  --project src/CulinaryBlog.Infrastructure \
  --startup-project src/CulinaryBlog.API
```

Chưa tạo migration rỗng trong skeleton này. Migration đầu tiên nên được tạo sau khi nhóm chốt các entity nền tảng.

