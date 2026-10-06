namespace CulinaryBlog.Application.Common.Exceptions;

using CulinaryBlog.Domain.Exceptions;

/// <summary>
/// Ngoại lệ đại diện cho trường hợp người dùng không có quyền truy cập hoặc thao tác trên tài nguyên (HTTP 403 Forbidden).
/// Tuân thủ mã lỗi RECIPE_FORBIDDEN theo SRS Phụ lục B.
/// </summary>
public class ForbiddenException : ForbiddenDomainException
{
    public ForbiddenException(string message = "Bạn không có quyền thực hiện thao tác này.", string errorCode = "FORBIDDEN")
        : base(message, errorCode)
    {
    }
}

