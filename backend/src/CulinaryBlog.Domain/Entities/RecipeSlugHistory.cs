namespace CulinaryBlog.Domain.Entities;

using CulinaryBlog.Domain.Common;

/// <summary>
/// Thực thể lưu lịch sử các Slug cũ của công thức (Recipe Slug History).
/// Phục vụ SEO URL 301 Permanent Redirect khi công thức được cập nhật slug trước khi xuất bản lần đầu.
/// Tuân thủ quy định tại RESOLVED-CONFLICTS.md mục C7.
/// </summary>
public class RecipeSlugHistory : BaseEntity
{
    /// <summary>
    /// Mã định danh công thức sở hữu slug này.
    /// </summary>
    public Guid RecipeId { get; set; }

    /// <summary>
    /// Chuỗi slug cũ từng được sử dụng.
    /// </summary>
    public string OldSlug { get; set; } = string.Empty;
}
