namespace CulinaryBlog.API.Middlewares;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Bổ sung cho <see cref="GlobalExceptionHandler"/> dùng chung của nhóm: xử lý các lỗi riêng của
/// module Auth mà handler chung chưa ánh xạ (401, 423) và request sai cú pháp JSON (400).
/// PHẢI được đăng ký TRƯỚC GlobalExceptionHandler (IExceptionHandler chạy theo thứ tự đăng ký);
/// exception không thuộc nhóm này trả về false để GlobalExceptionHandler xử lý tiếp.
/// Định dạng response giống hệt GlobalExceptionHandler (RFC 7807, type = mã lỗi SRS Phụ lục B).
/// </summary>
public sealed class AuthExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<AuthExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var mapped = Map(exception);
        if (mapped is null)
        {
            return false;
        }

        var (statusCode, type, title, detail) = mapped.Value;

        logger.LogWarning("Request {Path} bị từ chối với mã lỗi {ErrorCode} ({StatusCode})",
            httpContext.Request.Path, type, statusCode);

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

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });
    }

    /// <summary>Trả null nếu exception không thuộc phạm vi handler này.</summary>
    public static (int StatusCode, string Type, string Title, string Detail)? Map(Exception exception) => exception switch
    {
        UnauthorizedException unauthorizedEx => (
            StatusCodes.Status401Unauthorized,
            string.IsNullOrWhiteSpace(unauthorizedEx.ErrorCode) ? ErrorCodes.AuthTokenInvalid : unauthorizedEx.ErrorCode,
            "Unauthorized",
            unauthorizedEx.Message
        ),
        InvalidTokenException tokenEx => (
            StatusCodes.Status401Unauthorized,
            tokenEx.ErrorCode ?? ErrorCodes.AuthTokenInvalid,
            "Unauthorized",
            tokenEx.Message
        ),
        LockedException lockedEx => (
            StatusCodes.Status423Locked,
            string.IsNullOrWhiteSpace(lockedEx.ErrorCode) ? ErrorCodes.AuthAccountLocked : lockedEx.ErrorCode,
            "Locked",
            lockedEx.Message
        ),
        BadHttpRequestException badRequestEx => (
            badRequestEx.StatusCode,
            ErrorCodes.MalformedRequest,
            "Bad Request",
            "Request không đúng định dạng (JSON hoặc tham số sai cú pháp)."
        ),
        _ => null
    };
}
