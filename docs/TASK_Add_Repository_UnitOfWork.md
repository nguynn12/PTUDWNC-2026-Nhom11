# Task: Bổ sung Repository & Unit of Work pattern

**Dành cho:** AI coding agent (Antigravity/Claude Code) — làm việc trên nhánh `develop` của repo `nguynn12/PTUDWNC-2026-Nhom11`.

## Bối cảnh — ĐỌC KỸ TRƯỚC KHI SỬA

Đề bài Lab (buổi 3) yêu cầu tối thiểu: *"Hoàn thành việc cài đặt các lớp repository & unit of work."* Đây là 1 mục checklist bắt buộc, không phải tùy chọn.

Hiện tại file `backend/src/CulinaryBlog.Application/Common/Interfaces/IApplicationDbContext.cs` có đoạn comment:
```
/// Handler dùng trực tiếp DbSet này (không qua Repository/UnitOfWork — quy ước dự án).
```
Đây là quyết định kiến trúc kiểu Jason Taylor Clean Architecture template (`DbContext` = Unit of Work sẵn có, `DbSet<T>` = Repository sẵn có, không cần viết thêm lớp bọc). Về kỹ thuật không sai, nhưng **vi phạm trực tiếp checklist chấm điểm** của đề bài — cần bổ sung 1 lớp Repository/UnitOfWork thật sự tồn tại và được dùng, không phá vỡ code đang có.

**Chưa có Command/Query Handler nào được viết trong dự án** (dự án mới xong tầng Database ở buổi 2) — nghĩa là đây là thời điểm tốt nhất để thêm pattern này, không có code cũ nào cần refactor.

## NGUYÊN TẮC BẮT BUỘC — không được vi phạm

1. **KHÔNG xóa hay sửa cách `IApplicationDbContext` hoạt động** — Query (đọc dữ liệu: list, detail, search, filter) vẫn tiếp tục dùng thẳng `IApplicationDbContext`/`DbSet<T>` như hiện tại. Chỉ thêm 1 con đường MỚI (`IRepository<T>`/`IUnitOfWork`) dành cho phía ghi dữ liệu (Command: Create/Update/Delete), không bắt buộc Query phải đổi qua Repository.
2. **KHÔNG cài thêm MediatR, FluentValidation, hay bất kỳ package nào khác** ngoài phạm vi task này — đó là việc của người khác trong nhóm (xem `buoi3.md`, mục "Tích hợp Backend"), không thuộc phạm vi task này.
3. **KHÔNG tạo Repository/UnitOfWork riêng cho từng entity** (không tạo `RecipeRepository`, `CategoryRepository` riêng lẻ) — dùng **1 lớp generic duy nhất** `EfRepository<T>` áp dụng cho mọi entity, đúng tinh thần DRY.
4. **KHÔNG tự ý đặt thêm phương thức nào khác** ngoài danh sách bên dưới, dù thấy "có vẻ nên có" (ví dụ `GetPagedAsync`, `FindAsync` với predicate...) — nếu nghĩ cần, liệt kê ra hỏi lại, đừng tự thêm.
5. Giữ đúng convention code đã có trong repo: **primary constructor** (C# 12+, ví dụ `public sealed class Foo(Bar bar)`), XML doc comment tiếng Việt trên mọi public interface/class, `sealed class` cho implementation cụ thể.

## Danh sách việc cần làm — theo đúng thứ tự

### 1. Tạo `backend/src/CulinaryBlog.Application/Common/Interfaces/IUnitOfWork.cs`

```csharp
namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>
/// Gom nhiều thay đổi (thêm/sửa/xóa qua IRepository&lt;T&gt;) thành 1 giao dịch,
/// chỉ thực sự ghi xuống database khi gọi SaveChangesAsync.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

### 2. Tạo `backend/src/CulinaryBlog.Application/Common/Interfaces/IRepository.cs`

```csharp
namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>
/// Trừu tượng hóa thao tác CRUD cơ bản trên 1 loại entity, tách Command Handler
/// khỏi việc biết chi tiết EF Core/DbContext. Dùng cho phía ghi dữ liệu (Command);
/// phía đọc (Query) vẫn dùng thẳng IApplicationDbContext như quy ước hiện tại.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);

    void Add(T entity);

    void Update(T entity);

    void Remove(T entity);
}
```

### 3. Tạo `backend/src/CulinaryBlog.Infrastructure/Persistence/Repositories/EfRepository.cs`

```csharp
using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Repositories;

