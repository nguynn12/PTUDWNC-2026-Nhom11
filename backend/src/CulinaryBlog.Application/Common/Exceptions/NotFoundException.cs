namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>
/// Ngoại lệ đại diện cho trường hợp không tìm thấy tài nguyên (HTTP 404 Not Found).
/// </summary>
public class NotFoundException : Exception
{
    public string ErrorCode { get; }

    public NotFoundException(string message, string errorCode = "RESOURCE_NOT_FOUND") : base(message)
    {
        ErrorCode = errorCode;
    }
}
