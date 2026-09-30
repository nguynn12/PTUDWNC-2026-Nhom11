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
    Task<(bool Succeeded, string? Error, string UserId)> CreateUserAsync(string email, string password, string displayName);

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
    Task<string?> GenerateEmailConfirmationTokenAsync(string email);
    Task<bool> ConfirmEmailAsync(string email, string token);
}
