using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence;

/// <summary>
/// Kế thừa IdentityDbContext (thay vì DbContext thuần) để có sẵn AspNetUsers, AspNetRoles,
/// AspNetUserRoles, AspNetUserClaims, AspNetUserLogins (Google external login), AspNetUserTokens.
/// TKey = string vì ApplicationUser/IdentityRole dùng khoá chính string mặc định của Identity.
/// </summary>
public sealed class CulinaryBlogDbContext(DbContextOptions<CulinaryBlogDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole, string>(options), IApplicationDbContext
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // QUAN TRỌNG: phải gọi base TRƯỚC — IdentityDbContext.OnModelCreating khai báo
        // schema AspNetUsers/AspNetRoles/... Nếu gọi sau, ApplyConfigurationsFromAssembly
        // có thể ghi đè hoặc xung đột với cấu hình Identity mặc định.
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CulinaryBlogDbContext).Assembly);
    }
}
