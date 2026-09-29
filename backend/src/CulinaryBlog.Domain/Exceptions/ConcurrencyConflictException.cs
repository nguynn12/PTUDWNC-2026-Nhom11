namespace CulinaryBlog.Domain.Exceptions;

/// <summary>
/// Ngoại lệ ném ra khi phát hiện xung đột dữ liệu đồng thời (tương ứng HTTP 409 Conflict).
/// Tuân thủ quyết định kiến trúc RESOLVED-CONFLICTS.md (C6).
/// </summary>
public class ConcurrencyConflictException : DomainException
{
    public ConcurrencyConflictException(string message = "Dữ liệu đã bị thay đổi bởi thao tác khác. Vui lòng tải lại và thử lại.")
        : base(message) { }
}
