using CulinaryBlog.Application.Common.Models;

namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>
/// Bọc ASP.NET Core Identity (UserManager) cho tầng Application — RESOLVED-CONFLICTS D7.
/// </summary>
public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<bool> AuthorizeAsync(string userId, string policyName);

    // Auth specific methods
    /// <summary>Tạo tài khoản mới và gán role Author (SRS FR-AUTH-001).</summary>
    Task<CreateUserResult> CreateUserAsync(string email, string password, string displayName);

    /// <summary>
    /// Kiểm tra đăng nhập theo đúng thứ tự: lockout → mật khẩu (tăng bộ đếm sai) → IsActive.
    /// Dự án dùng AddIdentityCore (không có SignInManager) nên phải tự xử lý lockout ở đây.
    /// </summary>
    Task<CredentialCheckResult> CheckCredentialsAsync(string email, string password);

    Task<UserAccount?> GetUserDetailsByEmailAsync(string email);
    Task<UserAccount?> GetUserDetailsByIdAsync(string userId);
    Task<bool> UpdateProfileAsync(string userId, string displayName, string? bio, string? avatarUrl);
    Task<bool> ToggleUserStatusAsync(string userId, bool isActive);
    Task<string?> GeneratePasswordResetTokenAsync(string email);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
    /// <summary>Tạo token xác nhận email cho user (null nếu user không tồn tại).</summary>
    Task<string?> GenerateEmailConfirmationTokenAsync(string userId);

    /// <summary>
    /// SRS FR-AUTH-008: xác nhận email theo <paramref name="userId"/> + token. Idempotent — email đã
    /// xác nhận trước đó vẫn trả true. Trả false khi user không tồn tại hoặc token sai/hết hạn.
    /// </summary>
    Task<bool> ConfirmEmailAsync(string userId, string token);
}
