namespace CulinaryBlog.Application.Admin.Dtos;

public sealed record AdminUserDto(
    string Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    IReadOnlyList<string> Roles,
    bool EmailConfirmed,
    bool IsActive,
    DateTimeOffset CreatedAt);
