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
/// Handler xử lý GetCategoryBySlugQuery, truy vấn chi tiết từ cơ sở dữ liệu.
/// </summary>
public class GetCategoryBySlugQueryHandler : IRequestHandler<GetCategoryBySlugQuery, CategoryDetailDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<GetCategoryBySlugQueryHandler> _logger;

    public GetCategoryBySlugQueryHandler(
        IApplicationDbContext dbContext,
        ILogger<GetCategoryBySlugQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<CategoryDetailDto?> Handle(GetCategoryBySlugQuery request, CancellationToken cancellationToken)
    {
        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();
        _logger.LogInformation("Truy vấn thông tin danh mục với slug '{Slug}'.", normalizedSlug);

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

        return category;
    }
}
