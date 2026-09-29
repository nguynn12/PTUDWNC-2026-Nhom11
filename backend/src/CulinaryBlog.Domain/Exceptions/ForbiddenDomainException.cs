namespace CulinaryBlog.Domain.Exceptions;

/// <summary>
/// Ngoại lệ ném ra khi vi phạm quy tắc phân quyền sở hữu tài nguyên trong Domain (tương ứng HTTP 403 Forbidden).
/// </summary>
public class ForbiddenDomainException : DomainException
{
    public ForbiddenDomainException(string message = "Bạn không có quyền thực hiện thao tác trên tài nguyên này.")
        : base(message) { }
}
