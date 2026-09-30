namespace CulinaryBlog.Application.Features.Recipes.Commands.UpdateRecipe;

using CulinaryBlog.Application.Features.Recipes.DTOs;
using CulinaryBlog.Domain.Enums;
using MediatR;

/// <summary>
/// Command cập nhật thông tin công thức nấu ăn.
/// Hỗ trợ kiểm tra quyền tác giả, bất biến slug sau xuất bản, và concurrency token (xmin).
/// </summary>
public record UpdateRecipeCommand : IRequest<RecipeResponseDto>
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int PrepTimeMinutes { get; init; }
    public int CookTimeMinutes { get; init; }
    public int Servings { get; init; }
    public RecipeDifficulty Difficulty { get; init; } = RecipeDifficulty.Easy;
    public Guid CategoryId { get; init; }
    public uint? ConcurrencyToken { get; init; }
}
