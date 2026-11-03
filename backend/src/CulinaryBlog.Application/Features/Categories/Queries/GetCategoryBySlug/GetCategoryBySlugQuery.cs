using CulinaryBlog.Application.Common.Constants;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Categories.DTOs;
using CulinaryBlog.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Features.Categories.Queries.GetCategoryBySlug;

/// <summary>
/// Truy vấn lấy thông tin chi tiết của một danh mục theo định danh URL (Slug) (FR-CAT-002).
/// </summary>
/// <param name="Slug">Chuỗi định danh URL-friendly duy nhất của danh mục.</param>
public record GetCategoryBySlugQuery(string Slug) : IRequest<CategoryDetailDto?>;

/// <summary>
/// Handler xử lý GetCategoryBySlugQuery, truy vấn chi tiết từ cơ sở dữ liệu kết hợp Redis Cache 30 phút (NFR-PERF).
/// </summary>
public class GetCategoryBySlugQueryHandler : IRequestHandler<GetCategoryBySlugQuery, CategoryDetailDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<GetCategoryBySlugQueryHandler> _logger;
    private readonly ICacheService? _cacheService;

    public GetCategoryBySlugQueryHandler(
        IApplicationDbContext dbContext,
        ILogger<GetCategoryBySlugQueryHandler> logger,
        ICacheService? cacheService = null)
    {
        _dbContext = dbContext;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<CategoryDetailDto?> Handle(GetCategoryBySlugQuery request, CancellationToken cancellationToken)
    {
        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();
        var cacheKey = $"{CacheConstants.CategoryPrefix}{normalizedSlug}";

        if (_cacheService != null)
        {
            var cached = await _cacheService.GetAsync<CategoryDetailDto>(cacheKey, cancellationToken);
            if (cached != null)
            {
                return cached;
            }
        }

        _logger.LogInformation("Truy vấn DB: Thông tin danh mục với slug '{Slug}'.", normalizedSlug);

        var category = await _dbContext.Categories
            .AsNoTracking()
            .Where(c => c.Slug == normalizedSlug && !c.IsDeleted)
            .Select(c => new CategoryDetailDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                OrderIndex = c.OrderIndex,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                RecipeCount = _dbContext.Recipes
                    .Count(r => r.CategoryId == c.Id && r.Status == RecipeStatus.Published && !r.IsDeleted)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (category is null)
        {
            _logger.LogWarning("Không tìm thấy danh mục với slug '{Slug}'.", normalizedSlug);
            return null;
        }

        if (_cacheService != null)
        {
            await _cacheService.SetAsync(cacheKey, category, CacheConstants.CategoryDuration, cancellationToken);
        }

        return category;
    }
}
