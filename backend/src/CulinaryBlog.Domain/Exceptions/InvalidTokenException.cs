namespace CulinaryBlog.Domain.Exceptions;

/// <summary>Token (refresh token) không còn dùng được → 401 (SRS FR-AUTH-004).</summary>
public abstract class InvalidTokenException(string errorCode, string message)
    : DomainException(errorCode, message);

/// <summary>Refresh token đã bị thu hồi (logout/rotate/reuse) → 401 <c>AUTH_REFRESH_TOKEN_REVOKED</c>.</summary>
public sealed class RefreshTokenRevokedException()
    : InvalidTokenException(Code, "Refresh token đã bị thu hồi.")
{
    public const string Code = "AUTH_REFRESH_TOKEN_REVOKED";
}

/// <summary>Refresh token đã hết hạn → 401 <c>AUTH_REFRESH_TOKEN_EXPIRED</c>.</summary>
public sealed class RefreshTokenExpiredException()
    : InvalidTokenException(Code, "Refresh token đã hết hạn.")
{
    public const string Code = "AUTH_REFRESH_TOKEN_EXPIRED";
}
