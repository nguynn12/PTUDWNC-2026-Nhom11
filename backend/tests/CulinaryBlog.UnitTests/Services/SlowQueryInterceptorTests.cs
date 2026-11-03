using CulinaryBlog.Infrastructure.Persistence.Interceptors;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CulinaryBlog.UnitTests.Services;

public class SlowQueryInterceptorTests
{
    private class TestLogger<T> : ILogger<T>
    {
        public List<string> Warnings { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Warning)
            {
                Warnings.Add(formatter(state, exception));
            }
        }
    }

    [Fact]
    public void CheckAndLogSlowQuery_Does_Not_Log_Warning_When_Query_Under_100ms()
    {
        var logger = new TestLogger<SlowQueryInterceptor>();
        var interceptor = new SlowQueryInterceptor(logger, thresholdMs: 100);
        var sql = "SELECT * FROM \"Recipes\" WHERE \"Status\" = 1";

        var isSlow = interceptor.CheckAndLogSlowQuery(sql, TimeSpan.FromMilliseconds(50));

        Assert.False(isSlow);
        Assert.Empty(logger.Warnings);
    }

    [Fact]
    public void CheckAndLogSlowQuery_Logs_Warning_When_Query_Exceeds_100ms()
    {
        var logger = new TestLogger<SlowQueryInterceptor>();
        var interceptor = new SlowQueryInterceptor(logger, thresholdMs: 100);
        var sql = "SELECT * FROM \"Recipes\" WHERE \"Status\" = 1";

        var isSlow = interceptor.CheckAndLogSlowQuery(sql, TimeSpan.FromMilliseconds(150));

        Assert.True(isSlow);
        Assert.Single(logger.Warnings);
        Assert.Contains("Phát hiện truy vấn SQL EF Core chậm", logger.Warnings[0]);
        Assert.Contains("150.0 ms > 100 ms", logger.Warnings[0]);
        Assert.Contains(sql, logger.Warnings[0]);
    }
}
