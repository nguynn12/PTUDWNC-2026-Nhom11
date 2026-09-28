namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>Trùng unique hoặc xung đột trạng thái → 409. Ví dụ: <c>AUTH_EMAIL_EXISTS</c>.</summary>
public sealed class ConflictException(string errorCode, string message)
    : AppException(errorCode, message, AppErrorKind.Conflict);
