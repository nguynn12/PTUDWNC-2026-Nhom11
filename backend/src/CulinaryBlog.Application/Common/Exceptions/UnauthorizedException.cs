namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>Credential/token sai hoặc hết hạn → 401. Ví dụ: <c>AUTH_INVALID_CREDENTIALS</c>.</summary>
public sealed class UnauthorizedException(string errorCode, string message)
    : AppException(errorCode, message, AppErrorKind.Unauthorized);
