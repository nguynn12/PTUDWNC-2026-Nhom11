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
/// Handler xử lý GetCategoriesQuery, thực hiện truy vấn tối ưu từ cơ sở dữ liệu.
/// </summary>
public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<GetCategoriesQueryHandler> _logger;

    public GetCategoriesQueryHandler(
        IApplicationDbContext dbContext,
        ILogger<GetCategoriesQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Bắt đầu truy vấn danh sách danh mục món ăn.");

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

        _logger.LogInformation("Truy vấn thành công {Count} danh mục món ăn.", categories.Count);

        return categories;
    }
}
