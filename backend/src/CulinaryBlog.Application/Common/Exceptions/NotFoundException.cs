namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>Resource không tồn tại hoặc đã soft-delete → 404. Ví dụ: <c>RECIPE_NOT_FOUND</c>.</summary>
public sealed class NotFoundException(string errorCode, string message)
    : AppException(errorCode, message, AppErrorKind.NotFound);
