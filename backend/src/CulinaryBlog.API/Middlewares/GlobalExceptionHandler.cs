namespace CulinaryBlog.API.Middlewares;

using CulinaryBlog.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Xử lý ngoại lệ toàn cục theo chuẩn RFC 7807 Problem Details (application/problem+json)
/// và ánh xạ các Application Error Codes chuẩn hóa theo SRS Phụ lục B và RESOLVED-CONFLICTS.
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Đã xảy ra ngoại lệ khi xử lý request: {Path}", httpContext.Request.Path);

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
            BusinessRuleValidationException businessRuleEx => (
                StatusCodes.Status422UnprocessableEntity,
                businessRuleEx.ErrorCode,
                "Unprocessable Entity",
                businessRuleEx.Message,
                null
            ),
            NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                string.IsNullOrWhiteSpace(notFoundEx.ErrorCode) ? "RESOURCE_NOT_FOUND" : notFoundEx.ErrorCode,
                "Not Found",
                notFoundEx.Message,
                null
            ),
            ForbiddenException forbiddenEx => (
                StatusCodes.Status403Forbidden,
                string.IsNullOrWhiteSpace(forbiddenEx.ErrorCode) ? "FORBIDDEN" : forbiddenEx.ErrorCode,
                "Forbidden",
                forbiddenEx.Message,
                null
            ),
            ConflictException conflictEx => (
                StatusCodes.Status409Conflict,
                string.IsNullOrWhiteSpace(conflictEx.ErrorCode) ? "CONFLICT_ERROR" : conflictEx.ErrorCode,
                "Conflict",
                conflictEx.Message,
                null
            ),
            DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                "RECIPE_CONCURRENCY_CONFLICT",
                "Conflict",
                "Dữ liệu đã bị thay đổi bởi phiên làm việc khác. Vui lòng tải lại trang.",
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

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
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
