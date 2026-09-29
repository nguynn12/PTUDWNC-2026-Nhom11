namespace CulinaryBlog.Application.Common.Interfaces;

using System.Linq.Expressions;
using CulinaryBlog.Domain.Common;

/// <summary>
/// Giao diện Repository tổng quát (Generic Repository) theo chuẩn Clean Architecture.
/// Định nghĩa các thao tác CRUD cơ bản trên các thực thể kế thừa BaseEntity.
/// </summary>
/// <typeparam name="T">Kiểu thực thể Domain kế thừa BaseEntity.</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    void Update(T entity);

    void Delete(T entity);

    IQueryable<T> Query();
}
