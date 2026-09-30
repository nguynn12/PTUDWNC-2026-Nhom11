namespace CulinaryBlog.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(string userId, string email, IEnumerable<string> roles, bool emailConfirmed);
    (string TokenHash, string RawToken) GenerateRefreshToken();

    /// <summary>Thời hạn access token (giây) — trả về client ở trường <c>expiresIn</c>. SRS NFR-SEC: 15 phút.</summary>
    int AccessTokenLifetimeSeconds { get; }
}
