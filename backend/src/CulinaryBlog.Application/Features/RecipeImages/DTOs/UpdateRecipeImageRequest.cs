namespace CulinaryBlog.Application.Features.RecipeImages.DTOs;

/// <summary>
/// Hợp đồng dữ liệu yêu cầu cập nhật thông tin hình ảnh (PATCH /recipes/{id}/images/{imageId}).
/// Tuân thủ SRS v1.2.0 (FR-RCP-008) và RESOLVED-CONFLICTS.md (E6).
/// </summary>
public class UpdateRecipeImageRequest
{
    /// <summary>
    /// Văn bản thay thế mô tả ảnh (tùy chọn).
    /// </summary>
    public string? AltText { get; set; }

    /// <summary>
    /// Đặt ảnh này làm ảnh đại diện chính của công thức (tùy chọn).
    /// Khi đặt true, server sẽ tự động bỏ cờ IsPrimary của các ảnh khác cùng công thức trong transaction.
    /// </summary>
    public bool? IsPrimary { get; set; }

    /// <summary>
    /// Thứ tự hiển thị trong bộ sưu tập gallery (tùy chọn).
    /// </summary>
    public int? OrderIndex { get; set; }
}
