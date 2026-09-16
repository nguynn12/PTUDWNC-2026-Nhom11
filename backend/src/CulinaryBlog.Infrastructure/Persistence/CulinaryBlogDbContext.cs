namespace CulinaryBlog.Infrastructure.Persistence;

using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// DbContext của ứng dụng Culinary Blog cho module Category.
/// Quản lý DbSet Categories và nạp cấu hình Fluent API từ Assembly.
/// </summary>
public class CulinaryBlogDbContext : DbContext
{
    public CulinaryBlogDbContext(DbContextOptions<CulinaryBlogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CulinaryBlogDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
