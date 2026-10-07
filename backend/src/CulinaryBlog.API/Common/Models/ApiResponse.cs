using System.Text.Json.Serialization;

namespace CulinaryBlog.API.Common.Models;

/// <summary>
/// Chuẩn hóa Envelope phản hồi thành công theo SRS Chương 5.2/8: { "data": ..., "meta": ... }
/// Trường 'meta' chỉ xuất hiện khi có thông tin bổ sung (ví dụ phân trang).
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu của payload trả về.</typeparam>
public record ApiResponse<T>
{
    [JsonPropertyName("data")]
    public T Data { get; init; }

    [JsonPropertyName("meta")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Meta { get; init; }

    public ApiResponse(T data, object? meta = null)
    {
        Data = data;
        Meta = meta;
    }
}
