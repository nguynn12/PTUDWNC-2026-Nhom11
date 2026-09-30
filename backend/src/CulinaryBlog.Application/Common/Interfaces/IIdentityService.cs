namespace CulinaryBlog.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<bool> AuthorizeAsync(string userId, string policyName);
    
    // Auth specific methods
    Task<(bool Succeeded, string? Error, string UserId)> CreateUserAsync(string email, string password, string displayName);
    Task<bool> CheckPasswordAsync(string email, string password);
}
