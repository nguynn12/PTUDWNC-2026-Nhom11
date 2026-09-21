namespace CulinaryBlog.Domain.Enums;

/// <summary>
/// Nguồn gốc thông tin dinh dưỡng của công thức theo quyết định RESOLVED-CONFLICTS.md mục E3.
/// </summary>
public enum NutritionSource
{
    /// <summary>
    /// Do tác giả tự nhập tay (mặc định cho MVP).
    /// </summary>
    Manual = 0,

    /// <summary>
    /// Do hệ thống tự động tính toán từ danh sách nguyên liệu (chuẩn bị cho Phase 2).
    /// </summary>
    Calculated = 1
}
