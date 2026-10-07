namespace CulinaryBlog.Application.Features.Recipes.Commands.ArchiveRecipe;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Recipes.DTOs;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using MediatR;

/// <summary>
/// Command lưu trữ công thức nấu ăn (Draft/Published -> Archived).
/// </summary>
public record ArchiveRecipeCommand(Guid Id) : IRequest<RecipeResponseDto>;

/// <summary>
/// Handler xử lý lưu trữ công thức nấu ăn.
/// </summary>
public sealed class ArchiveRecipeCommandHandler(
    IRepository<Recipe> recipeRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<ArchiveRecipeCommand, RecipeResponseDto>
{
    public async Task<RecipeResponseDto> Handle(
        ArchiveRecipeCommand request,
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
            throw new ForbiddenException("Bạn không có quyền lưu trữ công thức này.", "RECIPE_FORBIDDEN");
        }

        // Nếu đã Archived rồi thì báo lỗi
        if (recipe.Status == RecipeStatus.Archived)
        {
            throw new ConflictException("Công thức đã ở trạng thái lưu trữ.", "RECIPE_ALREADY_ARCHIVED");
        }

        recipe.Status = RecipeStatus.Archived;
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
