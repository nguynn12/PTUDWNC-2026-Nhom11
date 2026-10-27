namespace CulinaryBlog.Infrastructure.Repositories;

using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Hiện thực RecipeImageRepository quản lý hình ảnh công thức nấu ăn.
/// </summary>
public class RecipeImageRepository : Repository<RecipeImage>, IRecipeImageRepository
{
    public RecipeImageRepository(CulinaryBlogDbContext context) : base(context) { }

    public async Task<IReadOnlyList<RecipeImage>> GetByRecipeIdAsync(Guid recipeId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(i => i.RecipeId == recipeId)
            .OrderByDescending(i => i.IsPrimary)
            .ThenBy(i => i.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task<RecipeImage?> GetPrimaryImageAsync(Guid recipeId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(i => i.RecipeId == recipeId && i.IsPrimary, cancellationToken);
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
