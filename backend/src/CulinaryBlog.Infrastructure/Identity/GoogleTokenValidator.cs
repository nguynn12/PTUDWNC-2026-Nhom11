using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.Infrastructure.Identity;

/// <summary>
/// Xác minh Google ID token bằng thư viện Google chính thức (SRS FR-AUTH-003, RESOLVED-CONFLICTS D2):
/// <see cref="GoogleJsonWebSignature.ValidateAsync(string, GoogleJsonWebSignature.ValidationSettings)"/>
/// kiểm tra chữ ký (public key Google, tự cache và làm mới), issuer <c>accounts.google.com</c>,
/// audience = <see cref="GoogleAuthSettings.ClientId"/> và hạn dùng. Nonce do lớp này đối chiếu thêm.
/// </summary>
public sealed class GoogleTokenValidator : IGoogleTokenValidator
{
    /// <summary>Điểm thay thế cho unit test; mặc định gọi thẳng thư viện Google.</summary>
    public delegate Task<GoogleJsonWebSignature.Payload> VerifyIdToken(
        string idToken, GoogleJsonWebSignature.ValidationSettings settings);

    private static readonly TimeSpan ClockTolerance = TimeSpan.FromMinutes(1);

    private readonly GoogleAuthSettings _settings;
    private readonly ILogger<GoogleTokenValidator> _logger;
    private readonly VerifyIdToken _verify;

    public GoogleTokenValidator(
        IOptions<GoogleAuthSettings> settings,
        ILogger<GoogleTokenValidator> logger,
        VerifyIdToken? verify = null)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings.Value;
        _logger = logger;
        _verify = verify ?? new VerifyIdToken(GoogleJsonWebSignature.ValidateAsync);
    }

    public async Task<GoogleUserInfo?> ValidateAsync(string idToken, string? nonce, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ClientId))
        {
            _logger.LogError("Chưa cấu hình {Section}:ClientId nên không thể xác minh Google ID token.", GoogleAuthSettings.SectionName);
            return null;
        }

        if (string.IsNullOrWhiteSpace(idToken))
        {
            return null;
        }

        var validationSettings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = [_settings.ClientId],
            IssuedAtClockTolerance = ClockTolerance,
            ExpirationTimeClockTolerance = ClockTolerance,
        };

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await _verify(idToken, validationSettings).WaitAsync(cancellationToken);
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogInformation(ex, "Google ID token không hợp lệ.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(payload.Subject) || string.IsNullOrWhiteSpace(payload.Email))
        {
            return null;
        }

        // Client gửi nonce (frontend sinh khi khởi tạo Google Identity Services) thì token phải mang đúng nonce đó.
        if (!string.IsNullOrEmpty(nonce) && !string.Equals(payload.Nonce, nonce, StringComparison.Ordinal))
        {
            _logger.LogWarning("Google ID token có nonce không khớp.");
            return null;
        }

        return new GoogleUserInfo(payload.Subject, payload.Email, payload.EmailVerified, payload.Name, payload.Picture);
    }
}
