using CulinaryBlog.Application.Common.Behaviours;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CulinaryBlog.UnitTests.Application.Behaviours;

public class LoggingBehaviourTests
{
    [Fact]
    public async Task ShouldLogHandlingAndHandled()
    {
        // Arrange
        var logger = new FakeLogger();
        var behaviour = new LoggingBehaviour<TestRequest, TestResponse>(logger);
        var request = new TestRequest();
        
        // Act
        await behaviour.Handle(request, delegate { return Task.FromResult(new TestResponse()); }, CancellationToken.None);

        // Assert
        Assert.Contains(logger.Logs, l => l.Contains("Handling TestRequest"));
        Assert.Contains(logger.Logs, l => l.Contains("Handled TestRequest"));
    }

    public class TestRequest { }
    public class TestResponse { }

    private class FakeLogger : ILogger<LoggingBehaviour<TestRequest, TestResponse>>
    {
        public List<string> Logs = new();
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Logs.Add(formatter(state, exception));
        }
    }
}
