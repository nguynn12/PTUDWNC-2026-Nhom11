using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Identity;

/// <summary>
/// Cài đặt IUserQueryService bằng truy vấn trực tiếp bảng AspNetUsers (chỉ đọc).
/// Không lọc IsActive: công thức của tài khoản đã bị vô hiệu hoá vẫn hiển thị đúng tên tác giả.
/// </summary>
public sealed class UserQueryService(CulinaryBlogDbContext context) : IUserQueryService
{
    public async Task<IReadOnlyDictionary<string, AuthorSummary>> GetAuthorSummariesAsync(
        IEnumerable<string> userIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userIds);

        var ids = userIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (ids.Count == 0)
        {
            return new Dictionary<string, AuthorSummary>();
        }

        return await context.Users
            .AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .Select(u => new AuthorSummary(u.Id, u.DisplayName, u.AvatarUrl))
            .ToDictionaryAsync(a => a.Id, StringComparer.Ordinal, cancellationToken);
    }
}
