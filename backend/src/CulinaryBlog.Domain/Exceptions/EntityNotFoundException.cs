namespace CulinaryBlog.Domain.Exceptions;

/// <summary>
/// Ngoại lệ ném ra khi không tìm thấy một thực thể trong Domain (tương ứng HTTP 404 Not Found).
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object key, string? errorCode = "RECIPE_NOT_FOUND")
        : base($"Không tìm thấy thực thể \"{entityName}\" với mã định danh ({key}).", errorCode) { }

    public EntityNotFoundException(string message) : base(message, "RECIPE_NOT_FOUND") { }

    public EntityNotFoundException(string message, string? errorCode) : base(message, errorCode) { }
}
