using System.Text.Json;
using CulinaryBlog.Infrastructure.Services.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;
using Xunit;

namespace CulinaryBlog.UnitTests.Services;

public class RedisCacheServiceTests
{
    private class FakeDistributedCache : IDistributedCache
    {
        private readonly Dictionary<string, byte[]> _store = new();
        public bool SimulateFailure { get; set; }

        public byte[]? Get(string key) => GetAsync(key).GetAwaiter().GetResult();

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            if (SimulateFailure)
            {
                throw new RedisException("Simulated Redis Down");
            }

            return Task.FromResult(_store.TryGetValue(key, out var val) ? val : null);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) =>
            SetAsync(key, value, options).GetAwaiter().GetResult();

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            if (SimulateFailure)
            {
                throw new RedisException("Simulated Redis Down");
            }

            _store[key] = value;
            return Task.CompletedTask;
        }

        public void Refresh(string key) { }
        public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;

        public void Remove(string key) => RemoveAsync(key).GetAwaiter().GetResult();

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            if (SimulateFailure)
            {
                throw new RedisException("Simulated Redis Down");
            }

            _store.Remove(key);
            return Task.CompletedTask;
        }
    }

    private class SampleData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public async Task SetAsync_And_GetAsync_Returns_Cached_Item()
    {
        var fakeCache = new FakeDistributedCache();
        var cacheService = new RedisCacheService(fakeCache, NullLogger<RedisCacheService>.Instance);
        var testObj = new SampleData { Id = 10, Name = "Bánh mì Việt Nam" };

        await cacheService.SetAsync("recipe:10", testObj, TimeSpan.FromMinutes(5));
        var retrieved = await cacheService.GetAsync<SampleData>("recipe:10");

        Assert.NotNull(retrieved);
        Assert.Equal(10, retrieved.Id);
        Assert.Equal("Bánh mì Việt Nam", retrieved.Name);
    }

    [Fact]
    public async Task RemoveAsync_Deletes_Cached_Item()
    {
        var fakeCache = new FakeDistributedCache();
        var cacheService = new RedisCacheService(fakeCache, NullLogger<RedisCacheService>.Instance);
        var testObj = new SampleData { Id = 20, Name = "Phở bò" };

        await cacheService.SetAsync("recipe:20", testObj);
        await cacheService.RemoveAsync("recipe:20");
        var retrieved = await cacheService.GetAsync<SampleData>("recipe:20");

        Assert.Null(retrieved);
    }

    [Fact]
    public async Task GetOrCreateAsync_Uses_Factory_When_Cache_Empty()
    {
        var fakeCache = new FakeDistributedCache();
        var cacheService = new RedisCacheService(fakeCache, NullLogger<RedisCacheService>.Instance);

        var factoryCalled = false;
        var result = await cacheService.GetOrCreateAsync("category:soup", async () =>
        {
            factoryCalled = true;
            await Task.Yield();
            return new SampleData { Id = 1, Name = "Món Súp" };
        }, TimeSpan.FromMinutes(30));

        Assert.True(factoryCalled);
        Assert.NotNull(result);
        Assert.Equal("Món Súp", result.Name);

        // Lần thứ 2 gọi lại: lấy từ cache, factory không được gọi tiếp
        factoryCalled = false;
        var secondResult = await cacheService.GetOrCreateAsync("category:soup", async () =>
        {
            factoryCalled = true;
            await Task.Yield();
            return new SampleData { Id = 1, Name = "Món Khác" };
        });

        Assert.False(factoryCalled);
        Assert.NotNull(secondResult);
        Assert.Equal("Món Súp", secondResult.Name);
    }

    [Fact]
    public async Task Resilience_GetAsync_Falls_Back_Safely_When_Redis_Down()
    {
        // NFR-REL-002: Redis down -> không ném exception, trả về default/null
        var fakeCache = new FakeDistributedCache { SimulateFailure = true };
        var cacheService = new RedisCacheService(fakeCache, NullLogger<RedisCacheService>.Instance);

        var result = await cacheService.GetAsync<SampleData>("recipe:down");

        Assert.Null(result); // Không quăng lỗi, fallback an toàn
    }

    [Fact]
    public async Task Resilience_SetAsync_Does_Not_Throw_When_Redis_Down()
    {
        // NFR-REL-002: Redis down -> SetAsync không làm sập luồng ghi
        var fakeCache = new FakeDistributedCache { SimulateFailure = true };
        var cacheService = new RedisCacheService(fakeCache, NullLogger<RedisCacheService>.Instance);

        var testObj = new SampleData { Id = 99, Name = "Cơm tấm" };
        var exception = await Record.ExceptionAsync(() => cacheService.SetAsync("recipe:99", testObj));

        Assert.Null(exception);
    }

    [Fact]
    public async Task Resilience_GetOrCreateAsync_Falls_Back_To_DB_Factory_When_Redis_Down()
    {
        // NFR-REL-002: Redis down -> GetOrCreateAsync vẫn gọi factory thành công trả dữ liệu DB
        var fakeCache = new FakeDistributedCache { SimulateFailure = true };
        var cacheService = new RedisCacheService(fakeCache, NullLogger<RedisCacheService>.Instance);

        var factoryCalled = false;
        var result = await cacheService.GetOrCreateAsync("recipe:down", async () =>
        {
            factoryCalled = true;
            await Task.Yield();
            return new SampleData { Id = 50, Name = "Bún bò Huế từ Database" };
        });

        Assert.True(factoryCalled);
        Assert.NotNull(result);
        Assert.Equal("Bún bò Huế từ Database", result.Name);
    }
}
