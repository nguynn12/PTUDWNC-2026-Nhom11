namespace CulinaryBlog.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<bool> AuthorizeAsync(string userId, string policyName);
    
    // Auth specific methods
    Task<(bool Succeeded, string? Error, string UserId)> CreateUserAsync(string email, string password, string displayName);
    Task<bool> CheckPasswordAsync(string email, string password);
    Task<(string Id, string Email, string DisplayName, IEnumerable<string> Roles, bool EmailConfirmed)?> GetUserDetailsByEmailAsync(string email);
    Task<(string Id, string Email, string DisplayName, IEnumerable<string> Roles, bool EmailConfirmed)?> GetUserDetailsByIdAsync(string userId);
    Task<bool> UpdateProfileAsync(string userId, string displayName, string? bio, string? avatarUrl);
    Task<bool> ToggleUserStatusAsync(string userId, bool isActive);
}
