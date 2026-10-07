namespace CulinaryBlog.Application.Features.Recipes.Commands.CreateRecipe;

using CulinaryBlog.Application.Features.Recipes.DTOs;
using CulinaryBlog.Domain.Enums;
using MediatR;

/// <summary>
/// Command tạo mới công thức nấu ăn ở trạng thái bản nháp (Draft).
/// </summary>
public record CreateRecipeCommand : IRequest<RecipeResponseDto>
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int PrepTimeMinutes { get; init; }
    public int CookTimeMinutes { get; init; }
    public int Servings { get; init; }
    public RecipeDifficulty Difficulty { get; init; } = RecipeDifficulty.Easy;
    public Guid CategoryId { get; init; }
    public string? AuthorId { get; init; }
}
