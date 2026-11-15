namespace CulinaryBlog.Application.Features.RecipeSteps.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CulinaryBlog.Application.Features.RecipeSteps.DTOs;

/// <summary>
/// Giao diện xử lý nghiệp vụ quản lý các bước thực hiện công thức nấu ăn.
/// Tuân thủ SRS v1.2.0 (FR-RCP-010) và kế hoạch sửa lỗi SEC-01, SEC-02, API-03, API-05, API-08, API-17.
/// </summary>
public interface IRecipeStepService
{
    /// <summary>
    /// Lấy danh sách toàn bộ các bước nấu của công thức, sắp xếp theo StepNumber tăng dần.
    /// Kiểm tra quyền tác giả hoặc Admin nếu Recipe ở trạng thái Draft/Archived (SEC-02).
    /// </summary>
    Task<IReadOnlyList<RecipeStepDto>> GetStepsAsync(
        Guid recipeId,
        string? currentUserId = null,
        bool isAdmin = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm một bước nấu mới vào công thức.
    /// Tự động sinh StepNumber liên tục kế tiếp từ server (API-17) và kiểm tra If-Match (API-03).
    /// </summary>
    Task<RecipeStepDto> AddStepAsync(
        Guid recipeId,
        CreateRecipeStepRequest request,
        string? currentUserId,
        bool isAdmin,
        uint? concurrencyToken = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật nội dung chi tiết của một bước nấu. Hỗ trợ If-Match (API-03).
    /// </summary>
    Task<RecipeStepDto> UpdateStepAsync(
        Guid recipeId,
        Guid stepId,
        UpdateRecipeStepRequest request,
        string? currentUserId,
        bool isAdmin,
        uint? concurrencyToken = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sắp xếp lại thứ tự toàn bộ các bước nấu theo danh sách IDs mới.
    /// Cập nhật hai giai đoạn (two-stage update) tránh xung đột unique index (API-05).
    /// </summary>
    Task<IReadOnlyList<RecipeStepDto>> ReorderStepsAsync(
        Guid recipeId,
        ReorderRecipeStepsRequest request,
        string? currentUserId,
        bool isAdmin,
        uint? concurrencyToken = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa mềm một bước nấu và tự động renumber lại các bước còn lại liên tục từ 1..N.
    /// </summary>
    Task DeleteStepAsync(
        Guid recipeId,
        Guid stepId,
        string? currentUserId,
        bool isAdmin,
        uint? concurrencyToken = null,
        CancellationToken cancellationToken = default);
}
