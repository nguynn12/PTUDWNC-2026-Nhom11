using System.Security.Claims;
using CulinaryBlog.Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace CulinaryBlog.Infrastructure.Identity;

/// <summary>
/// Các Authorization Policy dùng chung cho toàn hệ thống (SRS mục 2.3 — phân quyền 3 tầng).
/// Admin có toàn quyền của Author và được bypass điều kiện xác nhận email.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>Claim do JwtService phát hành: "true" khi email đã xác nhận.</summary>
    public const string EmailVerifiedClaim = "email_verified";

    public static void Configure(AuthorizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // Chỉ Admin.
        options.AddPolicy(Roles.Admin, policy => policy.RequireRole(Roles.Admin));

        // Author hoặc Admin (Admin có mọi quyền của Author — SRS 2.3).
        options.AddPolicy(Roles.Author, policy => policy.RequireRole(Roles.Author, Roles.Admin));

        // Email đã xác nhận HOẶC Admin (FR-AUTH-008: "Chỉ user có email confirmed hoặc Admin
        // mới được publish Recipe").
        options.AddPolicy(Policies.VerifiedAuthor, policy => policy
            .RequireAuthenticatedUser()
            .RequireAssertion(context => IsVerifiedAuthor(context.User)));
    }

    public static bool IsVerifiedAuthor(ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (user.IsInRole(Roles.Admin))
        {
            return true;
        }

        return user.IsInRole(Roles.Author) && user.HasClaim(EmailVerifiedClaim, "true");
    }
}
