namespace CulinaryBlog.Application.Common.Exceptions;

using CulinaryBlog.Domain.Exceptions;

/// <summary>
/// Ngoại lệ ném ra khi có xung đột dữ liệu hoặc concurrency conflict (HTTP 409 Conflict).
/// Kế thừa ConcurrencyConflictException từ Domain Layer để đảm bảo tính nhất quán kiến trúc.
/// </summary>
public class ConflictException : ConcurrencyConflictException
{
    public ConflictException(string message) : base(message) { }
}
