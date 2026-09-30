using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
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
    
    public async Task<(bool Succeeded, string? Error, string UserId)> CreateUserAsync(string email, string password, string displayName)
    {
        var user = ApplicationUser.Create(email, displayName);
        var result = await _userManager.CreateAsync(user, password);
        
        if (result.Succeeded)
        {
            return (true, null, user.Id);
        }
        
        return (false, string.Join("; ", result.Errors.Select(e => e.Description)), string.Empty);
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

    public async Task<bool> UpdateProfileAsync(string userId, string displayName, string? bio, string? avatarUrl)
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
        return result.Succeeded;
    }

    public async Task<string?> GenerateEmailConfirmationTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return null;
        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<bool> ConfirmEmailAsync(string email, string token)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return false;
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
