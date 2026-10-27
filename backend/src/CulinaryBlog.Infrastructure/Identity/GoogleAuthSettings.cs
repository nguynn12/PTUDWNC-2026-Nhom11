namespace CulinaryBlog.Infrastructure.Identity;

/// <summary>Cấu hình đăng nhập Google (FR-AUTH-003) — section <c>GoogleAuth</c> trong appsettings.</summary>
public class GoogleAuthSettings
{
    public const string SectionName = "GoogleAuth";

    /// <summary>
    /// OAuth Client ID (Web) tạo trên Google Cloud Console; frontend dùng cùng Client ID để lấy
    /// ID token. Đây là audience hợp lệ duy nhất của token. Để trống thì mọi Google token bị từ chối.
    /// </summary>
    public string ClientId { get; init; } = string.Empty;
}
