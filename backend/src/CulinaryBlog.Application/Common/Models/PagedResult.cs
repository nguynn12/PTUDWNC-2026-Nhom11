namespace CulinaryBlog.Application.Common.Models;

/// <summary>
/// Mô hình đóng gói kết quả phân trang dùng nội bộ trong tầng Application theo SRS Chương 5.2/8.
/// Presentation layer sẽ map sang response envelope: { "data": [...], "meta": { ... } }.
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu của phần tử trong danh sách.</typeparam>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int Total { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)Total / PageSize) : 0;

    public PagedResult() { }

    public PagedResult(IReadOnlyList<T> items, int total, int page, int pageSize)
    {
        Items = items;
        Total = total;
        Page = page;
        PageSize = pageSize;
    }
}
