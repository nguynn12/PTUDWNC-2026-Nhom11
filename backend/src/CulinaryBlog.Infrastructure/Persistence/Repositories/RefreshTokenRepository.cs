using System.Linq.Expressions;
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

    public async Task<IReadOnlyList<RefreshToken>> FindAsync(Expression<Func<RefreshToken, bool>> predicate, CancellationToken cancellationToken = default)
        => await _dbSet.Where(predicate).ToListAsync(cancellationToken);

    public async Task<bool> AnyAsync(Expression<Func<RefreshToken, bool>> predicate, CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(predicate, cancellationToken);

    public void Add(RefreshToken entity) => _dbSet.Add(entity);

    public async Task AddAsync(RefreshToken entity, CancellationToken cancellationToken = default)
        => await _dbSet.AddAsync(entity, cancellationToken);

    public void Update(RefreshToken entity) => _dbSet.Update(entity);

    public void Remove(RefreshToken entity) => _dbSet.Remove(entity);

    public void Delete(RefreshToken entity) => _dbSet.Remove(entity);

    public IQueryable<RefreshToken> Query() => _dbSet.AsQueryable();

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
