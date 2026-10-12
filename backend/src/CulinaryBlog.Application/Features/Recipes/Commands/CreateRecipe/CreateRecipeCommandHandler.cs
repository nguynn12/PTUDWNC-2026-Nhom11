namespace CulinaryBlog.Application.Features.Recipes.Commands.CreateRecipe;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Helpers;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Recipes.DTOs;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Handler xử lý tạo mới công thức nấu ăn.
/// Áp dụng IRepository&lt;Recipe&gt; và IUnitOfWork cho phía ghi dữ liệu,
/// IApplicationDbContext cho phía đọc kiểm tra ràng buộc.
/// </summary>
public sealed class CreateRecipeCommandHandler(
    IRepository<Recipe> recipeRepository,
    IUnitOfWork unitOfWork,
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateRecipeCommand, RecipeResponseDto>
{
    public async Task<RecipeResponseDto> Handle(
        CreateRecipeCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Xác định tác giả của công thức (từ user đăng nhập hoặc từ request)
        var authorId = !string.IsNullOrWhiteSpace(currentUserService.UserId)
            ? currentUserService.UserId
            : request.AuthorId;

        if (string.IsNullOrWhiteSpace(authorId))
        {
            throw new ForbiddenException("Không thể xác định tác giả. Vui lòng đăng nhập trước khi tạo công thức.", "RECIPE_FORBIDDEN");
        }

        // 2. Kiểm tra danh mục món ăn có tồn tại hay không
        var categoryExists = await dbContext.Categories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            throw new NotFoundException("Category", request.CategoryId, "CATEGORY_NOT_FOUND");
        }

        // 3. Tự động sinh Slug URL-friendly từ Title
        var baseSlug = SlugHelper.GenerateSlug(request.Title);
        var slug = baseSlug;
        var suffix = 1;

        // Xử lý tự động thêm hậu tố nếu slug bị trùng (tuân thủ RESOLVED-CONFLICTS C7)
        while (await dbContext.Recipes.AnyAsync(r => r.Slug == slug, cancellationToken) ||
               await dbContext.RecipeSlugHistories.AnyAsync(h => h.OldSlug == slug, cancellationToken))
        {
            slug = $"{baseSlug}-{suffix++}";
        }

        // 4. Khởi tạo thực thể Recipe mới ở trạng thái Draft
        var recipe = new Recipe
        {
            Title = request.Title.Trim(),
            Slug = slug,
            Description = request.Description.Trim(),
            PrepTimeMinutes = request.PrepTimeMinutes,
            CookTimeMinutes = request.CookTimeMinutes,
            Servings = request.Servings,
            Difficulty = request.Difficulty,
            Status = RecipeStatus.Draft,
            CategoryId = request.CategoryId,
            AuthorId = authorId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // 5. Thêm mới qua IRepository và commit qua IUnitOfWork
        recipeRepository.Add(recipe);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RecipeResponseDto
        {
            Id = recipe.Id,
            Title = recipe.Title,
            Slug = recipe.Slug,
            Description = recipe.Description,
            PrepTimeMinutes = recipe.PrepTimeMinutes,
            CookTimeMinutes = recipe.CookTimeMinutes,
            Servings = recipe.Servings,
            Difficulty = recipe.Difficulty,
            Status = recipe.Status,
            CategoryId = recipe.CategoryId,
            AuthorId = recipe.AuthorId,
            PublishedAt = recipe.PublishedAt,
            CreatedAt = recipe.CreatedAt,
            UpdatedAt = recipe.UpdatedAt
        };
    }
}
