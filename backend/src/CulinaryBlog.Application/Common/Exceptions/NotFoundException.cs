namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>
/// Ngoại lệ đại diện cho trường hợp không tìm thấy tài nguyên (HTTP 404 Not Found).
/// Phục vụ chuẩn hóa mã lỗi theo SRS Phụ lục B (ví dụ: RECIPE_NOT_FOUND, CATEGORY_NOT_FOUND).
/// </summary>
public class NotFoundException : Exception
{
    public string ErrorCode { get; }

    public NotFoundException(string message, string errorCode = "RESOURCE_NOT_FOUND")
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public NotFoundException(string name, object key, string errorCode = "RESOURCE_NOT_FOUND")
        : base($"Không tìm thấy thực thể \"{name}\" với mã định danh ({key}).")
    {
        ErrorCode = errorCode;
    }
}
