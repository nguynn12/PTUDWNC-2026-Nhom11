namespace CulinaryBlog.Domain.Exceptions;

/// <summary>
/// Entity từ chối thao tác vì vi phạm quy tắc nghiệp vụ → 422 (RESOLVED-CONFLICTS C3/C4).
/// Ví dụ: publish Recipe khi chưa có nguyên liệu/bước (<c>RECIPE_PUBLISH_INCOMPLETE</c>).
/// </summary>
public class BusinessRuleViolationException(string errorCode, string message)
    : DomainException(errorCode, message);
