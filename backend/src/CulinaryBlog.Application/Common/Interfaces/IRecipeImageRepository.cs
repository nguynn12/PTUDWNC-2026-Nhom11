namespace CulinaryBlog.Application.Common.Interfaces;

using CulinaryBlog.Domain.Entities;

/// <summary>
/// Giao diện Repository chuyên biệt cho thực thể RecipeImage.
/// Hỗ trợ truy vấn và xử lý hình ảnh và ảnh đại diện chính.
/// </summary>
public interface IRecipeImageRepository : IRepository<RecipeImage>
{
    Task<IReadOnlyList<RecipeImage>> GetByRecipeIdAsync(Guid recipeId, CancellationToken cancellationToken = default);

    Task<RecipeImage?> GetPrimaryImageAsync(Guid recipeId, CancellationToken cancellationToken = default);

    Task<int> GetMaxOrderIndexAsync(Guid recipeId, CancellationToken cancellationToken = default);
}
