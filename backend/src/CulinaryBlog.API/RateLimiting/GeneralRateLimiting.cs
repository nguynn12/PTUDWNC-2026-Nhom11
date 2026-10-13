using System.Threading.RateLimiting;
using CulinaryBlog.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CulinaryBlog.API.RateLimiting;

/// <summary>
/// Rate Limiting chung cho toàn bộ các API (/api/v1): 100 request/phút/IP.
/// Vượt ngưỡng trả về mã 429 RFC 7807 Problem Details (RATE_LIMIT_EXCEEDED) kèm header Retry-After,
/// X-RateLimit-Limit và X-RateLimit-Remaining theo yêu cầu Buổi 5.
/// </summary>
public static class GeneralRateLimiting
{
    public const string PolicyName = "General";
    public const int PermitLimit = 100;
    public static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    /// <summary>Phân vùng Fixed Window theo IP của client (100 request/phút).</summary>
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

    public static string ClientKey(HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor) &&
            !string.IsNullOrWhiteSpace(forwardedFor))
        {
            return forwardedFor.ToString().Split(',')[0].Trim();
        }

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    /// <summary>Xử lý khi request bị từ chối do vượt ngưỡng Rate Limit (HTTP 429).</summary>
    public static async ValueTask OnRejectedAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var httpContext = context.HttpContext;
        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        // Đính kèm các Header X-RateLimit theo chuẩn yêu cầu Lab 5
        httpContext.Response.Headers["X-RateLimit-Limit"] = PermitLimit.ToString();
        httpContext.Response.Headers["X-RateLimit-Remaining"] = "0";

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
            Detail = $"Bạn đã gửi quá giới hạn {PermitLimit} yêu cầu mỗi phút. Vui lòng thử lại sau.",
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
