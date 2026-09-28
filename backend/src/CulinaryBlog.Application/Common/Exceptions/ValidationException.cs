namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>
/// Ngoại lệ ném ra khi dữ liệu đầu vào vi phạm quy tắc nghiệp vụ hoặc kiểm thực (HTTP 422 Unprocessable Entity).
/// Tuân thủ RFC 7807 và quyết định kiến trúc RESOLVED-CONFLICTS.md (C3).
/// </summary>
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException()
        : base("Đã xảy ra một hoặc nhiều lỗi kiểm thực dữ liệu.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string propertyName, string errorMessage)
        : base(errorMessage)
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        };
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Đã xảy ra một hoặc nhiều lỗi kiểm thực dữ liệu.")
    {
        Errors = errors;
    }
}
