namespace CulinaryBlog.Application.Features.RecipeImages.Services;

using CulinaryBlog.Application.Features.RecipeImages.DTOs;

/// <summary>
/// Giao diện xử lý nghiệp vụ quản lý hình ảnh công thức nấu ăn.
/// Tuân thủ SRS v1.2.0 (FR-RCP-008) và RESOLVED-CONFLICTS.md (E6, E7).
/// </summary>
public interface IRecipeImageService
{
    /// <summary>
    /// Lấy danh sách toàn bộ hình ảnh minh họa của công thức.
    /// </summary>
    Task<IReadOnlyList<RecipeImageDto>> GetImagesAsync(
        Guid recipeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload hình ảnh minh họa mới cho công thức.
    /// Kiểm tra định dạng Magic Bytes, lưu trữ qua IFileStorageService
    /// và xử lý gán ảnh chính (IsPrimary) theo quy chuẩn E6 & E7.
    /// </summary>
    Task<RecipeImageDto> UploadImageAsync(
        Guid recipeId,
        Stream stream,
        string fileName,
        string contentType,
        long length,
        string? altText,
        bool? isPrimary,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật metadata hình ảnh (altText, orderIndex, isPrimary).
    /// Khi isPrimary = true, tự động set các ảnh khác = false trong cùng transaction (E6).
    /// </summary>
    Task<RecipeImageDto> UpdateImageAsync(
        Guid recipeId,
        Guid imageId,
        UpdateRecipeImageRequest request,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa hình ảnh minh họa của công thức.
    /// Nếu ảnh bị xóa là ảnh chính, tự động bầu ảnh có orderIndex nhỏ nhất còn lại làm ảnh chính (FR-RCP-008).
    /// </summary>
    Task DeleteImageAsync(
        Guid recipeId,
        Guid imageId,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);
}
