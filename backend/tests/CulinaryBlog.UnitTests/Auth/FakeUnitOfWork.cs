using System.Linq.Expressions;
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

    public Task<IReadOnlyList<RefreshToken>> FindAsync(Expression<Func<RefreshToken, bool>> predicate, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<RefreshToken>>(Tokens.AsQueryable().Where(predicate).ToList());

    public Task<bool> AnyAsync(Expression<Func<RefreshToken, bool>> predicate, CancellationToken cancellationToken = default) =>
        Task.FromResult(Tokens.AsQueryable().Any(predicate));

    public void Add(RefreshToken entity) => Tokens.Add(entity);

    public Task AddAsync(RefreshToken entity, CancellationToken cancellationToken = default)
    {
        Tokens.Add(entity);
        return Task.CompletedTask;
    }

    public void Update(RefreshToken entity)
    {
    }

    public void Remove(RefreshToken entity) => Tokens.Remove(entity);

    public void Delete(RefreshToken entity) => Tokens.Remove(entity);

    public IQueryable<RefreshToken> Query() => Tokens.AsQueryable();

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

    public IRepository<Recipe> Recipes => null!;

    public IRecipeIngredientRepository RecipeIngredients => null!;

    public IRecipeStepRepository RecipeSteps => null!;

    public IRecipeImageRepository RecipeImages => null!;

    public int SaveChangesCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCount++;
        return Task.FromResult(1);
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void Dispose()
    {
    }
}

