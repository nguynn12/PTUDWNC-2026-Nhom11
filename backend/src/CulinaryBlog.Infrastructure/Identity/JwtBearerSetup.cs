using System.Text;
using CulinaryBlog.Application.Common.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CulinaryBlog.Infrastructure.Identity;

/// <summary>
/// Cấu hình xác thực JWT Bearer (SRS NFR-SEC) và response 401 theo Phụ lục B:
/// access token hết hạn → <c>AUTH_TOKEN_EXPIRED</c> (frontend dựa vào mã này để tự gọi refresh);
/// thiếu token, token sai chữ ký/issuer/audience → <c>AUTH_TOKEN_INVALID</c>.
/// </summary>
public static class JwtBearerSetup
{
    public static void Configure(JwtBearerOptions options, JwtSettings settings)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(settings);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = settings.Issuer,
            ValidAudience = settings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret)),
            // Token hết hạn là hết hạn ngay, không cộng thêm 5 phút mặc định (TTL đúng 15 phút).
            ClockSkew = TimeSpan.Zero,
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = OnChallengeAsync,
        };
    }

    public static async Task OnChallengeAsync(JwtBearerChallengeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Tự ghi response thay cho 401 rỗng mặc định.
        context.HandleResponse();

        var expired = context.AuthenticateFailure is SecurityTokenExpiredException;
        var httpContext = context.HttpContext;

        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        httpContext.Response.Headers.WWWAuthenticate = expired
            ? "Bearer error=\"invalid_token\", error_description=\"The token expired\""
            : "Bearer";

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Type = expired ? ErrorCodes.AuthTokenExpired : ErrorCodes.AuthTokenInvalid,
            Title = "Unauthorized",
            Detail = expired
                ? "Access token đã hết hạn. Vui lòng làm mới token."
                : "Bạn chưa đăng nhập hoặc access token không hợp lệ.",
            Instance = httpContext.Request.Path,
        };

        var problemDetailsService = httpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
        await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
        });
    }
}
