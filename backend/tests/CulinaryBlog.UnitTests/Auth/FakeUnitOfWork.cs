using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.UnitTests.Auth;

/// <summary>Repository refresh token lưu trong bộ nhớ — dùng để test handler không cần database.</summary>
internal sealed class FakeRefreshTokenRepository : IRefreshTokenRepository
{
    public List<RefreshToken> Tokens { get; } = [];

    public Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Tokens.FirstOrDefault(token => token.Id == id));

    public Task<List<RefreshToken>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Tokens.ToList());

    public void Add(RefreshToken entity) => Tokens.Add(entity);

    public void Update(RefreshToken entity)
    {
    }

    public void Remove(RefreshToken entity) => Tokens.Remove(entity);

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        Task.FromResult(Tokens.FirstOrDefault(token => token.TokenHash == tokenHash));

    public Task<List<RefreshToken>> GetNotRevokedByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Tokens.Where(token => token.FamilyId == familyId && !token.IsRevoked).ToList());

    public Task<List<RefreshToken>> GetNotRevokedByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Tokens.Where(token => token.UserId == userId && !token.IsRevoked).ToList());
}

/// <summary>
/// Unit of Work giả lập — đếm số lần SaveChanges. Giữ sẵn một <see cref="FakeRefreshTokenRepository"/>
/// (<see cref="Tokens"/>) để test truyền cùng lúc repository và unit of work vào handler.
/// </summary>
internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public FakeRefreshTokenRepository Tokens { get; } = new();

    public int SaveChangesCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCount++;
        return Task.FromResult(1);
    }
}
