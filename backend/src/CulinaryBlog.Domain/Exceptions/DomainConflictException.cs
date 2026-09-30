namespace CulinaryBlog.Domain.Exceptions;

/// <summary>Xung đột trạng thái của entity → 409. Ví dụ: <c>RECIPE_NOT_DELETED</c> khi restore Recipe chưa xoá.</summary>
public class DomainConflictException(string errorCode, string message)
    : DomainException(errorCode, message);
