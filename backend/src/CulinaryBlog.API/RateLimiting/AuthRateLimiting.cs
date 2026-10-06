using System.Threading.RateLimiting;
using CulinaryBlog.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CulinaryBlog.API.RateLimiting;

/// <summary>
/// SRS NFR-SEC: "Auth rate limit: 10 request/phút/IP". Mỗi địa chỉ IP có một cửa sổ cố định riêng;
/// vượt giới hạn trả 429 Problem Details với <c>type = RATE_LIMIT_EXCEEDED</c> kèm header Retry-After.
/// </summary>
public static class AuthRateLimiting
{
    public const string PolicyName = "Auth";
    public const int PermitLimit = 10;
    public static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    /// <summary>Chia partition theo IP của client (không có IP thì gom chung một nhóm).</summary>
    public static RateLimitPartition<string> GetPartition(HttpContext httpContext) =>
        RateLimitPartition.GetFixedWindowLimiter(
            ClientKey(httpContext),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = PermitLimit,
                Window = Window,
                QueueLimit = 0,
                AutoReplenishment = true,
            });

    public static string ClientKey(HttpContext httpContext) =>
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    /// <summary>Ghi response 429 theo cùng định dạng RFC 7807 với các lỗi khác.</summary>
    public static async ValueTask OnRejectedAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var httpContext = context.HttpContext;
        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            httpContext.Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(
                System.Globalization.CultureInfo.InvariantCulture);
        }

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Type = ErrorCodes.RateLimitExceeded,
            Title = "Too Many Requests",
            Detail = $"Bạn đã gửi quá {PermitLimit} yêu cầu mỗi phút. Vui lòng thử lại sau.",
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
