using CulinaryBlog.Application.Common.Constants;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Categories.DTOs;
using CulinaryBlog.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Features.Categories.Queries.GetCategories;

/// <summary>
/// Truy vấn lấy danh sách tất cả các danh mục món ăn đang hoạt động (FR-CAT-001).
/// </summary>
public record GetCategoriesQuery : IRequest<List<CategoryDto>>;

/// <summary>
/// Handler xử lý GetCategoriesQuery, thực hiện truy vấn tối ưu từ cơ sở dữ liệu kết hợp Redis Cache 30 phút (NFR-PERF).
/// </summary>
public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<GetCategoriesQueryHandler> _logger;
    private readonly ICacheService? _cacheService;

    public GetCategoriesQueryHandler(
        IApplicationDbContext dbContext,
        ILogger<GetCategoriesQueryHandler> logger,
        ICacheService? cacheService = null)
    {
        _dbContext = dbContext;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        if (_cacheService != null)
        {
            return await _cacheService.GetOrCreateAsync(
                CacheConstants.CategoryAllKey,
                () => QueryCategoriesFromDbAsync(cancellationToken),
                CacheConstants.CategoryDuration,
                cancellationToken);
        }

        return await QueryCategoriesFromDbAsync(cancellationToken);
    }

    private async Task<List<CategoryDto>> QueryCategoriesFromDbAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Truy vấn cơ sở dữ liệu: Danh sách danh mục món ăn (AsNoTracking).");

        var categories = await _dbContext.Categories
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.OrderIndex)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                OrderIndex = c.OrderIndex,
                CreatedAt = c.CreatedAt,
                RecipeCount = _dbContext.Recipes
                    .Count(r => r.CategoryId == c.Id && r.Status == RecipeStatus.Published && !r.IsDeleted)
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Truy vấn DB thành công: {Count} danh mục.", categories.Count);

        return categories;
    }
}
