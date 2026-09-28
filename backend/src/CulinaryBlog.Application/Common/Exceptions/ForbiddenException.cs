namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>Không đủ role/ownership/email chưa xác nhận → 403. Ví dụ: <c>RECIPE_FORBIDDEN</c>.</summary>
public sealed class ForbiddenException(string errorCode, string message)
    : AppException(errorCode, message, AppErrorKind.Forbidden);
