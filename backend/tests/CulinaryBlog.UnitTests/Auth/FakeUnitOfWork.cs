using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Repositories;

namespace CulinaryBlog.UnitTests.Auth;

/// <summary>Repository refresh token lưu trong bộ nhớ — dùng để test handler không cần database.</summary>
internal sealed class FakeRefreshTokenRepository : IRefreshTokenRepository
{
    public List<RefreshToken> Tokens { get; } = [];

    public Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Tokens.FirstOrDefault(token => token.Id == id));

    public Task AddAsync(RefreshToken entity, CancellationToken cancellationToken = default)
    {
        Tokens.Add(entity);
        return Task.CompletedTask;
    }

    public void Update(RefreshToken entity)
    {
    }

    public void Remove(RefreshToken entity) => Tokens.Remove(entity);

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        Task.FromResult(Tokens.FirstOrDefault(token => token.TokenHash == tokenHash));

    public Task<IReadOnlyList<RefreshToken>> GetNotRevokedByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<RefreshToken>>(Tokens.Where(token => token.FamilyId == familyId && !token.IsRevoked).ToList());

    public Task<IReadOnlyList<RefreshToken>> GetNotRevokedByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<RefreshToken>>(Tokens.Where(token => token.UserId == userId && !token.IsRevoked).ToList());
}

/// <summary>Unit of Work giả lập — đếm số lần SaveChanges và trạng thái transaction.</summary>
internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public FakeRefreshTokenRepository Tokens { get; } = new();

    public IRefreshTokenRepository RefreshTokens => Tokens;

    public int SaveChangesCount { get; private set; }

    public bool Committed { get; private set; }

    public bool RolledBack { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCount++;
        return Task.FromResult(1);
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        Committed = true;
        return Task.CompletedTask;
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        RolledBack = true;
        return Task.CompletedTask;
    }
}
