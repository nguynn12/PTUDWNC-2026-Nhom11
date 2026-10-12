using CulinaryBlog.Application.Common.Models;

namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>
/// Tra cứu thông tin người dùng cho các module khác (Recipe, Discovery, ...).
///
/// Vì sao cần: ApplicationUser nằm ở Infrastructure (Domain phải thuần .NET BCL — NFR-MAINT-004),
/// nên Recipe không có navigation Author và tầng Application không truy cập trực tiếp bảng
/// AspNetUsers. Mọi nhu cầu "lấy tên/ảnh tác giả" đi qua interface này.
/// Xem docs/decisions/RESOLVED-CONFLICTS.md mục D7.
/// </summary>
public interface IUserQueryService
{
    /// <summary>
    /// Lấy thông tin tác giả cho nhiều user trong MỘT câu truy vấn (tránh N+1 khi hiển thị
    /// danh sách công thức). Id không tồn tại sẽ không có trong kết quả; Id rỗng/trùng được bỏ qua.
    /// </summary>
    /// <example>
    /// var authors = await userQueryService.GetAuthorSummariesAsync(recipes.Select(r => r.AuthorId), ct);
    /// var name = authors.TryGetValue(recipe.AuthorId, out var a) ? a.DisplayName : null;
    /// </example>
    Task<IReadOnlyDictionary<string, AuthorSummary>> GetAuthorSummariesAsync(
        IEnumerable<string> userIds,
        CancellationToken cancellationToken = default);
}
