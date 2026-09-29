namespace CulinaryBlog.Application.Common.Exceptions;

using CulinaryBlog.Domain.Exceptions;

/// <summary>
/// Ngoại lệ ném ra khi người dùng không có quyền truy cập hoặc chỉnh sửa tài nguyên (HTTP 403 Forbidden).
/// Kế thừa ForbiddenDomainException từ Domain Layer để đảm bảo tính nhất quán kiến trúc.
/// </summary>
public class ForbiddenException : ForbiddenDomainException
{
    public ForbiddenException(string message = "Bạn không có quyền thực hiện thao tác trên tài nguyên này.")
        : base(message) { }
}
