namespace CulinaryBlog.Application.Features.RecipeSteps.Services;

using CulinaryBlog.Application.Features.RecipeSteps.DTOs;

/// <summary>
/// Giao diện xử lý nghiệp vụ quản lý các bước thực hiện công thức nấu ăn.
/// Tuân thủ SRS v1.2.0 (FR-RCP-010).
/// </summary>
public interface IRecipeStepService
{
    /// <summary>
    /// Lấy danh sách toàn bộ các bước nấu của công thức, sắp xếp theo StepNumber tăng dần.
    /// </summary>
    Task<IReadOnlyList<RecipeStepDto>> GetStepsAsync(
        Guid recipeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm một bước nấu mới vào công thức.
    /// Tự động tính toán StepNumber kế tiếp nếu không truyền vào.
    /// </summary>
    Task<RecipeStepDto> AddStepAsync(
        Guid recipeId,
        CreateRecipeStepRequest request,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật nội dung chi tiết của một bước nấu.
    /// </summary>
    Task<RecipeStepDto> UpdateStepAsync(
        Guid recipeId,
        Guid stepId,
        UpdateRecipeStepRequest request,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sắp xếp lại thứ tự toàn bộ các bước nấu theo danh sách IDs mới.
    /// Tự động cập nhật lại StepNumber từ 1..N liên tục.
    /// </summary>
    Task<IReadOnlyList<RecipeStepDto>> ReorderStepsAsync(
        Guid recipeId,
        ReorderRecipeStepsRequest request,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa mềm một bước nấu và tự động renumber lại các bước còn lại liên tục từ 1..N.
    /// </summary>
    Task DeleteStepAsync(
        Guid recipeId,
        Guid stepId,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);
}
