using CulinaryBlog.Application.Common.Models;

namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>
/// Xác minh Google ID token (chữ ký, issuer, audience, hạn dùng và nonce nếu có) — FR-AUTH-003.
/// Trả null khi token sai hoặc hết hạn.
/// </summary>
public interface IGoogleTokenValidator
{
    Task<GoogleUserInfo?> ValidateAsync(string idToken, string? nonce, CancellationToken cancellationToken = default);
}
