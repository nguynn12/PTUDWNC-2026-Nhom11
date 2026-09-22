namespace CulinaryBlog.Domain.ValueObjects;

using CulinaryBlog.Domain.Enums;

/// <summary>
/// Value Object đại diện cho thông tin dinh dưỡng của món ăn.
/// Tuân thủ đặc tả SRS v1.2.0 (mục 7.4) và quyết định kiến trúc RESOLVED-CONFLICTS.md mục E3.
/// Được lưu nhúng vào bảng Recipes dưới dạng Owned Entity trong EF Core.
/// </summary>
public class RecipeNutrition
{
    /// <summary>
    /// Năng lượng cung cấp (đơn vị: kcal).
    /// </summary>
    public decimal? Calories { get; set; }

    /// <summary>
    /// Hàm lượng chất đạm (đơn vị: gram).
    /// </summary>
    public decimal? Protein { get; set; }

    /// <summary>
    /// Hàm lượng carbohydrate / tinh bột (đơn vị: gram).
    /// </summary>
    public decimal? Carbohydrates { get; set; }

    /// <summary>
    /// Hàm lượng chất béo (đơn vị: gram).
    /// </summary>
    public decimal? Fat { get; set; }

    /// <summary>
    /// Hàm lượng chất xơ (đơn vị: gram).
    /// </summary>
    public decimal? Fiber { get; set; }

    /// <summary>
    /// Hàm lượng natri / muối (đơn vị: mg).
    /// </summary>
    public decimal? Sodium { get; set; }

    /// <summary>
    /// Nguồn gốc thông tin dinh dưỡng (mặc định Manual).
    /// </summary>
    public NutritionSource Source { get; set; } = NutritionSource.Manual;
}
