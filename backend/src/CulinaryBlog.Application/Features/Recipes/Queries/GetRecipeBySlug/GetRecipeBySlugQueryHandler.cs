namespace CulinaryBlog.Application.Features.Recipes.Queries.GetRecipeBySlug;

using CulinaryBlog.Application.Common.Constants;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Categories.DTOs;
using CulinaryBlog.Application.Features.Recipes.DTOs;
using CulinaryBlog.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler xử lý GetRecipeBySlugQuery: eager load các thực thể con, ánh xạ RecipeDetailDto,
/// kết hợp Redis Cache 5 phút (NFR-PERF) và hỗ trợ phát hiện slug cũ để chuyển hướng 301 Permanent Redirect (RESOLVED-CONFLICTS C7, FR-RCP-002).
/// </summary>
public class GetRecipeBySlugQueryHandler : IRequestHandler<GetRecipeBySlugQuery, GetRecipeBySlugResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserQueryService _userQueryService;
    private readonly ILogger<GetRecipeBySlugQueryHandler> _logger;
    private readonly ICacheService? _cacheService;

    public GetRecipeBySlugQueryHandler(
        IApplicationDbContext dbContext,
        IUserQueryService userQueryService,
        ILogger<GetRecipeBySlugQueryHandler> logger,
        ICacheService? cacheService = null)
    {
        _dbContext = dbContext;
        _userQueryService = userQueryService;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<GetRecipeBySlugResult> Handle(GetRecipeBySlugQuery request, CancellationToken cancellationToken)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        var slugCacheKey = $"{CacheConstants.RecipeSlugPrefix}{slug}";

        if (_cacheService != null)
        {
            var cached = await _cacheService.GetAsync<RecipeDetailDto>(slugCacheKey, cancellationToken);
            if (cached != null)
            {
                return GetRecipeBySlugResult.Found(cached);
            }
        }

        _logger.LogInformation("Truy vấn DB: Chi tiết Recipe theo Slug: '{Slug}'", slug);

        // Eager load: Category, Ingredients, Steps, Images (và Nutrition là owned entity)
        var recipe = await _dbContext.Recipes
            .AsNoTracking()
            .Include(r => r.Category)
            .Include(r => r.Ingredients.Where(i => !i.IsDeleted))
            .Include(r => r.Steps.Where(s => !s.IsDeleted))
            .Include(r => r.Images.Where(img => !img.IsDeleted))
            .FirstOrDefaultAsync(r => r.Slug == slug && !r.IsDeleted && r.Status == RecipeStatus.Published, cancellationToken);

        if (recipe is null)
        {
            // Kiểm tra trong lịch sử slug cũ (RecipeSlugHistory) xem có chuyển hướng 301 được không
            var slugHistory = await _dbContext.RecipeSlugHistories
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.OldSlug == slug, cancellationToken);

            if (slugHistory != null)
            {
                var targetRecipe = await _dbContext.Recipes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == slugHistory.RecipeId && !r.IsDeleted, cancellationToken);

                if (targetRecipe != null && !string.IsNullOrWhiteSpace(targetRecipe.Slug))
                {
                    _logger.LogInformation("Slug '{OldSlug}' đã chuyển hướng sang '{NewSlug}' (301 Permanent Redirect)", slug, targetRecipe.Slug);
                    return GetRecipeBySlugResult.Redirect(targetRecipe.Slug);
                }
            }

            _logger.LogWarning("Không tìm thấy Recipe nào đang Published với Slug: '{Slug}'", slug);
            return GetRecipeBySlugResult.NotFound();
        }

        var authors = await _userQueryService.GetAuthorSummariesAsync([recipe.AuthorId], cancellationToken);
        var authorSummary = authors.TryGetValue(recipe.AuthorId, out var a) ? a : null;

        // Ánh xạ sang RecipeDetailDto
        var dto = new RecipeDetailDto
        {
            Id = recipe.Id,
            Title = recipe.Title,
            Slug = recipe.Slug,
            Description = recipe.Description,
            PrepTimeMinutes = recipe.PrepTimeMinutes,
            CookTimeMinutes = recipe.CookTimeMinutes,
            Servings = recipe.Servings,
            Difficulty = recipe.Difficulty.ToString(),
            Status = recipe.Status.ToString(),
            PublishedAt = recipe.PublishedAt,
            CreatedAt = recipe.CreatedAt,
            UpdatedAt = recipe.UpdatedAt,
            Category = recipe.Category is null ? null : new CategoryDto
            {
                Id = recipe.Category.Id,
                Name = recipe.Category.Name,
                Slug = recipe.Category.Slug,
                Description = recipe.Category.Description,
                ImageUrl = recipe.Category.ImageUrl,
                OrderIndex = recipe.Category.OrderIndex,
                RecipeCount = 0,
                CreatedAt = recipe.Category.CreatedAt
            },
            Author = authorSummary is null ? null : new RecipeAuthorDto
            {
                Id = authorSummary.Id,
                DisplayName = authorSummary.DisplayName,
                UserName = authorSummary.DisplayName,
                AvatarUrl = authorSummary.AvatarUrl
            },
            Nutrition = recipe.Nutrition is null ? null : new RecipeNutritionDto
            {
                Calories = recipe.Nutrition.Calories,
                Protein = recipe.Nutrition.Protein,
                Carbohydrates = recipe.Nutrition.Carbohydrates,
                Fat = recipe.Nutrition.Fat,
                Fiber = recipe.Nutrition.Fiber,
                Sodium = recipe.Nutrition.Sodium,
                Source = recipe.Nutrition.Source.ToString()
            },
            Ingredients = recipe.Ingredients
                .OrderBy(i => i.OrderIndex)
                .Select(i => new RecipeIngredientDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Notes = i.Notes,
                    OrderIndex = i.OrderIndex
                })
                .ToList(),
            Steps = recipe.Steps
                .OrderBy(s => s.StepNumber)
                .Select(s => new RecipeStepDto
                {
                    Id = s.Id,
                    StepNumber = s.StepNumber,
                    Title = s.Title,
                    Description = s.Description,
                    DurationMinutes = s.DurationMinutes,
                    ImageUrl = s.ImageUrl
                })
                .ToList(),
            Images = recipe.Images
                .OrderBy(img => img.OrderIndex)
                .Select(img => new RecipeImageDto
                {
                    Id = img.Id,
                    OriginalUrl = img.OriginalUrl,
                    MediumUrl = img.MediumUrl,
                    ThumbnailUrl = img.ThumbnailUrl,
                    AltText = img.AltText,
                    IsPrimary = img.IsPrimary,
                    OrderIndex = img.OrderIndex
                })
                .ToList()
        };

        if (_cacheService != null)
        {
            await _cacheService.SetAsync(slugCacheKey, dto, CacheConstants.RecipeDetailDuration, cancellationToken);
            await _cacheService.SetAsync($"{CacheConstants.RecipeDetailPrefix}{recipe.Id}", dto, CacheConstants.RecipeDetailDuration, cancellationToken);
        }

        return GetRecipeBySlugResult.Found(dto);
    }
}
