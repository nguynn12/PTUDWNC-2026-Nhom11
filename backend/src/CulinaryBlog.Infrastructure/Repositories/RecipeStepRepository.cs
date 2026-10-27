namespace CulinaryBlog.Infrastructure.Repositories;

using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Hiện thực RecipeStepRepository quản lý các bước thực hiện công thức nấu ăn.
/// </summary>
public class RecipeStepRepository : Repository<RecipeStep>, IRecipeStepRepository
{
    public RecipeStepRepository(CulinaryBlogDbContext context) : base(context) { }

    public async Task<IReadOnlyList<RecipeStep>> GetByRecipeIdAsync(Guid recipeId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(s => s.RecipeId == recipeId)
            .OrderBy(s => s.StepNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetMaxStepNumberAsync(Guid recipeId, CancellationToken cancellationToken = default)
    {
        var max = await DbSet
            .Where(s => s.RecipeId == recipeId)
            .Select(s => (int?)s.StepNumber)
            .MaxAsync(cancellationToken);

        return max ?? 0;
    }
}
