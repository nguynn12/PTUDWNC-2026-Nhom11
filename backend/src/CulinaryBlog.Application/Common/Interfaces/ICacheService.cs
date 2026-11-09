namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>
/// Dịch vụ bộ đệm phân tán Redis (NFR-PERF, NFR-REL-002).
/// Đóng gói IDistributedCache và StackExchange.Redis với cơ chế resilience (fallback DB an toàn).
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Lấy dữ liệu từ cache theo khóa.
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lưu trữ dữ liệu vào cache với thời gian hết hạn cụ thể.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa một khóa khỏi cache.
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa tất cả các khóa có tiền tố trùng khớp (phục vụ invalidate theo nhóm/tag).
    /// </summary>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy dữ liệu từ cache nếu có; nếu chưa có hoặc Redis lỗi, gọi hàm factory để truy vấn DB và nạp vào cache.
    /// </summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
}

