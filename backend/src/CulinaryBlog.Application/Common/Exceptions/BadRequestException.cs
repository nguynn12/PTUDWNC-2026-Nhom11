namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>
/// Ngoại lệ ném ra khi cú pháp yêu cầu không hợp lệ hoặc dữ liệu tệp tin sai định dạng (HTTP 400 Bad Request).
/// Tuân thủ quyết định kiến trúc RESOLVED-CONFLICTS.md (C3) và kế hoạch sửa lỗi API-06.
/// </summary>
public class BadRequestException : Exception
{
    public string? ErrorCode { get; }
    public int StatusCode { get; } = 400;

    public BadRequestException(string message) : base(message)
    {
        ErrorCode = "BAD_REQUEST";
    }

    public BadRequestException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    public BadRequestException(string message, string errorCode, int statusCode) : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}

