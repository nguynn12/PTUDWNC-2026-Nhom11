namespace CulinaryBlog.Application.Features.Recipes.Commands.PublishRecipe;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Recipes.DTOs;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Command xuất bản công thức nấu ăn (Draft -> Published).
/// </summary>
public record PublishRecipeCommand(Guid Id) : IRequest<RecipeResponseDto>;

/// <summary>
/// Handler xử lý xuất bản công thức.
/// Kiểm tra điều kiện C4, C5 (bắt buộc >= 1 Ingredient và >= 1 Step),
/// cập nhật trạng thái Published và thiết lập PublishedAt nếu lần đầu xuất bản.
/// </summary>
public sealed class PublishRecipeCommandHandler(
    IRepository<Recipe> recipeRepository,
    IUnitOfWork unitOfWork,
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService)
    : IRequestHandler<PublishRecipeCommand, RecipeResponseDto>
{
    public async Task<RecipeResponseDto> Handle(
        PublishRecipeCommand request,
        CancellationToken cancellationToken)
    {
        var recipe = await recipeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (recipe == null)
        {
            throw new NotFoundException("Recipe", request.Id, "RECIPE_NOT_FOUND");
        }

        // Kiểm tra quyền sở hữu
        if (!string.IsNullOrWhiteSpace(currentUserService.UserId) &&
            !currentUserService.IsAdmin &&
            !string.Equals(recipe.AuthorId, currentUserService.UserId, StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException("Bạn không có quyền xuất bản công thức này.", "RECIPE_FORBIDDEN");
        }

        // Kiểm tra trạng thái hiện tại
        if (recipe.Status == RecipeStatus.Published)
        {
            throw new ConflictException("Công thức đã ở trạng thái xuất bản.", "RECIPE_ALREADY_PUBLISHED");
        }

        // Kiểm tra điều kiện xuất bản theo RESOLVED-CONFLICTS C5: >= 1 nguyên liệu và >= 1 bước thực hiện
        var ingredientsCount = await dbContext.RecipeIngredients
            .CountAsync(i => i.RecipeId == recipe.Id, cancellationToken);

        var stepsCount = await dbContext.RecipeSteps
            .CountAsync(s => s.RecipeId == recipe.Id, cancellationToken);

        if (ingredientsCount == 0 || stepsCount == 0)
        {
            throw new BusinessRuleValidationException(
                "Công thức cần có ít nhất 1 nguyên liệu và 1 bước thực hiện để được xuất bản.",
                "RECIPE_PUBLISH_INCOMPLETE");
        }

        // Chuyển trạng thái sang Published
        recipe.Status = RecipeStatus.Published;

        // Nếu xuất bản lần đầu, ghi nhận mốc thời gian PublishedAt (đóng băng slug theo C7)
        if (!recipe.PublishedAt.HasValue)
        {
            recipe.PublishedAt = DateTimeOffset.UtcNow;
        }

        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        recipeRepository.Update(recipe);
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
