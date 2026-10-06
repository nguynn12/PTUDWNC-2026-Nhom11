namespace CulinaryBlog.Application.Features.Recipes.Queries.GetRecipes;

using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes.DTOs;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler xử lý GetRecipesQuery: truy vấn danh sách Recipe đã xuất bản, phân trang và ánh xạ DTO.
/// </summary>
public class GetRecipesQueryHandler : IRequestHandler<GetRecipesQuery, PagedResult<RecipeSummaryDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<GetRecipesQueryHandler> _logger;

    public GetRecipesQueryHandler(
        IApplicationDbContext dbContext,
        ILogger<GetRecipesQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<PagedResult<RecipeSummaryDto>> Handle(GetRecipesQuery request, CancellationToken cancellationToken)
    {
        // 1. Chuẩn hóa tham số phân trang
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize switch
        {
            < 1 => 12,
            > 50 => 50,
            _ => request.PageSize
        };

        _logger.LogInformation("Truy vấn danh sách Recipe: Page={Page}, PageSize={PageSize}, CategoryId={CategoryId}",
            page, pageSize, request.CategoryId);

        // 2. Xây dựng truy vấn cơ sở: Chỉ lấy Recipe đã xuất bản (Published) và chưa bị soft delete
        var query = _dbContext.Recipes
            .AsNoTracking()
            .Where(r => !r.IsDeleted && r.Status == RecipeStatus.Published);

        // 3. Áp dụng các bộ lọc đa tiêu chí (kết hợp bằng toán tử AND theo FR-SRCH-002)
        if (request.CategoryId.HasValue)
        {
            query = query.Where(r => r.CategoryId == request.CategoryId.Value);
        }

        if (request.Difficulty.HasValue)
        {
            query = query.Where(r => r.Difficulty == request.Difficulty.Value);
        }

        if (request.MaxPrepTime.HasValue)
        {
            query = query.Where(r => r.PrepTimeMinutes <= request.MaxPrepTime.Value);
        }

        if (request.MaxCookTime.HasValue)
        {
            query = query.Where(r => r.CookTimeMinutes <= request.MaxCookTime.Value);
        }

        if (request.MaxTotalTime.HasValue)
        {
            query = query.Where(r => (r.PrepTimeMinutes + r.CookTimeMinutes) <= request.MaxTotalTime.Value);
        }

        if (request.MinCalories.HasValue)
        {
            query = query.Where(r => r.Nutrition != null && r.Nutrition.Calories != null && r.Nutrition.Calories >= request.MinCalories.Value);
        }

        if (request.MaxCalories.HasValue)
        {
            query = query.Where(r => r.Nutrition != null && r.Nutrition.Calories != null && r.Nutrition.Calories <= request.MaxCalories.Value);
        }

        // 4. Đếm tổng số bản ghi thỏa mãn điều kiện
        var total = await query.CountAsync(cancellationToken);

        if (total == 0)
        {
            return new PagedResult<RecipeSummaryDto>(Array.Empty<RecipeSummaryDto>(), 0, page, pageSize);
        }

        // 5. Áp dụng sắp xếp linh hoạt theo whitelist (FR-SRCH-003) và phân trang
        var sortedQuery = ApplySorting(query, request.SortBy, request.SortOrder);

        var items = await sortedQuery
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
                AuthorName = r.Author != null ? r.Author.DisplayName : null,
                PublishedAt = r.PublishedAt,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Tìm thấy {Count}/{Total} Recipes cho trang {Page}", items.Count, total, page);

        return new PagedResult<RecipeSummaryDto>(items, total, page, pageSize);
    }

    /// <summary>
    /// Áp dụng sắp xếp động theo danh sách whitelist quy định trong SRS FR-SRCH-003 và quyết định 2440.
    /// </summary>
    private static IQueryable<Recipe> ApplySorting(IQueryable<Recipe> query, string? sortBy, string? sortOrder)
    {
        var normalizedSortBy = sortBy?.Trim().ToLowerInvariant();
        var isAsc = string.Equals(sortOrder?.Trim(), "asc", StringComparison.OrdinalIgnoreCase);

        return normalizedSortBy switch
        {
            "totaltime" or "quickest" => isAsc
                ? query.OrderBy(r => r.PrepTimeMinutes + r.CookTimeMinutes)
                : query.OrderByDescending(r => r.PrepTimeMinutes + r.CookTimeMinutes),

            "preptime" => isAsc
                ? query.OrderBy(r => r.PrepTimeMinutes)
                : query.OrderByDescending(r => r.PrepTimeMinutes),

            "cooktime" => isAsc
                ? query.OrderBy(r => r.CookTimeMinutes)
                : query.OrderByDescending(r => r.CookTimeMinutes),

            "calories" => isAsc
                ? query.OrderBy(r => r.Nutrition!.Calories)
                : query.OrderByDescending(r => r.Nutrition!.Calories),

            "title" => isAsc
                ? query.OrderBy(r => r.Title)
                : query.OrderByDescending(r => r.Title),

            "servings" => isAsc
                ? query.OrderBy(r => r.Servings)
                : query.OrderByDescending(r => r.Servings),

            "createdat" => isAsc
                ? query.OrderBy(r => r.CreatedAt)
                : query.OrderByDescending(r => r.CreatedAt),

            // Mặc định hoặc "publishedat" / "newest"
            _ => isAsc
                ? query.OrderBy(r => r.PublishedAt ?? r.CreatedAt)
                : query.OrderByDescending(r => r.PublishedAt ?? r.CreatedAt)
        };
    }
}
