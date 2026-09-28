namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>
/// Ngoại lệ ném ra khi có xung đột dữ liệu hoặc concurrency conflict (HTTP 409 Conflict).
/// Tuân thủ quyết định kiến trúc RESOLVED-CONFLICTS.md (C6).
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
