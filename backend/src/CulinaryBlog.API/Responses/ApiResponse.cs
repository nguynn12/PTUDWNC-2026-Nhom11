using CulinaryBlog.Application.Common.Models;

namespace CulinaryBlog.API.Responses;

/// <summary>
/// Envelope cho endpoint chi tiết/mutation: <c>{"data": T}</c> (RESOLVED-CONFLICTS C2).
/// </summary>
public sealed record ApiResponse<T>(T Data);

/// <summary>
/// Envelope cho endpoint danh sách: <c>{"data": T[], "meta": {...}}</c> (RESOLVED-CONFLICTS C2).
/// </summary>
public sealed record PagedApiResponse<T>(IReadOnlyList<T> Data, PaginationMeta Meta);

/// <summary>Thông tin phân trang trong <c>meta</c> — tên field đúng theo C2.</summary>
public sealed record PaginationMeta(
    int Page,
    int PageSize,
    int Total,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage)
{
    public static PaginationMeta From(IPagedResult paged)
    {
        ArgumentNullException.ThrowIfNull(paged);

        return new PaginationMeta(
            paged.Page,
            paged.PageSize,
            paged.TotalCount,
            paged.TotalPages,
            paged.HasNextPage,
            paged.HasPreviousPage);
    }
}
