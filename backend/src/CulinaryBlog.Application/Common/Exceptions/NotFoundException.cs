namespace CulinaryBlog.Application.Common.Exceptions;

using CulinaryBlog.Domain.Exceptions;

/// <summary>
/// Ngoại lệ ném ra khi không tìm thấy tài nguyên yêu cầu (HTTP 404 Not Found).
/// Kế thừa EntityNotFoundException từ Domain Layer để đảm bảo tính nhất quán kiến trúc.
/// </summary>
public class NotFoundException : EntityNotFoundException
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string name, object key)
        : base(name, key) { }
}
