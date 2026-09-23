namespace CulinaryBlog.Domain.Entities;

using CulinaryBlog.Domain.Common;

/// <summary>
/// Thực thể Hình ảnh minh họa của Công thức nấu ăn (RecipeImage) theo SRS v1.2.0 (mục 7.5)
/// và các quyết định kiến trúc trong RESOLVED-CONFLICTS.md (E6, E7).
/// Thuộc trách nhiệm phân công của Thành viên 4.
/// </summary>
public class RecipeImage : BaseEntity
{
    /// <summary>
    /// Khóa ngoại tham chiếu đến Recipe sở hữu.
    /// </summary>
    public Guid RecipeId { get; set; }

    /// <summary>
    /// Đường dẫn ảnh gốc trên MinIO/Object Storage (tối đa 500 ký tự, bắt buộc).
    /// Quy tắc đặt tên: recipes/{recipeId}/{guid}.{ext}
    /// </summary>
    public string OriginalUrl { get; set; } = string.Empty;

    /// <summary>
    /// Đường dẫn ảnh kích thước trung bình 800x600 (sinh bởi background job, nullable).
    /// </summary>
    public string? MediumUrl { get; set; }

    /// <summary>
    /// Đường dẫn ảnh thu nhỏ thumbnail 300x300 (sinh bởi background job, nullable).
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Văn bản thay thế cho ảnh phục vụ khả năng truy cập trợ năng accessibility (tối đa 200 ký tự, nullable).
    /// </summary>
    public string? AltText { get; set; }

    /// <summary>
    /// Đánh dấu ảnh chính (ảnh đại diện món ăn hiển thị đầu tiên).
    /// Mỗi Recipe chỉ có duy nhất 1 ảnh IsPrimary = true.
    /// </summary>
    public bool IsPrimary { get; set; } = false;

    /// <summary>
    /// Thứ tự hiển thị trong bộ sưu tập gallery (mặc định 0, sắp xếp tăng dần).
    /// </summary>
    public int OrderIndex { get; set; } = 0;

    /// <summary>
    /// Thuộc tính điều hướng đến công thức sở hữu ảnh này.
    /// </summary>
    public virtual Recipe Recipe { get; set; } = null!;
}
