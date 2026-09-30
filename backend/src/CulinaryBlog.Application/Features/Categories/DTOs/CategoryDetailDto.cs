namespace CulinaryBlog.Application.Features.Categories.DTOs;

/// <summary>
/// Data Transfer Object (DTO) biểu diễn thông tin chi tiết đầy đủ của một danh mục
/// khi người dùng hoặc quản trị viên tra cứu theo slug hoặc id.
/// </summary>
public class CategoryDetailDto
{
    /// <summary>
    /// Mã định danh duy nhất của danh mục (UUID v4).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tên danh mục món ăn.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Đường dẫn định danh URL-friendly duy nhất của danh mục.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả chi tiết danh mục.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Đường dẫn ảnh đại diện của danh mục.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Thứ tự sắp xếp hiển thị.
    /// </summary>
    public int OrderIndex { get; set; }

    /// <summary>
    /// Tổng số lượng công thức món ăn đã xuất bản (Published) thuộc danh mục này.
    /// </summary>
    public int RecipeCount { get; set; }

    /// <summary>
    /// Thời điểm danh mục được tạo (UTC).
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Thời điểm danh mục được cập nhật lần cuối (UTC).
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}
