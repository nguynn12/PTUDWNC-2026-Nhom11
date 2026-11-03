using CulinaryBlog.Application.Common.Constants;
using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Infrastructure.Services.Cache;

/// <summary>
/// Triển khai nghiệp vụ hủy hiệu lực bộ đệm theo tag và phân hệ (NFR-PERF).
/// </summary>
public class CacheInvalidator : ICacheInvalidator
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheInvalidator> _logger;

    public CacheInvalidator(ICacheService cacheService, ILogger<CacheInvalidator> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task InvalidateRecipeAsync(Guid recipeId, string? slug = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Bắt đầu invalidate cache cho RecipeId={RecipeId}, Slug={Slug}", recipeId, slug);

        // 1. Xóa chi tiết recipe theo ID
        await _cacheService.RemoveAsync($"{CacheConstants.RecipeDetailPrefix}{recipeId}", cancellationToken);

        // 2. Xóa chi tiết recipe theo slug (nếu có)
        if (!string.IsNullOrWhiteSpace(slug))
        {
            await _cacheService.RemoveAsync($"{CacheConstants.RecipeSlugPrefix}{slug.ToLowerInvariant()}", cancellationToken);
        }

        // 3. Xóa danh sách công thức công khai (vì số lượng hoặc dữ liệu trang chủ/danh sách đã đổi)
        await _cacheService.RemoveByPrefixAsync(CacheConstants.RecipeListPrefix, cancellationToken);

        // 4. Xóa cache kết quả tìm kiếm (để tránh trả về dữ liệu cũ)
        await _cacheService.RemoveByPrefixAsync(CacheConstants.SearchPrefix, cancellationToken);

        _logger.LogInformation("Hoàn tất invalidate cache cho RecipeId={RecipeId}", recipeId);
    }

    public async Task InvalidateCategoriesAsync(string? slug = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Bắt đầu invalidate cache cho danh mục, Slug={Slug}", slug);

        await _cacheService.RemoveAsync(CacheConstants.CategoryAllKey, cancellationToken);

        if (!string.IsNullOrWhiteSpace(slug))
        {
            await _cacheService.RemoveAsync($"{CacheConstants.CategoryPrefix}{slug.ToLowerInvariant()}", cancellationToken);
        }

        await _cacheService.RemoveByPrefixAsync(CacheConstants.CategoryPrefix, cancellationToken);
        _logger.LogInformation("Hoàn tất invalidate cache cho danh mục");
    }

    public async Task InvalidateSearchAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Bắt đầu invalidate toàn bộ cache tìm kiếm");
        await _cacheService.RemoveByPrefixAsync(CacheConstants.SearchPrefix, cancellationToken);
        _logger.LogInformation("Hoàn tất invalidate cache tìm kiếm");
    }

    public async Task InvalidateAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Bắt đầu dọn sạch toàn bộ cache công khai");
        await InvalidateCategoriesAsync(cancellationToken: cancellationToken);
        await _cacheService.RemoveByPrefixAsync(CacheConstants.RecipePrefix, cancellationToken);
        await InvalidateSearchAsync(cancellationToken: cancellationToken);
        _logger.LogInformation("Hoàn tất dọn sạch toàn bộ cache công khai");
    }
}

