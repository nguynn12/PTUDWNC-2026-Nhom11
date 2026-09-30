namespace CulinaryBlog.Domain.Exceptions;

/// <summary>
/// Refresh token không còn dùng được (SRS FR-AUTH-004) → HTTP 401.
/// Kế thừa <see cref="DomainException"/> dùng chung của nhóm (DomainExceptions.cs); được
/// AuthExceptionHandler (tầng API) xử lý trước GlobalExceptionHandler để trả 401 thay vì 400.
/// </summary>
public abstract class InvalidTokenException(string message, string errorCode)
    : DomainException(message, errorCode);

/// <summary>Refresh token đã bị thu hồi (logout/rotate/reuse) → 401 <c>AUTH_REFRESH_TOKEN_REVOKED</c>.</summary>
public sealed class RefreshTokenRevokedException()
    : InvalidTokenException("Refresh token đã bị thu hồi.", Code)
{
    public const string Code = "AUTH_REFRESH_TOKEN_REVOKED";
}

/// <summary>Refresh token đã hết hạn → 401 <c>AUTH_REFRESH_TOKEN_EXPIRED</c>.</summary>
public sealed class RefreshTokenExpiredException()
    : InvalidTokenException("Refresh token đã hết hạn.", Code)
{
    public const string Code = "AUTH_REFRESH_TOKEN_EXPIRED";
}
