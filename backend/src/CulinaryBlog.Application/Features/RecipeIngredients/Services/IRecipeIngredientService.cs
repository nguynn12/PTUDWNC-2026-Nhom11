namespace CulinaryBlog.Application.Features.RecipeIngredients.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CulinaryBlog.Application.Features.RecipeIngredients.DTOs;

/// <summary>
/// Giao diện xử lý nghiệp vụ quản lý nguyên liệu công thức nấu ăn.
/// Tuân thủ SRS v1.2.0 (FR-RCP-009) và kế hoạch sửa lỗi SEC-01, SEC-02, API-03, API-08.
/// </summary>
public interface IRecipeIngredientService
{
    /// <summary>
    /// Lấy danh sách toàn bộ nguyên liệu của công thức, sắp xếp theo OrderIndex.
    /// Kiểm tra quyền tác giả hoặc Admin nếu Recipe ở trạng thái Draft/Archived (SEC-02).
    /// </summary>
    Task<IReadOnlyList<RecipeIngredientDto>> GetIngredientsAsync(
        Guid recipeId,
        string? currentUserId = null,
        bool isAdmin = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin chi tiết một nguyên liệu theo ID.
    /// Kiểm tra quyền tác giả hoặc Admin nếu Recipe ở trạng thái Draft/Archived (SEC-02).
    /// </summary>
    Task<RecipeIngredientDto> GetIngredientByIdAsync(
        Guid recipeId,
        Guid ingredientId,
        string? currentUserId = null,
        bool isAdmin = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm một nguyên liệu mới vào công thức.
    /// Kiểm tra quyền tác giả (Owner) hoặc Admin và kiểm tra concurrency If-Match (API-03).
    /// </summary>
    Task<RecipeIngredientDto> AddIngredientAsync(
        Guid recipeId,
        CreateRecipeIngredientRequest request,
        string? currentUserId,
        bool isAdmin,
        uint? concurrencyToken = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật thông tin của một nguyên liệu trong công thức.
    /// Kiểm tra quyền tác giả (Owner) hoặc Admin và kiểm tra concurrency If-Match (API-03).
    /// </summary>
    Task<RecipeIngredientDto> UpdateIngredientAsync(
        Guid recipeId,
        Guid ingredientId,
        UpdateRecipeIngredientRequest request,
        string? currentUserId,
        bool isAdmin,
        uint? concurrencyToken = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa mềm một nguyên liệu khỏi công thức.
    /// Kiểm tra quyền tác giả (Owner) hoặc Admin và kiểm tra concurrency If-Match (API-03).
    /// </summary>
    Task DeleteIngredientAsync(
        Guid recipeId,
        Guid ingredientId,
        string? currentUserId,
        bool isAdmin,
        uint? concurrencyToken = null,
        CancellationToken cancellationToken = default);
}
