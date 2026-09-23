using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence;

/// <summary>
/// DbContext chính của ứng dụng CulinaryBlog, kế thừa IdentityDbContext để quản lý
/// xác thực ASP.NET Core Identity và các thực thể nghiệp vụ (Recipe, Category, RefreshToken,...).
/// </summary>
public sealed class CulinaryBlogDbContext(DbContextOptions<CulinaryBlogDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole, string>(options), IApplicationDbContext
{
    /// <summary>
    /// Bảng quản lý danh mục món ăn.
    /// </summary>
    public DbSet<Category> Categories => Set<Category>();

    /// <summary>
    /// Bảng quản lý các công thức nấu ăn.
    /// </summary>
    public DbSet<Recipe> Recipes => Set<Recipe>();

    /// <summary>
    /// Bảng lưu vết lịch sử các slug của công thức phục vụ SEO 301.
    /// </summary>
    public DbSet<RecipeSlugHistory> RecipeSlugHistories => Set<RecipeSlugHistory>();

    /// <summary>
    /// Bảng lưu trữ Refresh Token của người dùng.
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>
    /// Bảng quản lý nguyên liệu chi tiết của công thức (Thành viên 4).
    /// </summary>
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();

    /// <summary>
    /// Bảng quản lý các bước chế biến chi tiết của công thức (Thành viên 4).
    /// </summary>
    public DbSet<RecipeStep> RecipeSteps => Set<RecipeStep>();

    /// <summary>
    /// Bảng quản lý hình ảnh minh họa của công thức (Thành viên 4).
    /// </summary>
    public DbSet<RecipeImage> RecipeImages => Set<RecipeImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CulinaryBlogDbContext).Assembly);
    }
}
