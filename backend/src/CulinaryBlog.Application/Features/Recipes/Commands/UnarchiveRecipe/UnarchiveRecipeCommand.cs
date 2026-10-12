namespace CulinaryBlog.Application.Features.Recipes.Commands.UnarchiveRecipe;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Recipes.DTOs;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using MediatR;

/// <summary>
/// Command khôi phục công thức nấu ăn từ lưu trữ (Archived -> Draft).
/// </summary>
public record UnarchiveRecipeCommand(Guid Id) : IRequest<RecipeResponseDto>;

/// <summary>
/// Handler xử lý bỏ lưu trữ công thức nấu ăn, đưa về trạng thái Draft.
/// </summary>
public sealed class UnarchiveRecipeCommandHandler(
    IRepository<Recipe> recipeRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<UnarchiveRecipeCommand, RecipeResponseDto>
{
    public async Task<RecipeResponseDto> Handle(
        UnarchiveRecipeCommand request,
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
            throw new ForbiddenException("Bạn không có quyền khôi phục công thức này.", "RECIPE_FORBIDDEN");
        }

        // Chỉ công thức đang Archived mới được Unarchive
        if (recipe.Status != RecipeStatus.Archived)
        {
            throw new ConflictException("Chỉ có thể khôi phục đối với công thức đang ở trạng thái Archived.", "RECIPE_NOT_ARCHIVED");
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
