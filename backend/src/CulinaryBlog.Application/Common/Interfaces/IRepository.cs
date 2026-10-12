namespace CulinaryBlog.Application.Common.Interfaces;

using System.Linq.Expressions;

/// <summary>
/// Giao diện Repository tổng quát (Generic Repository) theo chuẩn Clean Architecture.
/// Định nghĩa các thao tác CRUD cơ bản trên các thực thể Domain.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    void Add(T entity);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    void Update(T entity);

    void Remove(T entity);

    void Delete(T entity);

    IQueryable<T> Query();
}
