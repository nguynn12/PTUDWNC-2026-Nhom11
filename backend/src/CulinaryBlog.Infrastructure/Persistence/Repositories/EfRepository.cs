namespace CulinaryBlog.Infrastructure.Persistence.Repositories;

using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await context.Set<T>().AddAsync(entity, cancellationToken);

    public void Update(T entity) => context.Set<T>().Update(entity);

    public void Remove(T entity) => context.Set<T>().Remove(entity);

    public void Delete(T entity) => context.Set<T>().Remove(entity);
}
