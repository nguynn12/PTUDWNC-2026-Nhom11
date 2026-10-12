namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>
/// Ngoại lệ đại diện cho trường hợp thông tin xác thực hoặc token không hợp lệ (HTTP 401 Unauthorized).
/// Ví dụ mã lỗi theo SRS Phụ lục B: AUTH_INVALID_CREDENTIALS, AUTH_TOKEN_INVALID.
/// Được xử lý bởi AuthExceptionHandler (tầng API).
/// </summary>
public class UnauthorizedException : Exception
{
    public string ErrorCode { get; }

    public UnauthorizedException(string message, string errorCode = "AUTH_TOKEN_INVALID")
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
