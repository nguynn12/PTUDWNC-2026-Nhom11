namespace CulinaryBlog.Domain.Repositories;

/// <summary>
/// Repository chung cho entity có khoá <see cref="Guid"/> (SRS 6.2 — interface đặt ở Domain,
/// hiện thực ở Infrastructure bằng EF Core). Thay đổi chỉ được ghi xuống DB khi gọi
/// <see cref="IUnitOfWork.SaveChangesAsync"/>.
/// </summary>
public interface IRepository<TEntity>
    where TEntity : class
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    void Remove(TEntity entity);
}
