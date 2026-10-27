namespace CulinaryBlog.Infrastructure.Repositories;

using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Hiện thực RecipeIngredientRepository quản lý nguyên liệu công thức nấu ăn.
/// </summary>
public class RecipeIngredientRepository : Repository<RecipeIngredient>, IRecipeIngredientRepository
{
    public RecipeIngredientRepository(CulinaryBlogDbContext context) : base(context) { }

    public async Task<IReadOnlyList<RecipeIngredient>> GetByRecipeIdAsync(Guid recipeId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(i => i.RecipeId == recipeId)
            .OrderBy(i => i.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetMaxOrderIndexAsync(Guid recipeId, CancellationToken cancellationToken = default)
    {
        var max = await DbSet
            .Where(i => i.RecipeId == recipeId)
            .Select(i => (int?)i.OrderIndex)
            .MaxAsync(cancellationToken);

        return max ?? -1;
    }
}
