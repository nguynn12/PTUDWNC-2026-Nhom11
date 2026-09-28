namespace CulinaryBlog.Application.Features.RecipeIngredients.Services;

using CulinaryBlog.Application.Features.RecipeIngredients.DTOs;

/// <summary>
/// Giao diện xử lý nghiệp vụ quản lý nguyên liệu công thức nấu ăn.
/// Tuân thủ SRS v1.2.0 (FR-RCP-009).
/// </summary>
public interface IRecipeIngredientService
{
    /// <summary>
    /// Lấy danh sách toàn bộ nguyên liệu của công thức, sắp xếp theo OrderIndex.
    /// </summary>
    Task<IReadOnlyList<RecipeIngredientDto>> GetIngredientsAsync(
        Guid recipeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm một nguyên liệu mới vào công thức.
    /// Kiểm tra quyền tác giả (Owner) hoặc Admin trước khi thêm.
    /// </summary>
    Task<RecipeIngredientDto> AddIngredientAsync(
        Guid recipeId,
        CreateRecipeIngredientRequest request,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật thông tin của một nguyên liệu trong công thức.
    /// Kiểm tra quyền tác giả (Owner) hoặc Admin.
    /// </summary>
    Task<RecipeIngredientDto> UpdateIngredientAsync(
        Guid recipeId,
        Guid ingredientId,
        UpdateRecipeIngredientRequest request,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa mềm một nguyên liệu khỏi công thức.
    /// Kiểm tra quyền tác giả (Owner) hoặc Admin.
    /// </summary>
    Task DeleteIngredientAsync(
        Guid recipeId,
        Guid ingredientId,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);
}
