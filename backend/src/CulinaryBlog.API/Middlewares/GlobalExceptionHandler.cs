using CulinaryBlog.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CulinaryBlog.API.Middlewares;

/// <summary>
/// Xử lý ngoại lệ toàn cục theo chuẩn RFC 7807 Problem Details (application/problem+json)
/// và mã lỗi Application Error Codes chuẩn hóa theo SRS Phụ lục B.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Đã xảy ra ngoại lệ khi xử lý request: {Path}", httpContext.Request.Path);

        var (statusCode, type, title, detail, errors) = exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status422UnprocessableEntity,
                "VALIDATION_ERROR",
                "Unprocessable Entity",
                "Dữ liệu gửi lên không hợp lệ hoặc vi phạm quy tắc xác thực.",
                validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => ToCamelCase(g.Key),
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    )
            ),
            NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                string.IsNullOrWhiteSpace(notFoundEx.ErrorCode) ? "RESOURCE_NOT_FOUND" : notFoundEx.ErrorCode,
                "Not Found",
                notFoundEx.Message,
                null
            ),
            ConflictException conflictEx => (
                StatusCodes.Status409Conflict,
                string.IsNullOrWhiteSpace(conflictEx.ErrorCode) ? "CONFLICT_ERROR" : conflictEx.ErrorCode,
                "Conflict",
                conflictEx.Message,
                null
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "INTERNAL_SERVER_ERROR",
                "Internal Server Error",
                "Đã xảy ra lỗi không mong muốn trên hệ thống máy chủ.",
                null
            )
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Type = type,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        if (errors != null)
        {
            problemDetails.Extensions["errors"] = errors;
        }

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
        {
            return str;
        }

        return char.ToLowerInvariant(str[0]) + str[1..];
    }
}
