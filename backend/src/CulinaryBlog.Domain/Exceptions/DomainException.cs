namespace CulinaryBlog.Domain.Exceptions;

/// <summary>
/// Lớp ngoại lệ cơ sở cho toàn bộ ngoại lệ nghiệp vụ của tầng Domain.
/// Tuân thủ nguyên tắc Clean Architecture: Domain độc lập và định nghĩa các lỗi quy tắc cốt lõi.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }

    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}
