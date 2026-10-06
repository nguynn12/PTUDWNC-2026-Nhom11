namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>
/// Ngoại lệ đại diện cho trường hợp xung đột dữ liệu hoặc vi phạm ràng buộc nghiệp vụ/concurrency (HTTP 409 Conflict).
/// </summary>
public class ConflictException : Exception
{
    public string ErrorCode { get; }

    public ConflictException(string message, string errorCode = "CONFLICT_ERROR")
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
