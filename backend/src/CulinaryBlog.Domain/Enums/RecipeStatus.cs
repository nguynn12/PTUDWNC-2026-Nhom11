namespace CulinaryBlog.Domain.Enums;

/// <summary>
/// Trạng thái vòng đời của công thức nấu ăn (Recipe Lifecycle State Machine).
/// Tuân theo đặc tả SRS v1.2.0 (mục 7.2) và quyết định kiến trúc trong RESOLVED-CONFLICTS.md.
/// </summary>
public enum RecipeStatus
{
    /// <summary>
    /// Bản nháp, chỉ tác giả xem và chỉnh sửa được, chưa hiển thị công khai.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Đã xuất bản công khai, hiển thị trên trang chủ và kết quả tìm kiếm.
    /// </summary>
    Published = 1,

    /// <summary>
    /// Đã lưu trữ, ẩn khỏi danh sách công khai nhưng tác giả vẫn có thể xem lại hoặc unarchive về Draft.
    /// </summary>
    Archived = 2
}