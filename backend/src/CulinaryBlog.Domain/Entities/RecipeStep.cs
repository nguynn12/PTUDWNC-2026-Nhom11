namespace CulinaryBlog.Domain.Entities;

using CulinaryBlog.Domain.Common;

/// <summary>
/// Thực thể Bước thực hiện của Công thức nấu ăn (RecipeStep) theo SRS v1.2.0 (mục 7.3)
/// và các quyết định kiến trúc trong RESOLVED-CONFLICTS.md (E5, E8).
/// Thuộc trách nhiệm phân công của Thành viên 4.
/// </summary>
public class RecipeStep : BaseEntity
{
    /// <summary>
    /// Khóa ngoại tham chiếu đến Recipe sở hữu.
    /// </summary>
    public Guid RecipeId { get; set; }

    /// <summary>
    /// Thứ tự bước thực hiện (1, 2, 3...), bắt buộc > 0, tăng dần liên tục.
    /// Tạo composite unique index với RecipeId.
    /// </summary>
    public int StepNumber { get; set; }

    /// <summary>
    /// Tiêu đề hoặc tên ngắn gọn của bước (tối đa 200 ký tự, bắt buộc, ví dụ: "Sơ chế nguyên liệu").
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Hướng dẫn chi tiết thao tác chế biến của bước này (bắt buộc theo SRS 7.3 và quyết định E5).
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Thời gian ước tính cần cho bước này (đơn vị: phút, >= 0, nullable).
    /// Đổi tên từ TimerMinutes sang DurationMinutes theo quyết định kiến trúc E5.
    /// </summary>
    public int? DurationMinutes { get; set; }

    /// <summary>
    /// Đường dẫn ảnh minh họa riêng cho bước này (tối đa 500 ký tự, nullable).
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Mã bước cha (ParentStepId) dùng cho cấu trúc bước con trong tương lai theo quyết định E8 (mặc định null).
    /// </summary>
    public Guid? ParentStepId { get; set; }

    /// <summary>
    /// Thuộc tính điều hướng đến bước cha (self-referencing).
    /// </summary>
    public virtual RecipeStep? ParentStep { get; set; }

    /// <summary>
    /// Danh sách các bước con trực thuộc bước này.
    /// </summary>
    public virtual ICollection<RecipeStep> SubSteps { get; set; } = new List<RecipeStep>();

    /// <summary>
    /// Thuộc tính điều hướng đến công thức chứa bước này.
    /// </summary>
    public virtual Recipe Recipe { get; set; } = null!;
}
