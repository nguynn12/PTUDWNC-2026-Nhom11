using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    /// <summary>
    /// Handler dùng trực tiếp DbSet này (không qua Repository/UnitOfWork — quy ước dự án).
    /// Thao tác trên ApplicationUser vẫn đi qua UserManager&lt;ApplicationUser&gt;, KHÔNG qua đây.
    /// </summary>
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
