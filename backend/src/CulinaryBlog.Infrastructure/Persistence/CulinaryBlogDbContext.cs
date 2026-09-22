using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence;

/// <summary>
/// DbContext chính của ứng dụng CulinaryBlog, quản lý kết nối PostgreSQL và ánh xạ các thực thể.
/// </summary>
public sealed class CulinaryBlogDbContext(DbContextOptions<CulinaryBlogDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    /// <summary>
    /// Bảng quản lý các công thức nấu ăn.
    /// </summary>
    public DbSet<Recipe> Recipes => Set<Recipe>();

    /// <summary>
    /// Bảng lưu vết lịch sử các slug của công thức phục vụ SEO 301.
    /// </summary>
    public DbSet<RecipeSlugHistory> RecipeSlugHistories => Set<RecipeSlugHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CulinaryBlogDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
