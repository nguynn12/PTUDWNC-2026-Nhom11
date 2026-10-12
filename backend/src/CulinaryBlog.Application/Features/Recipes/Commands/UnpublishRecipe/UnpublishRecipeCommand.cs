namespace CulinaryBlog.Application.Features.Recipes.Commands.UnpublishRecipe;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Recipes.DTOs;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using MediatR;

/// <summary>
/// Command hủy xuất bản công thức nấu ăn (Published -> Draft).
/// </summary>
public record UnpublishRecipeCommand(Guid Id) : IRequest<RecipeResponseDto>;

/// <summary>
/// Handler xử lý hủy xuất bản công thức nấu ăn.
/// </summary>
public sealed class UnpublishRecipeCommandHandler(
    IRepository<Recipe> recipeRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<UnpublishRecipeCommand, RecipeResponseDto>
{
    public async Task<RecipeResponseDto> Handle(
        UnpublishRecipeCommand request,
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
            throw new ForbiddenException("Bạn không có quyền hủy xuất bản công thức này.", "RECIPE_FORBIDDEN");
        }

        // Chỉ công thức đang Published mới được Unpublish về Draft
        if (recipe.Status != RecipeStatus.Published)
        {
            throw new ConflictException("Chỉ có thể hủy xuất bản đối với công thức đang ở trạng thái Published.", "RECIPE_INVALID_STATUS_TRANSITION");
        }

        recipe.Status = RecipeStatus.Draft;
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
