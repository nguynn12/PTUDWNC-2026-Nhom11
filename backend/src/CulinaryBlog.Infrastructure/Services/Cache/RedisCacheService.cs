using System.Text.Json;
using System.Text.Json.Serialization;
using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace CulinaryBlog.Infrastructure.Services.Cache;

/// <summary>
/// Dịch vụ Redis Cache đóng gói IDistributedCache và IConnectionMultiplexer.
/// Tuân thủ nghiêm ngặt NFR-REL-002: Nếu Redis sập hoặc mất kết nối,
/// hệ thống tự động fallback về DB, ghi cảnh báo Warning và KHÔNG ném ngoại lệ làm chết API.
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IConnectionMultiplexer? _connectionMultiplexer;
    private readonly ILogger<RedisCacheService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public RedisCacheService(
        IDistributedCache distributedCache,
        ILogger<RedisCacheService> logger,
        IConnectionMultiplexer? connectionMultiplexer = null)
    {
        _distributedCache = distributedCache;
        _logger = logger;
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var bytes = await _distributedCache.GetAsync(key, cancellationToken);
            if (bytes == null || bytes.Length == 0)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(bytes, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[NFR-REL-002] Redis Cache GetAsync thất bại cho khóa {Key}. Chuyển hướng truy vấn trực tiếp DB.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        if (value == null) return;

        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
            var options = new DistributedCacheEntryOptions();

            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration.Value;
            }

            await _distributedCache.SetAsync(key, bytes, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[NFR-REL-002] Redis Cache SetAsync thất bại cho khóa {Key}. Bỏ qua nạp cache.", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[NFR-REL-002] Redis Cache RemoveAsync thất bại cho khóa {Key}.", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        if (_connectionMultiplexer == null || !_connectionMultiplexer.IsConnected)
        {
            _logger.LogWarning("[NFR-REL-002] Redis Multiplexer không khả dụng khi gọi RemoveByPrefixAsync({Prefix}). Bỏ qua thao tác xóa nhóm.", prefix);
            return;
        }

        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            var endpoints = _connectionMultiplexer.GetEndPoints();

            foreach (var endpoint in endpoints)
            {
                var server = _connectionMultiplexer.GetServer(endpoint);
                if (!server.IsConnected) continue;

                // Pattern tìm kiếm các khóa có chứa prefix
                var pattern = $"*{prefix}*";
                var keys = server.Keys(pattern: pattern).ToArray();

                if (keys.Length > 0)
                {
                    await db.KeyDeleteAsync(keys);
                    _logger.LogInformation("Đã hủy {Count} khóa cache khớp với mẫu '{Pattern}'", keys.Length, pattern);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[NFR-REL-002] Lỗi khi xóa cache theo prefix {Prefix}.", prefix);
        }
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        var result = await factory();
        if (result != null)
        {
            await SetAsync(key, result, expiration, cancellationToken);
        }

        return result;
    }
}

