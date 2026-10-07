namespace CulinaryBlog.Application.Common.Models;

/// <summary>
/// Chuẩn hóa vỏ bọc dữ liệu phản hồi (Response Envelope) theo quyết định kiến trúc RESOLVED-CONFLICTS.md (C2).
/// Định dạng: { "data": T, "meta": object? }
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu của payload.</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Dữ liệu nghiệp vụ chính trả về cho Client.
    /// </summary>
    public T Data { get; set; } = default!;

    /// <summary>
    /// Metadata đi kèm (phân trang, thông tin bổ sung, null cho detail/mutation endpoint).
    /// </summary>
    public object? Meta { get; set; }

    public ApiResponse() { }

    public ApiResponse(T data, object? meta = null)
    {
        Data = data;
        Meta = meta;
    }

    /// <summary>
    /// Tạo phản hồi thành công chuẩn bọc { data, meta }.
    /// </summary>
    public static ApiResponse<T> Success(T data, object? meta = null) => new(data, meta);
}
