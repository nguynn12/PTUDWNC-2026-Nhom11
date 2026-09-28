namespace CulinaryBlog.Application.Features.Recipes.Queries.GetRecipes;

using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes.DTOs;
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

        // 3. Áp dụng bộ lọc theo danh mục (nếu có)
        if (request.CategoryId.HasValue)
        {
            query = query.Where(r => r.CategoryId == request.CategoryId.Value);
        }

        // 4. Đếm tổng số bản ghi thỏa mãn điều kiện
        var total = await query.CountAsync(cancellationToken);

        if (total == 0)
        {
            return new PagedResult<RecipeSummaryDto>(Array.Empty<RecipeSummaryDto>(), 0, page, pageSize);
        }

        // 5. Sắp xếp mặc định: Bài mới nhất lên đầu (PublishedAt ?? CreatedAt DESC) và phân trang
        var items = await query
            .OrderByDescending(r => r.PublishedAt ?? r.CreatedAt)
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
}
