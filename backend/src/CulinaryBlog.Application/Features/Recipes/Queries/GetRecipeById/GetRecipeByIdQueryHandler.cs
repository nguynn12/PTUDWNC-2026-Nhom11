namespace CulinaryBlog.Application.Features.Recipes.Queries.GetRecipeById;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Categories.DTOs;
using CulinaryBlog.Application.Features.Recipes.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler xử lý GetRecipeByIdQuery: lấy chi tiết công thức nấu ăn theo ID phục vụ màn hình chỉnh sửa (Author/Admin).
/// Kiểm tra nghiêm ngặt quyền sở hữu (Owner/Admin), không đưa dữ liệu bản nháp vào cache công khai,
/// và trả về đầy đủ Concurrency Token phục vụ Optimistic Concurrency Control (If-Match).
/// </summary>
public class GetRecipeByIdQueryHandler(
    IApplicationDbContext dbContext,
    IUserQueryService userQueryService,
    ICurrentUserService currentUserService,
    ILogger<GetRecipeByIdQueryHandler> logger)
    : IRequestHandler<GetRecipeByIdQuery, RecipeDetailDto>
{
    public async Task<RecipeDetailDto> Handle(GetRecipeByIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra xác thực người dùng
        if (string.IsNullOrWhiteSpace(currentUserService.UserId))
        {
            throw new UnauthorizedException("Vui lòng đăng nhập để xem thông tin chi tiết công thức.", "AUTH_UNAUTHORIZED");
        }

        logger.LogInformation("Truy vấn chi tiết Recipe theo ID={Id} bởi User={UserId}", request.Id, currentUserService.UserId);

        // 2. Truy vấn Recipe kèm các liên kết (Category, Ingredients, Steps, Images)
        var recipe = await dbContext.Recipes
            .AsNoTracking()
            .Include(r => r.Category)
            .Include(r => r.Ingredients.Where(i => !i.IsDeleted))
            .Include(r => r.Steps.Where(s => !s.IsDeleted))
            .Include(r => r.Images.Where(img => !img.IsDeleted))
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken);

        if (recipe is null)
        {
            throw new NotFoundException("Recipe", request.Id, "RECIPE_NOT_FOUND");
        }

        // 3. Kiểm tra quyền sở hữu: Phải là tác giả của bài viết hoặc có role Admin
        if (!currentUserService.IsAdmin &&
            !string.Equals(recipe.AuthorId, currentUserService.UserId, StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException("Bạn không có quyền truy cập hoặc chỉnh sửa công thức này.", "RECIPE_FORBIDDEN");
        }

        // 4. Lấy thông tin tác giả
        var authors = await userQueryService.GetAuthorSummariesAsync([recipe.AuthorId], cancellationToken);
        var authorSummary = authors.TryGetValue(recipe.AuthorId, out var a) ? a : null;

        // 5. Lấy Concurrency Token từ shadow property xmin
        var token = await dbContext.Recipes
            .Where(r => r.Id == recipe.Id)
            .Select(r => EF.Property<uint>(r, "xmin"))
            .FirstOrDefaultAsync(cancellationToken);

        // 6. Ánh xạ sang RecipeDetailDto
        return new RecipeDetailDto
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
            ConcurrencyToken = token != 0 ? token : null,
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
    }
}
