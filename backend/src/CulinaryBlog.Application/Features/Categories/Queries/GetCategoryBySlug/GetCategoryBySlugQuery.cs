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
public record GetCategoryBySlugQuery(string Slug, int Page = 1, int PageSize = 12) : IRequest<CategoryDetailDto?>;

/// <summary>
/// Handler xử lý GetCategoryBySlugQuery, truy vấn chi tiết từ cơ sở dữ liệu kết hợp Redis Cache 30 phút (NFR-PERF).
/// </summary>
public class GetCategoryBySlugQueryHandler : IRequestHandler<GetCategoryBySlugQuery, CategoryDetailDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<GetCategoryBySlugQueryHandler> _logger;
    private readonly ICacheService? _cacheService;
    private readonly ICurrentUserService? _currentUserService;
    private readonly IUserQueryService? _userQueryService;

    public GetCategoryBySlugQueryHandler(
        IApplicationDbContext dbContext,
        ILogger<GetCategoryBySlugQueryHandler> logger,
        ICacheService? cacheService = null,
        ICurrentUserService? currentUserService = null,
        IUserQueryService? userQueryService = null)
    {
        _dbContext = dbContext;
        _logger = logger;
        _cacheService = cacheService;
        _currentUserService = currentUserService;
        _userQueryService = userQueryService;
    }

    public async Task<CategoryDetailDto?> Handle(GetCategoryBySlugQuery request, CancellationToken cancellationToken)
    {
        if (request.Page < 1 || request.PageSize < 1 || request.PageSize > 50)
            throw new CulinaryBlog.Application.Common.Exceptions.BusinessRuleValidationException("Trang phải từ 1, số mục mỗi trang từ 1 đến 50.", "VALIDATION_ERROR");
        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();
        var cacheKey = $"{CacheConstants.CategoryPrefix}{normalizedSlug}";

        if (_cacheService != null)
        {
            var cached = await _cacheService.GetAsync<CategoryDetailDto>(cacheKey, cancellationToken);
            if (cached != null)
            {
                return await LoadRecipesAsync(cached, request, cancellationToken);
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

        return await LoadRecipesAsync(category, request, cancellationToken);
    }
    private async Task<CategoryDetailDto> LoadRecipesAsync(CategoryDetailDto category, GetCategoryBySlugQuery request, CancellationToken ct)
    {
        var userId = _currentUserService?.UserId;
        var query = _dbContext.Recipes.AsNoTracking().Where(r => !r.IsDeleted && r.CategoryId == category.Id &&
            (r.Status == RecipeStatus.Published || (userId != null && r.AuthorId == userId && r.Status == RecipeStatus.Draft)));
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(r => r.PublishedAt).ThenBy(r => r.Id)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(r => new CulinaryBlog.Application.Features.Recipes.DTOs.RecipeSummaryDto
            {
                Id = r.Id, Title = r.Title, Slug = r.Slug, Description = r.Description,
                PrepTimeMinutes = r.PrepTimeMinutes, CookTimeMinutes = r.CookTimeMinutes,
                Servings = r.Servings, Difficulty = r.Difficulty, Status = r.Status,
                CategoryId = r.CategoryId, CategoryName = category.Name, CategorySlug = category.Slug,
                AuthorId = r.AuthorId, PublishedAt = r.PublishedAt, CreatedAt = r.CreatedAt,
                ThumbnailUrl = r.Images.Where(i => !i.IsDeleted).OrderByDescending(i => i.IsPrimary).ThenBy(i => i.OrderIndex)
                    .Select(i => i.ThumbnailUrl ?? i.MediumUrl ?? i.OriginalUrl).FirstOrDefault()
            }).ToListAsync(ct);
        if (_userQueryService != null)
        {
            var authors = await _userQueryService.GetAuthorSummariesAsync(items.Select(i => i.AuthorId).Distinct(), ct);
            foreach (var item in items)
                if (authors.TryGetValue(item.AuthorId, out var author)) item.AuthorName = author.DisplayName;
        }
        return new CategoryDetailDto
        {
            Id = category.Id, Name = category.Name, Slug = category.Slug, Description = category.Description,
            ImageUrl = category.ImageUrl, OrderIndex = category.OrderIndex, CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt, RecipeCount = category.RecipeCount,
            Recipes = new(items, total, request.Page, request.PageSize)
        };
    }}
