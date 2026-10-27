namespace CulinaryBlog.Application.Auth.Dtos;

public class UserDto
{
    public string Id { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string DisplayName { get; init; } = null!;
    public string? AvatarUrl { get; init; }
    public string? Bio { get; init; }
    public IEnumerable<string> Roles { get; init; } = Array.Empty<string>();
    public bool EmailConfirmed { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
