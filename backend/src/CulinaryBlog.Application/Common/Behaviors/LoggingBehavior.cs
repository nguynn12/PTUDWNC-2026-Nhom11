namespace CulinaryBlog.Application.Common.Behaviors;

using System.Diagnostics;
using System.Reflection;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

/// <summary>
/// MediatR Pipeline Behavior tự động ghi log giám sát thời gian xử lý cho mọi Command và Query.
/// Cảnh báo cấp độ Warning khi thời gian vượt quá ngưỡng 500ms (FR-OBS-002, SRS Lab 8).
/// Tích hợp ghi log kiểm toán có cấu trúc (NFR-SEC: UserId, ResourceId, Action, Timestamp, Result) cho mọi thao tác ghi.
/// Bảo mật thông tin: không serialize các thuộc tính nhạy cảm (mật khẩu, refresh token).
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    ICurrentUserService currentUserService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int SlowThresholdMilliseconds = 500;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = currentUserService.UserId ?? "Anonymous";
        var isCommand = requestName.EndsWith("Command", StringComparison.OrdinalIgnoreCase);

        logger.LogInformation(
            "MediatR Handling {RequestName} by User [{UserId}]",
            requestName,
            userId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            var elapsedMs = stopwatch.ElapsedMilliseconds;

            if (elapsedMs > SlowThresholdMilliseconds)
            {
                logger.LogWarning(
                    "[PERFORMANCE WARNING] Long running request: {RequestName} ({ElapsedMilliseconds} ms) by User [{UserId}]",
                    requestName,
                    elapsedMs,
                    userId);
            }
            else
            {
                logger.LogInformation(
                    "MediatR Handled {RequestName} successfully in {ElapsedMilliseconds} ms by User [{UserId}]",
                    requestName,
                    elapsedMs,
                    userId);
            }

            // NFR-SEC: Ghi log kiểm toán có cấu trúc cho các thao tác ghi (Command)
            if (isCommand)
            {
                var resourceId = ExtractResourceId(request, response);
                logger.LogInformation(
                    "[AUDIT] Action: {Action} | UserId: {UserId} | ResourceId: {ResourceId} | Result: {Result} | Timestamp: {Timestamp}",
                    requestName,
                    userId,
                    resourceId,
                    "Success",
                    DateTimeOffset.UtcNow);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            if (isCommand)
            {
                var resourceId = ExtractResourceId(request, default);
                logger.LogWarning(
                    ex,
                    "[AUDIT] Action: {Action} | UserId: {UserId} | ResourceId: {ResourceId} | Result: {Result} | Reason: {Reason} | Timestamp: {Timestamp}",
                    requestName,
                    userId,
                    resourceId,
                    "Failed",
                    ex.Message,
                    DateTimeOffset.UtcNow);
            }

            logger.LogError(
                ex,
                "MediatR Request {RequestName} failed after {ElapsedMilliseconds} ms by User [{UserId}]: {ErrorMessage}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                userId,
                ex.Message);
            throw;
        }
    }

    private static string ExtractResourceId(TRequest request, TResponse? response)
    {
        // 1. Thử lấy từ Request (Id, RecipeId, CategoryId...)
        var requestIdProp = request.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)
            ?? request.GetType().GetProperty("RecipeId", BindingFlags.Public | BindingFlags.Instance)
            ?? request.GetType().GetProperty("CategoryId", BindingFlags.Public | BindingFlags.Instance);

        if (requestIdProp != null)
        {
            var val = requestIdProp.GetValue(request);
            if (val != null) return val.ToString()!;
        }

        // 2. Thử lấy từ Response
        if (response != null)
        {
            var responseIdProp = response.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
            if (responseIdProp != null)
            {
                var val = responseIdProp.GetValue(response);
                if (val != null) return val.ToString()!;
            }
        }

        return "N/A";
    }
}
