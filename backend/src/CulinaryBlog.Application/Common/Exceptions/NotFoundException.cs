namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>
/// Ngoại lệ ném ra khi không tìm thấy tài nguyên yêu cầu (HTTP 404 Not Found).
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string name, object key)
        : base($"Không tìm thấy thực thể '{name}' với khóa '{key}'.") { }
}
