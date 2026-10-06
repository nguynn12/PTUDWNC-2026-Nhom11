namespace CulinaryBlog.Application.Common.Models;

/// <summary>
/// Thông tin tài khoản đọc từ ASP.NET Core Identity, dùng ở tầng Application thay cho
/// <c>ApplicationUser</c> (nằm ở Infrastructure — RESOLVED-CONFLICTS D7).
/// </summary>
public sealed record UserAccount(
    string Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    string? Bio,
    IReadOnlyList<string> Roles,
    bool EmailConfirmed,
    bool IsActive,
    DateTimeOffset CreatedAt);
