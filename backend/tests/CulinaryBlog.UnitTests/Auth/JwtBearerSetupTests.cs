using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using CulinaryBlog.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace CulinaryBlog.UnitTests.Auth;

/// <summary>
/// SRS NFR-SEC + Phụ lục B: access token có claim sub/email/roles/jti, được middleware JwtBearer
/// đọc đúng role; 401 phân biệt AUTH_TOKEN_EXPIRED và AUTH_TOKEN_INVALID.
/// </summary>
public sealed class JwtBearerSetupTests
{
    private static readonly JwtSettings Settings = new()
    {
        Secret = new string('k', 64),
        Issuer = "test-issuer",
        Audience = "test-audience",
    };

    private static JwtBearerOptions ConfiguredOptions()
    {
        var options = new JwtBearerOptions();
        JwtBearerSetup.Configure(options, Settings);
        return options;
    }

    [Fact]
    public void AccessToken_CoDuClaimSubEmailRolesJti()
    {
        var raw = new JwtService(Options.Create(Settings))
            .GenerateAccessToken("user-1", "admin@culinaryblog.local", ["Admin", "Author"], emailConfirmed: true);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(raw);

        Assert.Equal("user-1", token.Subject);
        Assert.Contains(token.Claims, c => c.Type == "email" && c.Value == "admin@culinaryblog.local");
        Assert.Contains(token.Claims, c => c.Type == "jti");
        Assert.Equal(["Admin", "Author"], token.Claims.Where(c => c.Type == "roles").Select(c => c.Value).Order());
    }

    [Fact]
    public async Task MiddlewareDocToken_NhanDungRoleVaUserId()
    {
        var options = ConfiguredOptions();
        var raw = new JwtService(Options.Create(Settings))
            .GenerateAccessToken("user-1", "admin@culinaryblog.local", ["Admin"], emailConfirmed: true);

        var result = await options.TokenHandlers[0].ValidateTokenAsync(raw, options.TokenValidationParameters);

        Assert.True(result.IsValid, result.Exception?.Message);
        var principal = new ClaimsPrincipal(result.ClaimsIdentity);
        Assert.True(principal.IsInRole("Admin"));
        Assert.False(principal.IsInRole("Author"));
        Assert.True(AuthorizationPolicies.IsVerifiedAuthor(principal));
        Assert.Equal("user-1",
            principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub"));
    }

    [Fact]
    public async Task TokenHetHan_Tra401AuthTokenExpired()
    {
        var (status, type) = await ChallengeAsync(new SecurityTokenExpiredException("expired"));

        Assert.Equal(StatusCodes.Status401Unauthorized, status);
        Assert.Equal("AUTH_TOKEN_EXPIRED", type);
    }

    [Fact]
    public async Task KhongCoTokenHoacTokenSai_Tra401AuthTokenInvalid()
    {
        Assert.Equal("AUTH_TOKEN_INVALID", (await ChallengeAsync(null)).Type);
        Assert.Equal("AUTH_TOKEN_INVALID", (await ChallengeAsync(new SecurityTokenInvalidSignatureException("sai"))).Type);
    }

    private static async Task<(int Status, string? Type)> ChallengeAsync(Exception? failure)
    {
        using var services = new ServiceCollection().AddLogging().AddProblemDetails().BuildServiceProvider();
        using var body = new MemoryStream();
        var httpContext = new DefaultHttpContext { RequestServices = services };
        httpContext.Request.Path = "/api/v1/auth/me";
        httpContext.Response.Body = body;

        var scheme = new AuthenticationScheme(JwtBearerDefaults.AuthenticationScheme, null, typeof(JwtBearerHandler));
        var context = new JwtBearerChallengeContext(httpContext, scheme, ConfiguredOptions(), new AuthenticationProperties())
        {
            AuthenticateFailure = failure,
        };

        await JwtBearerSetup.OnChallengeAsync(context);

        body.Position = 0;
        using var json = await JsonDocument.ParseAsync(body, cancellationToken: TestContext.Current.CancellationToken);
        return (httpContext.Response.StatusCode, json.RootElement.GetProperty("type").GetString());
    }
}
