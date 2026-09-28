namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>
/// Ngoại lệ ném ra khi người dùng không có quyền truy cập hoặc chỉnh sửa tài nguyên (HTTP 403 Forbidden).
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "Bạn không có quyền thực hiện thao tác trên tài nguyên này.")
        : base(message) { }
}
