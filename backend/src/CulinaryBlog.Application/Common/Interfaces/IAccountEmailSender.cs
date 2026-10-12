namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>
/// Soạn và gửi các email tài khoản (xác nhận email — FR-AUTH-001/009). Phần dựng link tới trang
/// frontend nằm ở Infrastructure (cần cấu hình URL frontend), handler chỉ truyền dữ liệu.
/// </summary>
public interface IAccountEmailSender
{
    Task SendEmailConfirmationAsync(
        string userId,
        string email,
        string displayName,
        string token,
        CancellationToken cancellationToken = default);
}
