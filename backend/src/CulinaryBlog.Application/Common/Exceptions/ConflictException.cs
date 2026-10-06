namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>
/// Ngoại lệ đại diện cho trường hợp xung đột dữ liệu hoặc vi phạm ràng buộc nghiệp vụ/concurrency (HTTP 409 Conflict).
<<<<<<< HEAD
=======
/// Tuân thủ mã lỗi RECIPE_CONCURRENCY_CONFLICT theo RESOLVED-CONFLICTS.md C6.
>>>>>>> origin/2312704_TaNhatNguyen_Recipe-lifecycle_backend
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
