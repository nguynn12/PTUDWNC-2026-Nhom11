using System.Collections;

namespace CulinaryBlog.Application.Common.Models;

/// <summary>
/// Giao diện không generic để tầng API nhận biết kết quả phân trang mà không cần biết kiểu phần tử.
/// </summary>
public interface IPagedResult
{
    IEnumerable Items { get; }

    int Page { get; }

    int PageSize { get; }

    int TotalCount { get; }

    int TotalPages { get; }

    bool HasNextPage { get; }

    bool HasPreviousPage { get; }
}

/// <summary>
/// Kết quả phân trang dùng NỘI BỘ tầng Application (Query handler trả về kiểu này).
/// KHÔNG serialize thẳng ra HTTP — tầng API map sang envelope <c>{data, meta}</c>
/// (RESOLVED-CONFLICTS C2).
/// </summary>
public sealed class PagedResult<T> : IPagedResult
{
    public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);

        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public IReadOnlyList<T> Items { get; }

    public int Page { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasNextPage => Page < TotalPages;

    public bool HasPreviousPage => Page > 1;

    IEnumerable IPagedResult.Items => Items;
}
