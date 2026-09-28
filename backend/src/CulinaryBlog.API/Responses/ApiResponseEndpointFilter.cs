using CulinaryBlog.Application.Common.Models;

namespace CulinaryBlog.API.Responses;

/// <summary>
/// Endpoint filter tự bọc kết quả thành envelope <c>{data, meta}</c> (RESOLVED-CONFLICTS C2 —
/// IMPACT: "tạo ApiResponse/PagedApiResponse qua Minimal API IEndpointFilter").
///
/// Quy ước dùng cho endpoint đã bật filter (xem <see cref="ApiResponseExtensions.WithApiResponseEnvelope{TBuilder}"/>):
/// - Trả thẳng DTO → <c>{"data": dto}</c> (200).
/// - Trả <see cref="PagedResult{T}"/> → <c>{"data": [...], "meta": {...}}</c> (200).
/// - Trả <see cref="IResult"/> (ví dụ <c>ApiResults.Created</c>, <c>TypedResults.NoContent()</c>) → giữ nguyên.
/// - Lỗi → throw <c>AppException</c>, GlobalExceptionHandler lo phần Problem Details.
/// </summary>
public sealed class ApiResponseEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(next);

        var result = await next(context);
        return Wrap(result);
    }

    public static object? Wrap(object? result) => result switch
    {
        null => null,
        IResult => result,
        IPagedResult paged => new PagedApiResponse<object?>(
            paged.Items.Cast<object?>().ToList(),
            PaginationMeta.From(paged)),
        object value => new ApiResponse<object>(value),
    };
}
