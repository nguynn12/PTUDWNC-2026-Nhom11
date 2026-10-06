namespace CulinaryBlog.Application.Features.RecipeIngredients.DTOs;

/// <summary>
/// DTO biểu diễn dữ liệu chi tiết của nguyên liệu trong công thức nấu ăn.
/// Tuân thủ SRS v1.2.0 (mục 7.4) và RESOLVED-CONFLICTS.md (E1, E2).
/// </summary>
public class RecipeIngredientDto
{
    /// <summary>
    /// Mã định danh duy nhất của nguyên liệu (UUID v4).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Mã công thức nấu ăn chứa nguyên liệu này.
    /// </summary>
    public Guid RecipeId { get; set; }

    /// <summary>
    /// Tên nguyên liệu (ví dụ: "Thịt thăn bò", "Hành lá").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Định lượng số học (decimal 10,3), nullable cho trường hợp "vừa đủ".
    /// </summary>
    public decimal? Quantity { get; set; }

    /// <summary>
    /// Đơn vị đo lường (ví dụ: "g", "ml", "thìa canh"), nullable theo E2.
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Ghi chú sơ chế (ví dụ: "thái mỏng", "rửa sạch").
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Thứ tự hiển thị trong danh sách nguyên liệu.
    /// </summary>
    public int OrderIndex { get; set; }

    /// <summary>
    /// Thời điểm tạo bản ghi.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Thời điểm cập nhật bản ghi gần nhất.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}
