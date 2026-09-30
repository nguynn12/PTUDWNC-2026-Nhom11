using CulinaryBlog.Application.Common.Interfaces;
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

    public async Task<bool> CheckPasswordAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return false;
        
        return await _userManager.CheckPasswordAsync(user, password);
    }
}
