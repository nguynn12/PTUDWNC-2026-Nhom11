namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>
/// Ngoại lệ đại diện cho trường hợp xung đột dữ liệu hoặc vi phạm ràng buộc nghiệp vụ/concurrency (HTTP 409 Conflict).
/// Tuân thủ mã lỗi RECIPE_CONCURRENCY_CONFLICT theo RESOLVED-CONFLICTS.md C6.
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
