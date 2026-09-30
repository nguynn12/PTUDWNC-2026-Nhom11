namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>
/// Ngoại lệ đại diện cho trường hợp tài khoản đang bị khóa tạm thời sau nhiều lần đăng nhập sai
/// (HTTP 423 Locked — FR-AUTH-002 A2). Được xử lý bởi AuthExceptionHandler (tầng API).
/// </summary>
public class LockedException : Exception
{
    public string ErrorCode { get; }

    public LockedException(string message, string errorCode = "AUTH_ACCOUNT_LOCKED")
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
