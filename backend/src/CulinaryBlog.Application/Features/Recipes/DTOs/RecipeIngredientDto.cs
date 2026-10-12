namespace CulinaryBlog.Application.Features.Recipes.DTOs;

/// <summary>
/// DTO thông tin nguyên liệu trong công thức nấu ăn.
/// </summary>
public class RecipeIngredientDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal? Quantity { get; init; }
    public string? Unit { get; init; }
    public string? Notes { get; init; }
    public int OrderIndex { get; init; }
}
