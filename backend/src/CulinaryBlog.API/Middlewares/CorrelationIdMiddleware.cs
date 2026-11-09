namespace CulinaryBlog.API.Middlewares;

using Serilog.Context;

/// <summary>
/// Middleware đảm bảo mỗi HTTP request đều có một CorrelationId định danh duy nhất (FR-OBS-002, SRS Lab 8).
/// Đọc header 'X-Correlation-ID' từ client hoặc tự sinh mới nếu thiếu.
/// Đưa CorrelationId vào Serilog LogContext và trả ngược lại client qua response header.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string CorrelationIdHeader = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        // Gắn CorrelationId vào Response Header trước khi bắt đầu stream response
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
            {
                context.Response.Headers.Append(CorrelationIdHeader, correlationId);
            }
            return Task.CompletedTask;
        });

        // Đẩy CorrelationId vào LogContext của Serilog để mọi log trong request này đều mang mã định danh
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var headerValues) &&
            !string.IsNullOrWhiteSpace(headerValues.FirstOrDefault()))
        {
            return headerValues.First()!;
        }

        return Guid.NewGuid().ToString("D");
    }
}

