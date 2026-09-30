namespace CulinaryBlog.Domain.Exceptions;

/// <summary>
/// Lớp cơ sở cho mọi lỗi vi phạm quy tắc nghiệp vụ phát sinh trong Domain (entity tự bảo vệ
/// bất biến của mình). Domain không biết HTTP: tầng API (ProblemDetailsMapper) dựa vào loại
/// exception để chọn status và dùng <see cref="ErrorCode"/> làm trường <c>type</c> của RFC 7807.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string errorCode, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorCode);
        ErrorCode = errorCode;
    }

    /// <summary>Mã lỗi SCREAMING_SNAKE_CASE theo SRS Phụ lục B.</summary>
    public string ErrorCode { get; }
}
