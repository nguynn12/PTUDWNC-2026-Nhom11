namespace CulinaryBlog.Domain.Exceptions;

/// <summary>
/// Ngoại lệ ném ra khi vi phạm quy tắc nghiệp vụ cốt lõi trong Domain (tương ứng HTTP 422 Unprocessable Entity).
/// </summary>
public class BusinessRuleValidationException : DomainException
{
    public string? PropertyName { get; }

    public BusinessRuleValidationException(string message)
        : base(message, "UNPROCESSABLE_ENTITY") { }

    public BusinessRuleValidationException(string param1, string param2)
        : base(
            IsMessageAndErrorCode(param1, param2) ? param1 : param2,
            IsMessageAndErrorCode(param1, param2) ? param2 : "UNPROCESSABLE_ENTITY")
    {
        if (!IsMessageAndErrorCode(param1, param2))
        {
            PropertyName = param1;
        }
    }

    public BusinessRuleValidationException(string propertyName, string message, string? errorCode)
        : base(message, errorCode ?? "UNPROCESSABLE_ENTITY")
    {
        PropertyName = propertyName;
    }

    private static bool IsMessageAndErrorCode(string param1, string param2)
    {
        return param1.Contains(' ') || (param2.Length > 1 && param2.All(c => char.IsUpper(c) || char.IsDigit(c) || c == '_'));
    }
}