/// <summary>
/// Cài đặt IRepository&lt;T&gt; bằng EF Core, dùng chung cho mọi entity
/// (Recipe, Category, RecipeIngredient, RecipeStep, RecipeImage...).
/// </summary>
public sealed class EfRepository<T>(CulinaryBlogDbContext context) : IRepository<T>
    where T : class
{
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Set<T>().FindAsync([id], cancellationToken);

    public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Set<T>().ToListAsync(cancellationToken);

    public void Add(T entity) => context.Set<T>().Add(entity);

    public void Update(T entity) => context.Set<T>().Update(entity);

    public void Remove(T entity) => context.Set<T>().Remove(entity);
}
```

Lưu ý: `CulinaryBlogDbContext` (không phải `IApplicationDbContext`) nằm cùng namespace `CulinaryBlog.Infrastructure.Persistence` — kiểm tra lại file `backend/src/CulinaryBlog.Infrastructure/Persistence/CulinaryBlogDbContext.cs` để chắc namespace/tên class khớp, không tự đoán.

### 4. Tạo `backend/src/CulinaryBlog.Infrastructure/Persistence/UnitOfWork.cs`

```csharp
using CulinaryBlog.Application.Common.Interfaces;

namespace CulinaryBlog.Infrastructure.Persistence;

/// <summary>
/// Cài đặt IUnitOfWork bằng CulinaryBlogDbContext.SaveChangesAsync.
/// </summary>
public sealed class UnitOfWork(CulinaryBlogDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
```

### 5. Đăng ký Dependency Injection

Mở `backend/src/CulinaryBlog.Infrastructure/DependencyInjection.cs`. Tìm đoạn:
```csharp
services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<CulinaryBlogDbContext>());
```
Thêm ngay sau đoạn đó (KHÔNG xóa/sửa đoạn trên):
```csharp
services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
services.AddScoped<IUnitOfWork, UnitOfWork>();
```
Thêm `using CulinaryBlog.Infrastructure.Persistence.Repositories;` vào đầu file nếu chưa có.

### 6. Sửa lại comment sai trong `IApplicationDbContext.cs`

Tìm đoạn:
```csharp
/// <summary>
/// Handler dùng trực tiếp DbSet này (không qua Repository/UnitOfWork — quy ước dự án).
/// Thao tác trên ApplicationUser vẫn đi qua UserManager&lt;ApplicationUser&gt;, KHÔNG qua đây.
/// </summary>
DbSet<RefreshToken> RefreshTokens { get; }
```
Đổi thành:
```csharp
/// <summary>
/// Query handler (phía đọc) dùng trực tiếp DbSet này theo CQRS.
/// Command handler (phía ghi) dùng qua IRepository&lt;T&gt;/IUnitOfWork (xem
/// CulinaryBlog.Application.Common.Interfaces.IRepository).
/// Thao tác trên ApplicationUser vẫn đi qua UserManager&lt;ApplicationUser&gt;, KHÔNG qua đây.
/// </summary>
DbSet<RefreshToken> RefreshTokens { get; }
```
CHỈ sửa đúng đoạn comment này, không đổi bất kỳ dòng code nào khác trong file.

## Bước kiểm tra bắt buộc sau khi sửa — không được báo "xong" nếu chưa làm

1. Chạy `dotnet build backend/CulinaryBlog.sln` — phải build sạch, `0 Warning(s)`, `0 Error(s)` (dự án có `TreatWarningsAsErrors=true`).
2. Chạy `grep -rn "IRepository\|IUnitOfWork" backend/src` — xác nhận cả 4 file mới (`IUnitOfWork.cs`, `IRepository.cs`, `EfRepository.cs`, `UnitOfWork.cs`) xuất hiện, và dòng đăng ký DI trong `DependencyInjection.cs` cũng xuất hiện.
3. Chạy `grep -rn "không qua Repository" backend/src` — phải **KHÔNG còn kết quả nào** (xác nhận đã sửa comment ở bước 6).
4. In ra: danh sách file đã tạo/sửa, kết quả build, kết quả 2 lệnh grep trên.

## KHÔNG làm những việc sau (ngoài phạm vi task này)

- Không viết bất kỳ Command/Query/Handler nào (đó là việc của từng thành viên theo `buoi3.md`).
- Không cài MediatR/FluentValidation.
- Không tạo Domain Exceptions hay Middleware xử lý lỗi (đó là 2 mục khác trong "Công việc chung của cả nhóm", không thuộc task Repository/UnitOfWork này).
- Không tự thêm phương thức `GetPagedAsync`, `Query()`, hay bất kỳ overload nào khác vào `IRepository<T>` ngoài 5 phương thức đã liệt kê ở bước 2.
