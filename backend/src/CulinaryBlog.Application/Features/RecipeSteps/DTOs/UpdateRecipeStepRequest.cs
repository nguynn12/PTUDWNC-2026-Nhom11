namespace CulinaryBlog.Application.Features.RecipeSteps.DTOs;

/// <summary>
/// Hợp đồng dữ liệu yêu cầu cập nhật thông tin bước thực hiện trong công thức.
/// Tuân thủ SRS v1.2.0 (FR-RCP-010) và RESOLVED-CONFLICTS.md (E5).
/// </summary>
public class UpdateRecipeStepRequest
{
    /// <summary>
    /// Tiêu đề mới của bước (tối đa 200 ký tự, tùy chọn).
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Hướng dẫn chi tiết thao tác chế biến của bước này (1 - 2000 ký tự, bắt buộc).
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Thời gian ước tính cần cho bước này (đơn vị: phút, >= 0, tùy chọn).
    /// </summary>
    public int? DurationMinutes { get; set; }

    /// <summary>
    /// Đường dẫn ảnh minh họa riêng cho bước này (tùy chọn).
    /// </summary>
    public string? ImageUrl { get; set; }
}
