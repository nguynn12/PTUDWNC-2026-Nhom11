using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;

namespace CulinaryBlog.UnitTests.Auth;

/// <summary>IIdentityService giả lập cho unit test handler — chỉ cài các hàm test cần dùng.</summary>
internal sealed class FakeIdentityService : IIdentityService
{
    public CredentialCheckResult CredentialResult { get; set; } = new(CredentialCheckStatus.InvalidCredentials);

    public CreateUserResult CreateResult { get; set; } = CreateUserResult.Success("new-user");

    public UserAccount? Account { get; set; }

    public Task<CredentialCheckResult> CheckCredentialsAsync(string email, string password) =>
        Task.FromResult(CredentialResult);

    public Task<CreateUserResult> CreateUserAsync(string email, string password, string displayName) =>
        Task.FromResult(CreateResult);

    public Task<UserAccount?> GetUserDetailsByEmailAsync(string email) => Task.FromResult(Account);

    public Task<UserAccount?> GetUserDetailsByIdAsync(string userId) => Task.FromResult(Account);

    public Task<string?> GetUserNameAsync(string userId) => throw new NotSupportedException();

    public Task<bool> IsInRoleAsync(string userId, string role) => throw new NotSupportedException();

    public Task<bool> AuthorizeAsync(string userId, string policyName) => throw new NotSupportedException();

    public Task<bool> UpdateProfileAsync(string userId, string displayName, string? bio, string? avatarUrl) =>
        throw new NotSupportedException();

    public Task<bool> ToggleUserStatusAsync(string userId, bool isActive) => throw new NotSupportedException();

    public Task<string?> GeneratePasswordResetTokenAsync(string email) => throw new NotSupportedException();

    public Task<bool> ResetPasswordAsync(string email, string token, string newPassword) => throw new NotSupportedException();

    public Task<string?> GenerateEmailConfirmationTokenAsync(string email) => throw new NotSupportedException();

    public Task<bool> ConfirmEmailAsync(string email, string token) => throw new NotSupportedException();
}

/// <summary>IJwtService giả lập — trả chuỗi cố định.</summary>
internal sealed class FakeJwtService : IJwtService
{
    public int AccessTokenLifetimeSeconds => 900;

    public string GenerateAccessToken(string userId, string email, IEnumerable<string> roles, bool emailConfirmed) => "access-token";

    public (string TokenHash, string RawToken) GenerateRefreshToken() => (new string('a', 64), "raw-refresh-token");
}
