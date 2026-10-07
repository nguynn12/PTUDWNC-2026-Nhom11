namespace CulinaryBlog.Application.Features.RecipeSteps.DTOs;

/// <summary>
/// DTO biểu diễn dữ liệu của một bước thực hiện trong công thức nấu ăn.
/// Tuân thủ SRS v1.2.0 (mục 7.3, FR-RCP-010) và RESOLVED-CONFLICTS.md (E5, E8).
/// </summary>
public class RecipeStepDto
{
    /// <summary>
    /// Mã định danh duy nhất của bước nấu (UUID v4).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Mã công thức nấu ăn chứa bước này.
    /// </summary>
    public Guid RecipeId { get; set; }

    /// <summary>
    /// Thứ tự bước thực hiện (1, 2, 3...), tăng dần liên tục.
    /// </summary>
    public int StepNumber { get; set; }

    /// <summary>
    /// Tiêu đề hoặc tên tóm tắt của bước (ví dụ: "Sơ chế rau củ").
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Hướng dẫn chi tiết thao tác chế biến của bước này.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Thời gian ước tính cần cho bước này (đơn vị: phút, >= 0).
    /// </summary>
    public int? DurationMinutes { get; set; }

    /// <summary>
    /// Đường dẫn ảnh minh họa riêng cho bước này (nếu có).
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Thời điểm tạo bản ghi.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Thời điểm cập nhật bản ghi gần nhất.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}
