using CulinaryBlog.Application.Common.Constants;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Infrastructure.Services.Cache;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CulinaryBlog.UnitTests.Services;

public class CacheInvalidatorTests
{
    private class MockCacheService : ICacheService
    {
        public List<string> RemovedKeys { get; } = new();
        public List<string> RemovedPrefixes { get; } = new();

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) => Task.FromResult<T?>(default);
        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) => Task.CompletedTask;
        
        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            RemovedKeys.Add(key);
            return Task.CompletedTask;
        }

        public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            RemovedPrefixes.Add(prefix);
            return Task.CompletedTask;
        }

        public Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) => factory();
    }

    [Fact]
    public async Task InvalidateRecipeAsync_Removes_Expected_Keys_And_Prefixes()
    {
        var mockCache = new MockCacheService();
        var invalidator = new CacheInvalidator(mockCache, NullLogger<CacheInvalidator>.Instance);
        var recipeId = Guid.NewGuid();
        var slug = "pho-bo-ha-noi";

        await invalidator.InvalidateRecipeAsync(recipeId, slug);

        Assert.Contains($"{CacheConstants.RecipeDetailPrefix}{recipeId}", mockCache.RemovedKeys);
        Assert.Contains($"{CacheConstants.RecipeSlugPrefix}{slug}", mockCache.RemovedKeys);
        Assert.Contains(CacheConstants.RecipeListPrefix, mockCache.RemovedPrefixes);
        Assert.Contains(CacheConstants.SearchPrefix, mockCache.RemovedPrefixes);
    }

    [Fact]
    public async Task InvalidateCategoriesAsync_Removes_Expected_Keys_And_Prefixes()
    {
        var mockCache = new MockCacheService();
        var invalidator = new CacheInvalidator(mockCache, NullLogger<CacheInvalidator>.Instance);
        var slug = "mon-chay";

        await invalidator.InvalidateCategoriesAsync(slug);

        Assert.Contains(CacheConstants.CategoryAllKey, mockCache.RemovedKeys);
        Assert.Contains($"{CacheConstants.CategoryPrefix}{slug}", mockCache.RemovedKeys);
        Assert.Contains(CacheConstants.CategoryPrefix, mockCache.RemovedPrefixes);
    }

    [Fact]
    public async Task InvalidateSearchAsync_Removes_Search_Prefix()
    {
        var mockCache = new MockCacheService();
        var invalidator = new CacheInvalidator(mockCache, NullLogger<CacheInvalidator>.Instance);

        await invalidator.InvalidateSearchAsync();

        Assert.Contains(CacheConstants.SearchPrefix, mockCache.RemovedPrefixes);
    }
}

