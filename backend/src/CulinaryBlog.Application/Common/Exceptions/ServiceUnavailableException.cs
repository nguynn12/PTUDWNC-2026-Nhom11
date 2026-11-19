namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>
/// Ngoại lệ ném ra khi một dịch vụ phụ thuộc bên ngoài (Object Storage MinIO, Mail Server, v.v.) tạm thời không khả dụng.
/// Được ánh xạ sang HTTP 503 Service Unavailable theo chuẩn RFC 7807 (MEDIA-03).
/// </summary>
public class ServiceUnavailableException : Exception
{
    public string ErrorCode { get; }

    public ServiceUnavailableException(
        string message = "Dịch vụ tạm thời không khả dụng. Vui lòng thử lại sau.",
        string errorCode = "SERVICE_UNAVAILABLE")
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public ServiceUnavailableException(
        string message,
        Exception innerException,
        string errorCode = "SERVICE_UNAVAILABLE")
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
