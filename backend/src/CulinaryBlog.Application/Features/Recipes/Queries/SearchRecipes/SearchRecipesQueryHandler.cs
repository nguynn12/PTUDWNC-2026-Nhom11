namespace CulinaryBlog.Application.Features.Recipes.Queries.SearchRecipes;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes.DTOs;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler xử lý SearchRecipesQuery: tìm kiếm toàn văn công thức bằng PostgreSQL Full-Text Search (SearchVector + unaccent + simple + pg_trgm fallback).
/// </summary>
public class SearchRecipesQueryHandler : IRequestHandler<SearchRecipesQuery, PagedResult<RecipeSummaryDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserQueryService _userQueryService;
    private readonly ILogger<SearchRecipesQueryHandler> _logger;

    public SearchRecipesQueryHandler(
        IApplicationDbContext dbContext,
        IUserQueryService userQueryService,
        ILogger<SearchRecipesQueryHandler> logger)
    {
        _dbContext = dbContext;
        _userQueryService = userQueryService;
        _logger = logger;
    }

    public async Task<PagedResult<RecipeSummaryDto>> Handle(SearchRecipesQuery request, CancellationToken cancellationToken)
    {
        // 1. Chuẩn hóa tham số tìm kiếm và phân trang
        var searchTerm = request.Q?.Trim() ?? string.Empty;
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize switch
        {
            < 1 => 12,
            > 50 => 50,
            _ => request.PageSize
        };

        _logger.LogInformation("Thực hiện tìm kiếm công thức: Q='{SearchTerm}', Page={Page}, PageSize={PageSize}",
            searchTerm, page, pageSize);

        // 2. Truy vấn Full-Text Search sử dụng PostgreSQL SearchVector (GIN Index) và fallback Trigram Similarity
        var baseQuery = _dbContext.Recipes
            .FromSqlInterpolated($@"
                SELECT * FROM ""Recipes""
                WHERE ""Status"" = {(short)RecipeStatus.Published}
                  AND ""IsDeleted"" = false
                  AND (
                      ""SearchVector"" @@ plainto_tsquery('simple', unaccent({searchTerm}))
                      OR similarity(unaccent(""Title""), unaccent({searchTerm})) > 0.3
                  )
                ORDER BY
                  ts_rank(""SearchVector"", plainto_tsquery('simple', unaccent({searchTerm}))) DESC,
                  ""PublishedAt"" DESC
            ")
            .AsNoTracking();

        // 3. Áp dụng các bộ lọc kết hợp đa tiêu chí (toán tử AND theo FR-SRCH-002)
        if (request.CategoryId.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.CategoryId == request.CategoryId.Value);
        }

        if (request.Difficulty.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.Difficulty == request.Difficulty.Value);
        }

        if (request.MaxPrepTime.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.PrepTimeMinutes <= request.MaxPrepTime.Value);
        }

        if (request.MaxCookTime.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.CookTimeMinutes <= request.MaxCookTime.Value);
        }

        if (request.MaxTotalTime.HasValue)
        {
            baseQuery = baseQuery.Where(r => (r.PrepTimeMinutes + r.CookTimeMinutes) <= request.MaxTotalTime.Value);
        }

        if (request.MinCalories.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.Nutrition != null && r.Nutrition.Calories != null && r.Nutrition.Calories >= request.MinCalories.Value);
        }

        if (request.MaxCalories.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.Nutrition != null && r.Nutrition.Calories != null && r.Nutrition.Calories <= request.MaxCalories.Value);
        }

        // 4. Đếm tổng số kết quả thỏa mãn điều kiện
        var total = await baseQuery.CountAsync(cancellationToken);

        if (total == 0)
        {
            _logger.LogInformation("Không tìm thấy công thức nào phù hợp với từ khóa '{SearchTerm}'.", searchTerm);
            return new PagedResult<RecipeSummaryDto>(Array.Empty<RecipeSummaryDto>(), 0, page, pageSize);
        }

        // 5. Áp dụng sắp xếp: nếu người dùng yêu cầu sắp xếp khác ngoài "relevance"
        var isRelevanceSort = string.IsNullOrWhiteSpace(request.SortBy) ||
                              string.Equals(request.SortBy.Trim(), "relevance", StringComparison.OrdinalIgnoreCase);

        var finalQuery = isRelevanceSort
            ? baseQuery
            : ApplySorting(baseQuery, request.SortBy, request.SortOrder);

        // 6. Phân trang và ánh xạ DTO
        var items = await finalQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RecipeSummaryDto
            {
                Id = r.Id,
                Title = r.Title,
                Slug = r.Slug,
                Description = r.Description,
                ThumbnailUrl = r.Images
                    .Where(i => !i.IsDeleted && i.IsPrimary)
                    .Select(i => i.ThumbnailUrl ?? i.MediumUrl ?? i.OriginalUrl)
                    .FirstOrDefault() ?? r.Images
                    .Where(i => !i.IsDeleted)
                    .OrderBy(i => i.OrderIndex)
                    .Select(i => i.ThumbnailUrl ?? i.MediumUrl ?? i.OriginalUrl)
                    .FirstOrDefault(),
                PrepTimeMinutes = r.PrepTimeMinutes,
                CookTimeMinutes = r.CookTimeMinutes,
                Servings = r.Servings,
                Difficulty = r.Difficulty.ToString(),
                Status = r.Status.ToString(),
                CategoryId = r.CategoryId,
                CategoryName = r.Category != null ? r.Category.Name : null,
                CategorySlug = r.Category != null ? r.Category.Slug : null,
                AuthorId = r.AuthorId,
                AuthorName = null,
                PublishedAt = r.PublishedAt,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        // Nạp thông tin tác giả từ IUserQueryService
        var authorIds = items.Select(i => i.AuthorId).Distinct();
        var authors = await _userQueryService.GetAuthorSummariesAsync(authorIds, cancellationToken);
        foreach (var item in items)
        {
            if (authors.TryGetValue(item.AuthorId, out var author))
            {
                item.AuthorName = author.DisplayName;
            }
        }

        _logger.LogInformation("Tìm kiếm thành công: {Count}/{Total} công thức cho trang {Page}", items.Count, total, page);

        return new PagedResult<RecipeSummaryDto>(items, total, page, pageSize);
    }

    /// <summary>
    /// Áp dụng sắp xếp tùy chọn khi người dùng không chọn relevance.
    /// </summary>
    private static IQueryable<Recipe> ApplySorting(IQueryable<Recipe> query, string? sortBy, string? sortOrder)
    {
        var normalizedSortBy = sortBy?.Trim().ToLowerInvariant();
        var isAsc = string.Equals(sortOrder?.Trim(), "asc", StringComparison.OrdinalIgnoreCase);

        return normalizedSortBy switch
        {
            "publishedat" => isAsc ? query.OrderBy(r => r.PublishedAt) : query.OrderByDescending(r => r.PublishedAt),
            "cooktime" => isAsc ? query.OrderBy(r => r.CookTimeMinutes) : query.OrderByDescending(r => r.CookTimeMinutes),
            "totaltime" => isAsc
                ? query.OrderBy(r => r.PrepTimeMinutes + r.CookTimeMinutes)
                : query.OrderByDescending(r => r.PrepTimeMinutes + r.CookTimeMinutes),
            "calories" => isAsc
                ? query.OrderBy(r => r.Nutrition != null ? r.Nutrition.Calories : 0)
                : query.OrderByDescending(r => r.Nutrition != null ? r.Nutrition.Calories : 0),
            "title" => isAsc ? query.OrderBy(r => r.Title) : query.OrderByDescending(r => r.Title),
            _ => query // Mặc định giữ nguyên thứ tự ts_rank từ câu lệnh SQL FTS
        };
    }
}
