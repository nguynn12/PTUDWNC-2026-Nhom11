namespace CulinaryBlog.Application.Features.Categories.DTOs;

/// <summary>
/// Dữ liệu đầu vào (Request Contract) khi Quản trị viên (Admin) gửi yêu cầu tạo mới một danh mục món ăn.
/// </summary>
public class CreateCategoryRequest
{
    /// <summary>
    /// Tên danh mục món ăn mới (Bắt buộc, từ 2 đến 100 ký tự).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả chi tiết danh mục món ăn (Tùy chọn, tối đa 500 ký tự).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Đường dẫn ảnh đại diện của danh mục (Tùy chọn, định dạng URL hợp lệ).
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Thứ tự hiển thị danh mục (Tùy chọn, mặc định là 0, giá trị >= 0).
    /// </summary>
    public int OrderIndex { get; set; } = 0;
}
