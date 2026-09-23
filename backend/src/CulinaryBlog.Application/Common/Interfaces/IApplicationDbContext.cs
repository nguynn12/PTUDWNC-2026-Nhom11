using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    /// <summary>
    /// Bảng quản lý danh mục món ăn.
    /// </summary>
    DbSet<Category> Categories { get; }

    /// <summary>
    /// Bảng quản lý các công thức nấu ăn.
    /// </summary>
    DbSet<Recipe> Recipes { get; }

    /// <summary>
    /// Handler dùng trực tiếp DbSet này (không qua Repository/UnitOfWork — quy ước dự án).
    /// Thao tác trên ApplicationUser vẫn đi qua UserManager&lt;ApplicationUser&gt;, KHÔNG qua đây.
    /// </summary>
    DbSet<RefreshToken> RefreshTokens { get; }

    /// <summary>
    /// Bảng quản lý nguyên liệu chi tiết của công thức (Thành viên 4).
    /// </summary>
    DbSet<RecipeIngredient> RecipeIngredients { get; }

    /// <summary>
    /// Bảng quản lý các bước chế biến chi tiết của công thức (Thành viên 4).
    /// </summary>
    DbSet<RecipeStep> RecipeSteps { get; }

    /// <summary>
    /// Bảng quản lý hình ảnh minh họa của công thức (Thành viên 4).
    /// </summary>
    DbSet<RecipeImage> RecipeImages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
