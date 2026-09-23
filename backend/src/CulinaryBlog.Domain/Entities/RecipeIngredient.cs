namespace CulinaryBlog.Domain.Entities;

using CulinaryBlog.Domain.Common;

/// <summary>
/// Thực thể Nguyên liệu của Công thức nấu ăn (RecipeIngredient) theo SRS v1.2.0 (mục 7.4)
/// và các quyết định kiến trúc trong RESOLVED-CONFLICTS.md (E1, E2).
/// Thuộc trách nhiệm phân công của Thành viên 4.
/// </summary>
public class RecipeIngredient : BaseEntity
{
    /// <summary>
    /// Khóa ngoại tham chiếu đến Recipe sở hữu.
    /// </summary>
    public Guid RecipeId { get; set; }

    /// <summary>
    /// Tên nguyên liệu (ví dụ: "Thịt bò thăn", tối đa 200 ký tự, bắt buộc).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Định lượng nguyên liệu (kiểu decimal(10,3), nullable cho trường hợp "vừa đủ" theo quyết định E1 & E2).
    /// </summary>
    public decimal? Quantity { get; set; }

    /// <summary>
    /// Đơn vị đo lường (ví dụ: "gram", "ml", "thìa canh", "quả", tối đa 50 ký tự, nullable theo quyết định E2).
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Ghi chú sơ chế/chuẩn bị (ví dụ: "thái lát mỏng", "rửa sạch để ráo", tối đa 500 ký tự, nullable).
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Thứ tự hiển thị trong danh sách nguyên liệu (mặc định 0, sắp xếp tăng dần).
    /// </summary>
    public int OrderIndex { get; set; } = 0;

    /// <summary>
    /// Thuộc tính điều hướng đến công thức chứa nguyên liệu này.
    /// </summary>
    public virtual Recipe Recipe { get; set; } = null!;
}
