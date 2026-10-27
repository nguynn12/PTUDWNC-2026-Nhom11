namespace CulinaryBlog.Application.Features.RecipeImages.DTOs;

/// <summary>
/// DTO biểu diễn dữ liệu của một hình ảnh minh họa công thức nấu ăn.
/// Tuân thủ SRS v1.2.0 (mục 7.5, FR-RCP-008) và RESOLVED-CONFLICTS.md (E6, E7).
/// </summary>
public class RecipeImageDto
{
    /// <summary>
    /// Mã định danh duy nhất của hình ảnh (UUID v4).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Mã định danh hình ảnh (alias bắt buộc theo quyết định kiến trúc E7).
    /// </summary>
    public Guid ImageId => Id;

    /// <summary>
    /// Mã công thức sở hữu hình ảnh này.
    /// </summary>
    public Guid RecipeId { get; set; }

    /// <summary>
    /// Đường dẫn ảnh gốc trên MinIO/Object Storage (bắt buộc theo E7).
    /// </summary>
    public string OriginalUrl { get; set; } = string.Empty;

    /// <summary>
    /// Đường dẫn ảnh cỡ trung bình 800x600 (nếu có).
    /// </summary>
    public string? MediumUrl { get; set; }

    /// <summary>
    /// Đường dẫn ảnh thu nhỏ 300x300 (nếu có).
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Văn bản thay thế hỗ trợ trợ năng accessibility (nullable).
    /// </summary>
    public string? AltText { get; set; }

    /// <summary>
    /// Đánh dấu ảnh chính của công thức (chỉ có duy nhất 1 ảnh IsPrimary = true).
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Thứ tự hiển thị trong bộ sưu tập gallery.
    /// </summary>
    public int OrderIndex { get; set; }

    /// <summary>
    /// Thời điểm tạo bản ghi.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}
