using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Domain.Constants;
using Microsoft.AspNetCore.Identity;

namespace CulinaryBlog.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.DisplayName;
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        // Note: Policy-based authorization should typically be handled via [Authorize(Policy = "...")]
        // and IAuthorizationService. This is a placeholder for custom imperative authorization.
        throw new NotImplementedException();
    }
    
    public async Task<CreateUserResult> CreateUserAsync(string email, string password, string displayName)
    {
        // Email unique, không phân biệt hoa thường (Identity so theo NormalizedEmail).
        if (await _userManager.FindByEmailAsync(email) != null)
        {
            return CreateUserResult.Duplicate();
        }

        var user = ApplicationUser.Create(email, displayName);
        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            // Trường hợp 2 request đăng ký cùng email chạy song song.
            if (result.Errors.Any(error => error.Code is "DuplicateEmail" or "DuplicateUserName"))
            {
                return CreateUserResult.Duplicate();
            }

            var errors = result.Errors
                .GroupBy(error => FieldForIdentityError(error.Code))
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.Description).ToArray(),
                    StringComparer.Ordinal);

            return CreateUserResult.Failed(errors);
        }

        // SRS FR-AUTH-001: user mới luôn có role Author.
        var roleResult = await _userManager.AddToRoleAsync(user, Roles.Author);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            throw new InvalidOperationException(
                "Không gán được role Author cho tài khoản mới: " +
                string.Join("; ", roleResult.Errors.Select(error => error.Description)));
        }

        return CreateUserResult.Success(user.Id);
    }

    private static string FieldForIdentityError(string code)
    {
        if (code.StartsWith("Password", StringComparison.Ordinal))
        {
            return "password";
        }

        return code is "InvalidEmail" or "InvalidUserName" ? "email" : "account";
    }

    public async Task<CredentialCheckResult> CheckCredentialsAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return new CredentialCheckResult(CredentialCheckStatus.InvalidCredentials);
        }

        // 1. Đang bị khoá tạm → từ chối NGAY, không so mật khẩu (tránh dò mật khẩu trong lúc khoá).
        if (await _userManager.IsLockedOutAsync(user))
        {
            return new CredentialCheckResult(CredentialCheckStatus.LockedOut, LockoutEnd: user.LockoutEnd);
        }

        // 2. Sai mật khẩu → tăng AccessFailedCount; lần sai thứ 5 Identity tự đặt LockoutEnd (15 phút).
        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            await _userManager.AccessFailedAsync(user);

            return await _userManager.IsLockedOutAsync(user)
                ? new CredentialCheckResult(CredentialCheckStatus.LockedOut, LockoutEnd: user.LockoutEnd)
                : new CredentialCheckResult(CredentialCheckStatus.InvalidCredentials);
        }

        // 3. Mật khẩu đúng nhưng Admin đã vô hiệu hoá (kiểm tra SAU mật khẩu để không lộ trạng thái).
        if (!user.IsActive)
        {
            return new CredentialCheckResult(CredentialCheckStatus.Disabled);
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        return new CredentialCheckResult(CredentialCheckStatus.Success, await ToAccountAsync(user));
    }

    public const string GoogleProvider = "Google";

    public async Task<GoogleLoginResult> FindOrCreateGoogleUserAsync(GoogleUserInfo googleUser)
    {
        ArgumentNullException.ThrowIfNull(googleUser);

        // 1. Đã liên kết Google trước đó.
        var user = await _userManager.FindByLoginAsync(GoogleProvider, googleUser.Subject);
        if (user != null)
        {
            return new GoogleLoginResult(GoogleLoginStatus.Success, await ToAccountAsync(user));
        }

        var loginInfo = new UserLoginInfo(GoogleProvider, googleUser.Subject, GoogleProvider);

        // 2. Email đã có tài khoản → chỉ liên kết khi Google đã xác minh email.
        user = await _userManager.FindByEmailAsync(googleUser.Email);
        if (user != null)
        {
            if (!googleUser.EmailVerified)
            {
                return new GoogleLoginResult(GoogleLoginStatus.EmailNotVerified);
            }

            EnsureSucceeded(await _userManager.AddLoginAsync(user, loginInfo), "liên kết tài khoản Google");

            if (!user.EmailConfirmed)
            {
                // Google đã xác minh email thay chúng ta.
                user.EmailConfirmed = true;
                EnsureSucceeded(await _userManager.UpdateAsync(user), "xác nhận email");
            }

            return new GoogleLoginResult(GoogleLoginStatus.Success, await ToAccountAsync(user));
        }

        // 3. Chưa có tài khoản → tạo mới (không có mật khẩu), role Author.
        user = ApplicationUser.CreateFromGoogle(
            googleUser.Email,
            BuildDisplayName(googleUser),
            googleUser.PictureUrl is { Length: <= MaxAvatarUrlLength } picture ? picture : null);
        user.EmailConfirmed = googleUser.EmailVerified;

        EnsureSucceeded(await _userManager.CreateAsync(user), "tạo tài khoản từ Google");
        EnsureSucceeded(await _userManager.AddToRoleAsync(user, Roles.Author), "gán role Author");
        EnsureSucceeded(await _userManager.AddLoginAsync(user, loginInfo), "liên kết tài khoản Google");

        return new GoogleLoginResult(GoogleLoginStatus.Success, await ToAccountAsync(user));
    }

    private const int MaxAvatarUrlLength = 500;
    private const int MaxDisplayNameLength = 100;

    /// <summary>Tên hiển thị 2-100 ký tự: lấy claim <c>name</c>, thiếu thì dùng phần trước @ của email.</summary>
    public static string BuildDisplayName(GoogleUserInfo googleUser)
    {
        ArgumentNullException.ThrowIfNull(googleUser);

        var name = googleUser.Name?.Trim();
        if (string.IsNullOrEmpty(name) || name.Length < 2)
        {
            name = googleUser.Email.Split('@')[0].Trim();
        }

        if (name.Length < 2)
        {
            name = googleUser.Email.Trim();
        }

        return name.Length > MaxDisplayNameLength ? name[..MaxDisplayNameLength] : name;
    }

    private static void EnsureSucceeded(IdentityResult result, string action)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Không {action} được: " + string.Join("; ", result.Errors.Select(error => error.Description)));
        }
    }

    public async Task<UserAccount?> GetUserDetailsByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user == null ? null : await ToAccountAsync(user);
    }

    public async Task<UserAccount?> GetUserDetailsByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user == null ? null : await ToAccountAsync(user);
    }

    public async Task<bool> UpdateProfileAsync(string userId, string? displayName, string? bio, string? avatarUrl)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        user.UpdateProfile(displayName, avatarUrl, bio);
        var result = await _userManager.UpdateAsync(user);
        
        return result.Succeeded;
    }

    public async Task<bool> ToggleUserStatusAsync(string userId, bool isActive)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        if (isActive)
        {
            user.Activate();
        }
        else
        {
            user.Deactivate();
        }
        
        var result = await _userManager.UpdateAsync(user);
        
        return result.Succeeded;
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return null;
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return false;
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            return false;
        }

        // Người dùng đã chứng minh sở hữu email → mở khoá tạm (nếu đang bị khoá do nhập sai).
        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.ResetAccessFailedCountAsync(user);
        return true;
    }

    public async Task<string?> GenerateEmailConfirmationTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;
        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<bool> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        // Idempotent (FR-AUTH-008): bấm lại link xác nhận lần 2 không báo lỗi.
        if (user.EmailConfirmed) return true;

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    private async Task<UserAccount> ToAccountAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new UserAccount(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName,
            user.AvatarUrl,
            user.Bio,
            roles.ToList(),
            user.EmailConfirmed,
            user.IsActive,
            user.CreatedAt);
    }
}
