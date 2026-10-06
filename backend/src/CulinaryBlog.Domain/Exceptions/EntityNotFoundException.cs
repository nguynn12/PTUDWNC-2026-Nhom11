namespace CulinaryBlog.Domain.Exceptions;

/// <summary>
/// Ngoại lệ ném ra khi không tìm thấy một thực thể trong Domain (tương ứng HTTP 404 Not Found).
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object key)
        : base($"Thực thể '{entityName}' với khóa '{key}' không tồn tại hoặc đã bị xóa.") { }

    public EntityNotFoundException(string message) : base(message) { }
}
