using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core Command Interceptor theo chuẩn NFR-PERF:
/// Tự động đo lường thời gian thực thi của mọi câu truy vấn SQL và ghi cảnh báo Log Warning khi thời gian vượt quá ngưỡng 100 ms.
/// </summary>
public class SlowQueryInterceptor : DbCommandInterceptor
{
    public const long DefaultSlowQueryThresholdMs = 100;
    private readonly long _thresholdMs;
    private readonly ILogger<SlowQueryInterceptor> _logger;

    public SlowQueryInterceptor(
        ILogger<SlowQueryInterceptor> logger,
        long thresholdMs = DefaultSlowQueryThresholdMs)
    {
        _logger = logger;
        _thresholdMs = thresholdMs;
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        CheckAndLogSlowQuery(command.CommandText, eventData.Duration);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        CheckAndLogSlowQuery(command.CommandText, eventData.Duration);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override object? ScalarExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result)
    {
        CheckAndLogSlowQuery(command.CommandText, eventData.Duration);
        return base.ScalarExecuted(command, eventData, result);
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        CheckAndLogSlowQuery(command.CommandText, eventData.Duration);
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result)
    {
        CheckAndLogSlowQuery(command.CommandText, eventData.Duration);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        CheckAndLogSlowQuery(command.CommandText, eventData.Duration);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    public bool CheckAndLogSlowQuery(string commandText, TimeSpan duration)
    {
        var durationMs = duration.TotalMilliseconds;
        if (durationMs > _thresholdMs)
        {
            _logger.LogWarning(
                "[NFR-PERF] Phát hiện truy vấn SQL EF Core chậm ({Duration:F1} ms > {Threshold} ms): {CommandText}",
                durationMs,
                _thresholdMs,
                commandText);
            return true;
        }

        return false;
    }
}
