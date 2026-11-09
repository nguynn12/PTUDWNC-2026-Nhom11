namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>
/// Quản lý việc hủy hiệu lực bộ đệm theo tag và phân hệ (NFR-PERF).
/// Dành cho các Command Handler (Tạo/Sửa/Publish/Archive/Xóa công thức, danh mục, ảnh, nguyên liệu, bước nấu) gọi.
/// </summary>
public interface ICacheInvalidator
{
    /// <summary>
    /// Hủy cache công thức liên quan: recipe detail, slug, danh sách recipes và tìm kiếm.
    /// </summary>
    Task InvalidateRecipeAsync(Guid recipeId, string? slug = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hủy cache danh mục: danh sách categories và chi tiết category theo slug.
    /// </summary>
    Task InvalidateCategoriesAsync(string? slug = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hủy toàn bộ cache tìm kiếm (gọi khi danh sách công thức công khai có thay đổi).
    /// </summary>
    Task InvalidateSearchAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa sạch mọi cache công khai.
    /// </summary>
    Task InvalidateAllAsync(CancellationToken cancellationToken = default);
}

