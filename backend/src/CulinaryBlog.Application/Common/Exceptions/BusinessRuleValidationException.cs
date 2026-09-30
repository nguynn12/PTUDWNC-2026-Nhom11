namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>
/// Ngoại lệ đại diện cho vi phạm quy tắc nghiệp vụ (HTTP 422 Unprocessable Entity).
/// Ví dụ: RECIPE_PUBLISH_INCOMPLETE khi công thức thiếu nguyên liệu hoặc bước thực hiện (RESOLVED-CONFLICTS C4, C5).
/// </summary>
public class BusinessRuleValidationException : Exception
{
    public string ErrorCode { get; }

    public BusinessRuleValidationException(string message, string errorCode = "UNPROCESSABLE_ENTITY")
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
