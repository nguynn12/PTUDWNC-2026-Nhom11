namespace CulinaryBlog.Domain.Exceptions;

/// <summary>
/// Ngoại lệ cơ sở cho các lỗi vi phạm quy tắc nghiệp vụ trong tầng Domain.
/// </summary>
public abstract class DomainException : Exception
{
    public string? ErrorCode { get; }

    protected DomainException(string message, string? errorCode = "DOMAIN_ERROR")
        : base(message)
    {
        ErrorCode = errorCode;
    }

    protected DomainException(string message, Exception innerException, string? errorCode = "DOMAIN_ERROR")
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
