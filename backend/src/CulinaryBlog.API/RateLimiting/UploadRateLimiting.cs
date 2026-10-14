using System.Threading.RateLimiting;
using CulinaryBlog.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CulinaryBlog.API.RateLimiting;

/// <summary>
/// Rate limiting cho tác vụ tải lên hình ảnh công thức nấu ăn: 5 request/phút/IP (hoặc client).
/// Mỗi IP có một cửa sổ cố định (Fixed Window) riêng 1 phút; vượt giới hạn trả về HTTP 429 Problem Details
/// chuẩn RFC 7807 với <c>type = RATE_LIMIT_EXCEEDED</c> kèm header Retry-After.
/// </summary>
public static class UploadRateLimiting
{
    public const string PolicyName = "Upload";
    public const int PermitLimit = 5;
    public static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    /// <summary>Chia partition theo địa chỉ IP của client (không có IP thì gom nhóm unknown).</summary>
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

    /// <summary>Ghi response 429 theo chuẩn RFC 7807 Problem Details.</summary>
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
            Detail = $"Bạn đã gửi quá {PermitLimit} yêu cầu tải lên tệp tin mỗi phút. Vui lòng thử lại sau.",
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
