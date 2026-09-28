using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CulinaryBlog.API.ErrorHandling;

/// <summary>
/// Bắt mọi exception chưa được xử lý và trả RFC 7807 Problem Details thống nhất
/// (NFR-REL-002, SRS 5.2). Trường <c>type</c> chứa Application Error Code (Phụ lục B);
/// kèm <c>traceId</c> để đối chiếu với log.
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problem = ProblemDetailsMapper.Map(exception);

        if (problem.Status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Lỗi không xử lý được khi gọi {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else
        {
            logger.LogInformation(
                "Request {Method} {Path} kết thúc với lỗi {ErrorCode} ({Status})",
                httpContext.Request.Method,
                httpContext.Request.Path,
                problem.ErrorCode,
                problem.Status);
        }

        ProblemDetails details = problem.Errors is null
            ? new ProblemDetails()
            : new HttpValidationProblemDetails(
                problem.Errors.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal));

        details.Type = problem.ErrorCode;
        details.Title = problem.Title;
        details.Status = problem.Status;
        details.Detail = problem.Detail;
        details.Instance = httpContext.Request.Path;
        details.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = problem.Status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = details,
            Exception = exception,
        });
    }
}
