namespace CulinaryBlog.Application.Features.RecipeIngredients.DTOs;

/// <summary>
/// Hợp đồng dữ liệu yêu cầu cập nhật nguyên liệu trong công thức.
/// Tuân thủ SRS v1.2.0 (FR-RCP-009) và RESOLVED-CONFLICTS.md (E1, E2).
/// </summary>
public class UpdateRecipeIngredientRequest
{
    /// <summary>
    /// Tên nguyên liệu mới (1 - 200 ký tự, bắt buộc).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Định lượng nguyên liệu (decimal > 0 nếu có giá trị; null nếu "vừa đủ").
    /// </summary>
    public decimal? Quantity { get; set; }

    /// <summary>
    /// Đơn vị đo lường (tối đa 50 ký tự; bắt buộc nếu Quantity có giá trị, ngược lại null).
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Ghi chú sơ chế/chuẩn bị (tối đa 500 ký tự, tùy chọn).
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Thứ tự hiển thị trong danh sách nguyên liệu.
    /// </summary>
    public int OrderIndex { get; set; }
}
