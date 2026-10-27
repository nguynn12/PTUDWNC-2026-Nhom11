using System.Security.Claims;
using CulinaryBlog.Domain.Constants;
using CulinaryBlog.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CulinaryBlog.UnitTests.Auth;

/// <summary>SRS 2.3 + FR-AUTH-008: Admin có mọi quyền của Author và được bypass xác nhận email.</summary>
public sealed class AuthorizationPoliciesTests
{
    [Fact]
    public async Task VerifiedAuthor_AdminChuaXacNhanEmail_DuocPhep()
    {
        Assert.True(await AuthorizeAsync(Policies.VerifiedAuthor, Role(Roles.Admin), EmailVerified(false)));
    }

    [Fact]
    public async Task VerifiedAuthor_AuthorDaXacNhanEmail_DuocPhep()
    {
        Assert.True(await AuthorizeAsync(Policies.VerifiedAuthor, Role(Roles.Author), EmailVerified(true)));
    }

    [Fact]
    public async Task VerifiedAuthor_AuthorChuaXacNhanEmail_BiTuChoi()
    {
        Assert.False(await AuthorizeAsync(Policies.VerifiedAuthor, Role(Roles.Author), EmailVerified(false)));
    }

    [Fact]
    public async Task VerifiedAuthor_KhongCoRole_BiTuChoi()
    {
        Assert.False(await AuthorizeAsync(Policies.VerifiedAuthor, EmailVerified(true)));
    }

    [Fact]
    public async Task Author_AdminDuocPhep()
    {
        Assert.True(await AuthorizeAsync(Roles.Author, Role(Roles.Admin)));
    }

    [Fact]
    public async Task Author_AuthorDuocPhep()
    {
        Assert.True(await AuthorizeAsync(Roles.Author, Role(Roles.Author)));
    }

    [Fact]
    public async Task Admin_AuthorBiTuChoi()
    {
        Assert.False(await AuthorizeAsync(Roles.Admin, Role(Roles.Author)));
    }

    private static Claim Role(string role) => new(ClaimTypes.Role, role);

    private static Claim EmailVerified(bool verified) =>
        new(AuthorizationPolicies.EmailVerifiedClaim, verified ? "true" : "false");

    private static async Task<bool> AuthorizeAsync(string policy, params Claim[] claims)
    {
        using var services = new ServiceCollection()
            .AddLogging()
            .AddAuthorizationCore(AuthorizationPolicies.Configure)
            .BuildServiceProvider();

        var authorizationService = services.GetRequiredService<IAuthorizationService>();
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "Test"));

        var result = await authorizationService.AuthorizeAsync(user, policy);
        return result.Succeeded;
    }
}
