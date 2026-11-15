using System.Security.Claims;
using CulinaryBlog.Domain.Constants;
using CulinaryBlog.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using CulinaryBlog.API.Filters;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Http;

namespace CulinaryBlog.UnitTests.Auth;

/// <summary>SRS 2.3 + FR-AUTH-008: Admin có mọi quyền của Author và được bypass xác nhận email.</summary>
public sealed class AuthorizationPoliciesTests
{
    [Theory]
    [InlineData(false, "Admin", false)]
    [InlineData(false, "", false)]
    [InlineData(true, "Author", false)]
    [InlineData(true, "", false)]
    [InlineData(true, "Admin", true)]
    public void Hangfire_RequiresAuthenticatedAdmin_EvenOnLoopback(bool authenticated, string role, bool allowed)
    {
        var httpContext = new DefaultHttpContext();
        using var services = new ServiceCollection().BuildServiceProvider();
        httpContext.RequestServices = services;
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Loopback;
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Role, role)], authenticated ? "Test" : null));
        var dashboard = new AspNetCoreDashboardContext(new MemoryStorage(), new DashboardOptions(), httpContext);

        Assert.Equal(allowed, new HangfireAdminAuthorizationFilter().Authorize(dashboard));
        httpContext.Connection.RemoteIpAddress = null;
        Assert.Equal(allowed, new HangfireAdminAuthorizationFilter().Authorize(dashboard));
    }

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
