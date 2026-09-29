namespace CulinaryBlog.Domain.Exceptions;

/// <summary>
/// Ngoại lệ ném ra khi vi phạm quy tắc nghiệp vụ cốt lõi trong Domain (tương ứng HTTP 422 Unprocessable Entity).
/// </summary>
public class BusinessRuleValidationException : DomainException
{
    public string? PropertyName { get; }

    public BusinessRuleValidationException(string message) : base(message) { }

    public BusinessRuleValidationException(string propertyName, string message) : base(message)
    {
        PropertyName = propertyName;
    }
}
