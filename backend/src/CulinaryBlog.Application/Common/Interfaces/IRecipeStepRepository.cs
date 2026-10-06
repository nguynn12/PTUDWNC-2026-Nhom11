namespace CulinaryBlog.Application.Common.Interfaces;

using CulinaryBlog.Domain.Entities;

/// <summary>
/// Giao diện Repository chuyên biệt cho thực thể RecipeStep.
/// Hỗ trợ truy vấn và xử lý thứ tự các bước nấu.
/// </summary>
public interface IRecipeStepRepository : IRepository<RecipeStep>
{
    Task<IReadOnlyList<RecipeStep>> GetByRecipeIdAsync(Guid recipeId, CancellationToken cancellationToken = default);

    Task<int> GetMaxStepNumberAsync(Guid recipeId, CancellationToken cancellationToken = default);
}
