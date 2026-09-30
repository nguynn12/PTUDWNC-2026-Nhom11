namespace CulinaryBlog.Application.Common.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
