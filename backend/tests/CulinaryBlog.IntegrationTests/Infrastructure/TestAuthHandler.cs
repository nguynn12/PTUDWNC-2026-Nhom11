using System.Security.Claims;
using System.Text.Encodings.Web;
using CulinaryBlog.Domain.Constants;
using CulinaryBlog.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.IntegrationTests.Infrastructure;

/// <summary>
/// Handler xác thực giả lập phục vụ chạy kiểm thử tích hợp (Integration Tests) tự động.
/// Đọc header 'X-Test-Role' hoặc 'Authorization: Bearer test-{role}-token' để phân vai Admin / Author / Guest.
/// </summary>
public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationScheme = "TestScheme";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // 1. Kiểm tra header X-Test-Role (tiện lợi nhất khi viết test)
        string? role = null;
        if (Request.Headers.TryGetValue("X-Test-Role", out var roleHeader))
        {
            role = roleHeader.ToString();
        }
        // 2. Kiểm tra header Authorization: Bearer test-{role}-token
        else if (Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var headerValue = authHeader.ToString();
            if (headerValue.StartsWith("Bearer test-admin", StringComparison.OrdinalIgnoreCase))
            {
                role = Roles.Admin;
            }
            else if (headerValue.StartsWith("Bearer test-author", StringComparison.OrdinalIgnoreCase))
            {
                role = Roles.Author;
            }
        }

        // Nếu không có header chỉ định role -> Coi như khách vãng lai (Anonymous)
        if (string.IsNullOrWhiteSpace(role))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userId = role == Roles.Admin ? "00000000-0000-0000-0000-000000000001" : "00000000-0000-0000-0000-000000000002";
        var userName = role == Roles.Admin ? "admin_test" : "author_test";
        var email = role == Roles.Admin ? "admin@culinary.local" : "author@culinary.local";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role),
            new(AuthorizationPolicies.EmailVerifiedClaim, "true"),
        };

        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
