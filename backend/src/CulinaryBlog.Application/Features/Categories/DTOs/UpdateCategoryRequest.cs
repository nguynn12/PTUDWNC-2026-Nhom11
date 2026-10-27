namespace CulinaryBlog.Application.Features.Categories.DTOs;

/// <summary>
/// Dữ liệu đầu vào (Request Contract) khi Quản trị viên (Admin) gửi yêu cầu cập nhật thông tin danh mục món ăn.
/// Lưu ý: Thuộc tính Slug sẽ được giữ nguyên (bảo toàn SEO và liên kết ngoài theo SRS v1.2.1).
/// </summary>
public class UpdateCategoryRequest
{
    /// <summary>
    /// Tên cập nhật của danh mục món ăn (Bắt buộc, từ 2 đến 100 ký tự).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả chi tiết cập nhật của danh mục (Tùy chọn, tối đa 500 ký tự).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Đường dẫn ảnh đại diện cập nhật của danh mục.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Thứ tự hiển thị cập nhật của danh mục (Giá trị >= 0).
    /// </summary>
    public int OrderIndex { get; set; }
}
