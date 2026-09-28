using System.Text.Json;
using CulinaryBlog.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.API.ErrorHandling;

/// <summary>
/// Ánh xạ exception → (HTTP status, Application Error Code). Tách riêng khỏi
/// <see cref="GlobalExceptionHandler"/> để unit test được mà không cần HttpContext.
/// Quy tắc: SRS Phụ lục A/B + RESOLVED-CONFLICTS C3 (400 vs 422) và C6 (concurrency → 409).
/// </summary>
public static class ProblemDetailsMapper
{
    public const string InternalErrorDetail =
        "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại sau hoặc liên hệ quản trị viên kèm traceId.";

    public static ProblemDescriptor Map(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            ValidationFailedException validation => new ProblemDescriptor(
                StatusCodes.Status422UnprocessableEntity,
                validation.ErrorCode,
                TitleFor(validation.Kind),
                validation.Message,
                validation.Errors),

            AppException app => new ProblemDescriptor(
                StatusFor(app.Kind),
                app.ErrorCode,
                TitleFor(app.Kind),
                app.Message),

            // JSON/query/route sai cú pháp hoặc sai kiểu khi Minimal API bind tham số (C3 → 400).
            // Giữ nguyên status gốc nếu framework trả mã 4xx khác (ví dụ 413 body quá lớn).
            BadHttpRequestException badRequest => new ProblemDescriptor(
                badRequest.StatusCode,
                ErrorCodes.MalformedRequest,
                "Request sai cấu trúc",
                "Không đọc được dữ liệu gửi lên: JSON, query string hoặc tham số sai cú pháp/sai kiểu."),

            JsonException => new ProblemDescriptor(
                StatusCodes.Status400BadRequest,
                ErrorCodes.MalformedRequest,
                "Request sai cấu trúc",
                "JSON gửi lên không hợp lệ."),

            // Cột xmin (optimistic concurrency) không khớp — RESOLVED-CONFLICTS C6.
            // Hiện chỉ Recipe có luồng sửa đồng thời theo SRS nên dùng mã RECIPE_CONCURRENCY_CONFLICT;
            // module khác cần mã riêng thì bổ sung vào Phụ lục B và bắt exception ở handler.
            DbUpdateConcurrencyException => new ProblemDescriptor(
                StatusCodes.Status409Conflict,
                ErrorCodes.RecipeConcurrencyConflict,
                "Xung đột dữ liệu",
                "Dữ liệu đã bị thay đổi bởi người khác. Vui lòng tải lại và thử lại."),

            // Không trả message gốc ra ngoài (tránh lộ thông tin nội bộ) — chi tiết nằm trong log.
            _ => new ProblemDescriptor(
                StatusCodes.Status500InternalServerError,
                ErrorCodes.InternalServerError,
                "Lỗi hệ thống",
                InternalErrorDetail),
        };
    }

    public static int StatusFor(AppErrorKind kind) => kind switch
    {
        AppErrorKind.Validation => StatusCodes.Status422UnprocessableEntity,
        AppErrorKind.BusinessRule => StatusCodes.Status422UnprocessableEntity,
        AppErrorKind.NotFound => StatusCodes.Status404NotFound,
        AppErrorKind.Conflict => StatusCodes.Status409Conflict,
        AppErrorKind.Unauthorized => StatusCodes.Status401Unauthorized,
        AppErrorKind.Forbidden => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status500InternalServerError,
    };

    private static string TitleFor(AppErrorKind kind) => kind switch
    {
        AppErrorKind.Validation => "Dữ liệu không hợp lệ",
        AppErrorKind.BusinessRule => "Vi phạm quy tắc nghiệp vụ",
        AppErrorKind.NotFound => "Không tìm thấy dữ liệu",
        AppErrorKind.Conflict => "Xung đột dữ liệu",
        AppErrorKind.Unauthorized => "Xác thực không hợp lệ",
        AppErrorKind.Forbidden => "Không có quyền thực hiện",
        _ => "Lỗi hệ thống",
    };
}
