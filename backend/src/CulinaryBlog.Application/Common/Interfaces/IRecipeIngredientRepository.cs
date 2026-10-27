namespace CulinaryBlog.Application.Common.Interfaces;

using CulinaryBlog.Domain.Entities;

/// <summary>
/// Giao diện Repository chuyên biệt cho thực thể RecipeIngredient.
/// </summary>
public interface IRecipeIngredientRepository : IRepository<RecipeIngredient>
{
    Task<IReadOnlyList<RecipeIngredient>> GetByRecipeIdAsync(Guid recipeId, CancellationToken cancellationToken = default);

    Task<int> GetMaxOrderIndexAsync(Guid recipeId, CancellationToken cancellationToken = default);
}
