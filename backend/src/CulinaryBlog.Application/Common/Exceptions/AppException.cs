namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>
/// Lớp cơ sở cho mọi lỗi nghiệp vụ "có chủ đích" của hệ thống. Handler chỉ cần throw
/// exception con phù hợp; GlobalExceptionHandler (tầng API) chuyển thành RFC 7807 Problem
/// Details với <c>type</c> = <see cref="ErrorCode"/> (SRS Phụ lục B, Phụ lục C mục AEC).
/// </summary>
public abstract class AppException : Exception
{
    protected AppException(string errorCode, string message, AppErrorKind kind, Exception? innerException = null)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorCode);
        ErrorCode = errorCode;
        Kind = kind;
    }

    /// <summary>Mã lỗi SCREAMING_SNAKE_CASE, lấy từ <see cref="ErrorCodes"/>.</summary>
    public string ErrorCode { get; }

    public AppErrorKind Kind { get; }
}
