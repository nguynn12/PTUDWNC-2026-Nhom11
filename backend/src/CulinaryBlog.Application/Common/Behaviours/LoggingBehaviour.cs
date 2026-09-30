using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CulinaryBlog.Application.Common.Behaviours;

public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Handling {RequestName}", requestName);

        var response = await next();

        stopwatch.Stop();
        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            _logger.LogWarning("Long Running Request: {RequestName} ({ElapsedMilliseconds} milliseconds)", requestName, elapsedMilliseconds);
        }

        _logger.LogInformation("Handled {RequestName}", requestName);

        return response;
    }
}
