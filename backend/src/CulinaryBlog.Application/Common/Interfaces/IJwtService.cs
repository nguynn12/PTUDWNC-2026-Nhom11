namespace CulinaryBlog.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(string userId, string email, IEnumerable<string> roles, bool emailConfirmed);
    (string TokenHash, string RawToken) GenerateRefreshToken();
}
