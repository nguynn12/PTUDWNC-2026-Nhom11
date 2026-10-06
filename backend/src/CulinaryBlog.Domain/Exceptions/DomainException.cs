namespace CulinaryBlog.Domain.Exceptions;

using System;

/// <summary>
/// Ngoại lệ cơ sở cho các lỗi vi phạm quy tắc nghiệp vụ trong tầng Domain theo chuẩn Lab 3.
/// </summary>
public abstract class DomainException : Exception
{
    public string? ErrorCode { get; }

    protected DomainException(string message, string? errorCode = null)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    protected DomainException(string message, Exception innerException, string? errorCode = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
