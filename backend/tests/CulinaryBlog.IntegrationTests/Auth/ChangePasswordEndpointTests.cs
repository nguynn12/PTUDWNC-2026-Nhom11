using System.Net;
using System.Net.Http.Json;
using CulinaryBlog.Infrastructure.Identity;
using CulinaryBlog.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CulinaryBlog.IntegrationTests.Auth;

public sealed class ChangePasswordEndpointTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task ChangePassword_KhongCoToken_Tra401()
    {
        var client = factory.CreateGuestClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/password/change",
            new { currentPassword = "OldPassword1!", newPassword = "NewPassword1!" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_CoTokenVaMatKhauDung_Tra200()
    {
        await EnsureAuthorIdentityAsync();
        var client = factory.CreateAuthorClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/password/change",
            new { currentPassword = "OldPassword1!", newPassword = "NewPassword1!" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponseContract>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(body.RefreshToken));
    }

    private async Task EnsureAuthorIdentityAsync()
    {
        factory.EnsureDatabaseSeeded();
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        if (await userManager.FindByIdAsync(CustomWebApplicationFactory.AuthorUserId) is not null)
        {
            return;
        }

        var user = ApplicationUser.Create("author@culinary.local", "Author Test");
        user.Id = CustomWebApplicationFactory.AuthorUserId;
        user.EmailConfirmed = true;
        var result = await userManager.CreateAsync(user, "OldPassword1!");
        Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(error => error.Description)));
    }

    private sealed record AuthResponseContract(string AccessToken, string RefreshToken);
}
