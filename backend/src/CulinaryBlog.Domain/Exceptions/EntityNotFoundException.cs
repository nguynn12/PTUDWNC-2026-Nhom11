namespace CulinaryBlog.Domain.Exceptions;

/// <summary>Không tìm thấy entity hoặc entity đã bị xoá mềm → 404. Ví dụ: <c>RECIPE_NOT_FOUND</c>.</summary>
public class EntityNotFoundException(string errorCode, string message)
    : DomainException(errorCode, message);
