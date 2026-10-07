namespace CulinaryBlog.Application.Common.Exceptions;

using CulinaryBlog.Domain.Exceptions;

/// <summary>
/// Ngoại lệ đại diện cho trường hợp không tìm thấy tài nguyên (HTTP 404 Not Found).
/// Phục vụ chuẩn hóa mã lỗi theo SRS Phụ lục B (ví dụ: RECIPE_NOT_FOUND, CATEGORY_NOT_FOUND).
/// </summary>
public class NotFoundException : EntityNotFoundException
{
    public NotFoundException(string message, string errorCode = "RESOURCE_NOT_FOUND")
        : base(message, errorCode)
    {
    }

    public NotFoundException(string name, object key, string errorCode = "RESOURCE_NOT_FOUND")
        : base(name, key, errorCode)
    {
    }
}

