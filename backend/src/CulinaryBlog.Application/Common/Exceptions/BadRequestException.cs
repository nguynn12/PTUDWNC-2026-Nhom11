namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>
/// Ngoại lệ ném ra khi cú pháp yêu cầu không hợp lệ hoặc dữ liệu tệp tin sai định dạng (HTTP 400 Bad Request).
/// Tuân thủ quyết định kiến trúc RESOLVED-CONFLICTS.md (C3).
/// </summary>
public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}
