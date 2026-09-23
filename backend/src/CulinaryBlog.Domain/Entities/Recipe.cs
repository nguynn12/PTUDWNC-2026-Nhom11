namespace CulinaryBlog.Domain.Entities;

using CulinaryBlog.Domain.Common;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.ValueObjects;

/// <summary>
/// Thực thể cốt lõi Công thức nấu ăn (Recipe) theo đặc tả SRS v1.2.0 (mục 7.2)
/// và các quyết định kiến trúc trong RESOLVED-CONFLICTS.md (A1, C5, C7, E3, E4).
/// Quản lý toàn bộ thông tin công thức và trạng thái vòng đời xuất bản.
/// </summary>
public class Recipe : BaseEntity
{
    /// <summary>
    /// Tiêu đề công thức nấu ăn (tối đa 200 ký tự, bắt buộc).
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Chuỗi định danh URL-friendly duy nhất (tối đa 220 ký tự, bắt buộc).
    /// Sau khi xuất bản lần đầu (PublishedAt != null), slug trở thành bất biến theo C7.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả ngắn gọn hoặc tóm tắt hương vị món ăn (tối đa 500 ký tự).
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Thời gian sơ chế nguyên liệu (đơn vị: phút, bắt buộc > 0 theo E4).
    /// </summary>
    public int PrepTimeMinutes { get; set; }

    /// <summary>
    /// Thời gian nấu nướng (đơn vị: phút, >= 0 theo E4, cho phép 0 với món không cần nấu như salad/sinh tố).
    /// </summary>
    public int CookTimeMinutes { get; set; }

    /// <summary>
    /// Khẩu phần phục vụ (số người ăn, bắt buộc > 0).
    /// </summary>
    public int Servings { get; set; }

    /// <summary>
    /// Mức độ khó chế biến của công thức.
    /// </summary>
    public RecipeDifficulty Difficulty { get; set; } = RecipeDifficulty.Easy;

    /// <summary>
    /// Trạng thái vòng đời xuất bản hiện tại (mặc định Draft).
    /// </summary>
    public RecipeStatus Status { get; set; } = RecipeStatus.Draft;

    /// <summary>
    /// Khóa ngoại tham chiếu đến Danh mục món ăn (Category).
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Điều hướng tham chiếu đến thực thể Danh mục món ăn (Category).
    /// </summary>
    public Category? Category { get; set; }

    /// <summary>
    /// Mã định danh người dùng tác giả tạo công thức.
    /// </summary>
    public string AuthorId { get; set; } = string.Empty;

    /// <summary>
    /// Điều hướng tham chiếu đến người dùng tác giả (ApplicationUser).
    /// </summary>
    public ApplicationUser? Author { get; set; }

    /// <summary>
    /// Thời điểm công thức được xuất bản công khai lần đầu tiên.
    /// Null nếu công thức chưa từng được xuất bản.
    /// </summary>
    public DateTimeOffset? PublishedAt { get; set; }

    /// <summary>
    /// Thông tin giá trị dinh dưỡng đính kèm (Value Object / Owned Entity).
    /// </summary>
    public RecipeNutrition? Nutrition { get; set; }
}