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

        var (statusCode, type, title, detail, errors) = Map(exception);

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

    public static (int StatusCode, string Type, string Title, string Detail, IDictionary<string, string[]>? Errors) Map(Exception exception)
    {
        if (AuthExceptionHandler.Map(exception) is { } authError)
        {
            return (authError.StatusCode, authError.Type, authError.Title, authError.Detail, null);
        }

        return exception switch
        {
            FluentValidation.ValidationException validationEx => (
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
            CulinaryBlog.Application.Common.Exceptions.ValidationException appValEx => (
                StatusCodes.Status422UnprocessableEntity,
                "VALIDATION_ERROR",
                "Unprocessable Entity",
                appValEx.Message,
                appValEx.Errors.ToDictionary(g => ToCamelCase(g.Key), g => g.Value)
            ),
            BusinessRuleValidationException businessRuleEx => (
                StatusCodes.Status422UnprocessableEntity,
                businessRuleEx.ErrorCode ?? "BUSINESS_RULE_VIOLATION",
                "Unprocessable Entity",
                businessRuleEx.Message,
                null
            ),
            CulinaryBlog.Domain.Exceptions.RecipeIncompletePublishException publishEx => (
                StatusCodes.Status422UnprocessableEntity,
                publishEx.ErrorCode ?? "RECIPE_PUBLISH_INCOMPLETE",
                "Unprocessable Entity",
                publishEx.Message,
                null
            ),
            CulinaryBlog.Domain.Exceptions.OwnershipViolationException ownershipEx => (
                StatusCodes.Status403Forbidden,
                ownershipEx.ErrorCode ?? "RECIPE_FORBIDDEN",
                "Forbidden",
                ownershipEx.Message,
                null
            ),
            NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                string.IsNullOrWhiteSpace(notFoundEx.ErrorCode) ? "RESOURCE_NOT_FOUND" : notFoundEx.ErrorCode,
                "Not Found",
                notFoundEx.Message,
                null
            ),
            CulinaryBlog.Domain.Exceptions.EntityNotFoundException entityNotFoundEx => (
                StatusCodes.Status404NotFound,
                string.IsNullOrWhiteSpace(entityNotFoundEx.ErrorCode) ? "RESOURCE_NOT_FOUND" : entityNotFoundEx.ErrorCode,
                "Not Found",
                entityNotFoundEx.Message,
                null
            ),
            ForbiddenException forbiddenEx => (
                StatusCodes.Status403Forbidden,
                string.IsNullOrWhiteSpace(forbiddenEx.ErrorCode) ? "FORBIDDEN" : forbiddenEx.ErrorCode,
                "Forbidden",
                forbiddenEx.Message,
                null
            ),
            UnauthorizedException unauthorizedEx => (
                StatusCodes.Status401Unauthorized,
                string.IsNullOrWhiteSpace(unauthorizedEx.ErrorCode) ? "UNAUTHORIZED" : unauthorizedEx.ErrorCode,
                "Unauthorized",
                unauthorizedEx.Message,
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
            LockedException lockedEx => (
                StatusCodes.Status423Locked,
                string.IsNullOrWhiteSpace(lockedEx.ErrorCode) ? ErrorCodes.AuthAccountLocked : lockedEx.ErrorCode,
                "Locked",
                lockedEx.Message,
                null
            ),
            BadRequestException badRequestEx => (
                StatusCodes.Status400BadRequest,
                ErrorCodes.MalformedRequest,
                "Bad Request",
                badRequestEx.Message,
                null
            ),
            CulinaryBlog.Domain.Exceptions.DomainException domainEx => (
                StatusCodes.Status400BadRequest,
                domainEx.ErrorCode ?? "DOMAIN_ERROR",
                "Domain Rule Violation",
                domainEx.Message,
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
