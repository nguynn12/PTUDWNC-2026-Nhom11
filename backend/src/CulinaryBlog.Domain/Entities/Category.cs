namespace CulinaryBlog.Domain.Entities;

using CulinaryBlog.Domain.Common;

/// <summary>
/// Thực thể Danh mục món ăn (Category) theo đặc tả SRS v1.2.0 (mục 7.6).
/// Quản lý phân loại các công thức nấu ăn trong hệ thống.
/// </summary>
public class Category : BaseEntity
{
    /// <summary>
    /// Tên danh mục món ăn (tối đa 100 ký tự, bắt buộc).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Chuỗi định danh URL-friendly duy nhất (tối đa 120 ký tự, bắt buộc).
    /// Tự động sinh từ Name; giữ nguyên khi đổi tên để tránh gãy URL/SEO.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả chi tiết về danh mục.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Đường dẫn ảnh đại diện danh mục (tối đa 500 ký tự).
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Thứ tự hiển thị danh mục (mặc định 0).
    /// </summary>
    public int OrderIndex { get; set; } = 0;
}
