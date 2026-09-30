namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>Tài khoản bị khoá tạm thời → 423. Ví dụ: <c>AUTH_ACCOUNT_LOCKED</c>.</summary>
public sealed class LockedException(string errorCode, string message)
    : AppException(errorCode, message, AppErrorKind.Locked);
