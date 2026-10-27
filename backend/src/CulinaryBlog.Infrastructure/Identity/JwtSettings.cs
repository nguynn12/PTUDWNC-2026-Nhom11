namespace CulinaryBlog.Infrastructure.Identity;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    public string Secret { get; init; } = null!;
    /// <summary>Thời hạn access token (phút). Mặc định 15 theo SRS NFR-SEC.</summary>
    public int ExpiryMinutes { get; init; } = 15;
    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
}
