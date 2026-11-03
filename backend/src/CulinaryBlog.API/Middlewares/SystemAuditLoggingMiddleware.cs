using System.Diagnostics;
using CulinaryBlog.Infrastructure.Persistence;

namespace CulinaryBlog.API.Middlewares;

public sealed class SystemAuditLoggingMiddleware(
    RequestDelegate next,
    ILogger<SystemAuditLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, CulinaryBlogDbContext dbContext)
    {
        if (!context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        Exception? requestException = null;
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            requestException = exception;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = requestException is null
                ? context.Response.StatusCode
                : StatusCodes.Status500InternalServerError;
            var level = statusCode >= 500
                ? "Error"
                : statusCode >= 400
                    ? "Warning"
                    : "Information";

            dbContext.SystemLogs.Add(new Domain.Entities.SystemLog
            {
                UserId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    ?? context.User.FindFirst("sub")?.Value,
                Method = context.Request.Method,
                Path = context.Request.Path.Value ?? string.Empty,
                StatusCode = statusCode,
                Level = level,
                Message = requestException?.GetType().Name,
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                DurationMs = stopwatch.ElapsedMilliseconds,
                CreatedAt = DateTimeOffset.UtcNow,
            });

            try
            {
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Không thể lưu nhật ký hệ thống cho {Method} {Path}", context.Request.Method, context.Request.Path);
            }
        }
    }
}
