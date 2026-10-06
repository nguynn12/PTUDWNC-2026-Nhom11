namespace CulinaryBlog.Application.Common.Exceptions;

using CulinaryBlog.Domain.Exceptions;

/// <summary>
/// Ngoại lệ ném ra khi dữ liệu đầu vào vi phạm quy tắc nghiệp vụ hoặc kiểm thực (HTTP 422 Unprocessable Entity).
/// Kế thừa BusinessRuleValidationException từ Domain Layer để đảm bảo tính nhất quán kiến trúc.
/// </summary>
public class ValidationException : BusinessRuleValidationException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException()
        : base("Đã xảy ra một hoặc nhiều lỗi kiểm thực dữ liệu.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string propertyName, string errorMessage)
        : base(propertyName, errorMessage)
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
