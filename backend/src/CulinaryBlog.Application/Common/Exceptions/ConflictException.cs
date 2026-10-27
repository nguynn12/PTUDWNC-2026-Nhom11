namespace CulinaryBlog.Application.Common.Exceptions;

using CulinaryBlog.Domain.Exceptions;

/// <summary>
/// Ngoại lệ đại diện cho trường hợp xung đột dữ liệu hoặc vi phạm ràng buộc nghiệp vụ/concurrency (HTTP 409 Conflict).
/// </summary>
public class ConflictException : ConcurrencyConflictException
{
    public ConflictException(string message, string errorCode = "CONFLICT_ERROR")
        : base(message, errorCode)
    {
    }
}

