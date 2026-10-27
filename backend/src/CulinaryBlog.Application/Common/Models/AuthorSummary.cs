namespace CulinaryBlog.Application.Common.Models;

/// <summary>
/// Thông tin công khai tối thiểu của tác giả để hiển thị kèm công thức (danh sách, chi tiết).
/// KHÔNG chứa Email hay dữ liệu nhạy cảm. Id khớp với Recipe.AuthorId (AspNetUsers.Id).
/// </summary>
public sealed record AuthorSummary(string Id, string DisplayName, string? AvatarUrl);
