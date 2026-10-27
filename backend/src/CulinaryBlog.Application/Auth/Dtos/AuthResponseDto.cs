namespace CulinaryBlog.Application.Auth.Dtos;

public class AuthResponseDto
{
    public string AccessToken { get; init; } = null!;
    public string RefreshToken { get; init; } = null!;
    public int ExpiresIn { get; init; }
    public UserDto User { get; init; } = null!;
}
