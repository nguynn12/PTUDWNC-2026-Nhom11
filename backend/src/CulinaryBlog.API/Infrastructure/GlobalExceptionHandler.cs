namespace CulinaryBlog.API.Infrastructure;

using CulinaryBlog.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Trình xử lý ngoại lệ toàn cục cho Minimal API theo chuẩn RFC 7807 Problem Details.
/// Ánh xạ các ngoại lệ nghiệp vụ sang HTTP status code tương ứng (400, 403, 404, 409, 422).
/// Tuân thủ nghiêm ngặt quyết định kiến trúc RESOLVED-CONFLICTS.md (C3, C6).
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Ngoại lệ xảy ra trong quá trình xử lý yêu cầu: {Message}", exception.Message);

        var (statusCode, title) = exception switch
        {
            ValidationException => (StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"),
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            BadRequestException => (StatusCodes.Status400BadRequest, "Bad Request"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        if (exception is ValidationException validationEx && validationEx.Errors.Count > 0)
        {
            problemDetails.Extensions["errors"] = validationEx.Errors;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
