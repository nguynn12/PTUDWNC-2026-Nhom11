namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>
/// Vi phạm quy tắc nghiệp vụ → 422 (RESOLVED-CONFLICTS C3/C4).
/// Ví dụ: <c>RECIPE_PUBLISH_INCOMPLETE</c>.
/// </summary>
public sealed class BusinessRuleException(string errorCode, string message)
    : AppException(errorCode, message, AppErrorKind.BusinessRule);
