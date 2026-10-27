namespace CulinaryBlog.Application.Features.Categories.DTOs;

/// <summary>
/// Data Transfer Object (DTO) biểu diễn thông tin danh mục món ăn
/// trong danh sách trả về cho Client (người dùng và quản trị viên).
/// </summary>
public class CategoryDto
{
    /// <summary>
    /// Mã định danh duy nhất của danh mục (UUID v4).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tên danh mục (ví dụ: Món khai vị, Món chính, Món tráng miệng).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Đường dẫn định danh URL-friendly duy nhất của danh mục.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả chi tiết hoặc giới thiệu ngắn về danh mục.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Đường dẫn ảnh đại diện của danh mục.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Thứ tự sắp xếp hiển thị ưu tiên.
    /// </summary>
    public int OrderIndex { get; set; }

    /// <summary>
    /// Số lượng công thức món ăn đã xuất bản (Published) thuộc danh mục này.
    /// Dùng để hiển thị badge số lượng món ăn trên giao diện.
    /// </summary>
    public int RecipeCount { get; set; }

    /// <summary>
    /// Thời điểm danh mục được tạo (UTC).
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}
