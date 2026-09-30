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
