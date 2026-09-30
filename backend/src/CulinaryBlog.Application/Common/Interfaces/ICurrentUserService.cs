namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>
/// Giao diện lấy thông tin ngữ cảnh người dùng đang thực hiện request.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// ID định danh của người dùng (từ JWT claim NameIdentifier hoặc Sub).
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Kiểm tra người dùng có vai trò Quản trị viên (Admin) hay không.
    /// </summary>
    bool IsAdmin { get; }
}
