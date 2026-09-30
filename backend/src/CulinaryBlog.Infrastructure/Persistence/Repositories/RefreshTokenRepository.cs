using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Repositories;

/// <summary>
/// Hiện thực <see cref="IRefreshTokenRepository"/> bằng EF Core. Các thao tác CRUD cơ bản giống
/// <see cref="EfRepository{T}"/>; chỉ bổ sung truy vấn riêng của module Auth.
/// </summary>
public sealed class RefreshTokenRepository(CulinaryBlogDbContext context) : IRefreshTokenRepository
{
    private readonly DbSet<RefreshToken> _dbSet = context.Set<RefreshToken>();

    public async Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync([id], cancellationToken);

    public async Task<List<RefreshToken>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbSet.ToListAsync(cancellationToken);

    public void Add(RefreshToken entity) => _dbSet.Add(entity);

    public void Update(RefreshToken entity) => _dbSet.Update(entity);

    public void Remove(RefreshToken entity) => _dbSet.Remove(entity);

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => _dbSet.FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

    public Task<List<RefreshToken>> GetNotRevokedByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken = default)
        => _dbSet
            .Where(token => token.FamilyId == familyId && token.RevokedAt == null)
            .ToListAsync(cancellationToken);

    public Task<List<RefreshToken>> GetNotRevokedByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        => _dbSet
            .Where(token => token.UserId == userId && token.RevokedAt == null)
            .ToListAsync(cancellationToken);
}
