namespace CulinaryBlog.Application.Common.Models;

/// <summary>
/// Mô hình đóng gói kết quả phân trang dùng chung cho các API Query trong tầng Application.
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu của phần tử trong danh sách.</typeparam>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];

    private int _page = 1;
    public int Page
    {
        get => _page;
        init => _page = value;
    }

    public int PageNumber
    {
        get => _page;
        init => _page = value;
    }

    public int PageSize { get; init; } = 10;

    private int _total = 0;
    public int Total
    {
        get => _total;
        init => _total = value;
    }

    public int TotalCount
    {
        get => _total;
        init => _total = value;
    }

    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)Total / PageSize) : 0;
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    public PagedResult() { }

    public PagedResult(IReadOnlyList<T> items, int total, int page, int pageSize)
    {
        Items = items;
        Total = total;
        Page = page;
        PageSize = pageSize;
    }
}
