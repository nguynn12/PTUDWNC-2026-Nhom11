using CulinaryBlog.Domain.Repositories;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace CulinaryBlog.Infrastructure.Repositories;

/// <summary>
/// Unit of Work trên <see cref="CulinaryBlogDbContext"/> (scoped theo request). UserManager của
/// Identity dùng CHUNG DbContext này nên transaction mở ở đây bao cả thao tác tạo user.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly CulinaryBlogDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(CulinaryBlogDbContext context, IRefreshTokenRepository refreshTokens)
    {
        _context = context;
        RefreshTokens = refreshTokens;
    }

    public IRefreshTokenRepository RefreshTokens { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Đã có transaction đang mở trong Unit of Work này.");
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("Chưa mở transaction.");
        }

        try
        {
            await _transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            return;
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _transaction = null;
        GC.SuppressFinalize(this);
    }

    private async Task DisposeTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
