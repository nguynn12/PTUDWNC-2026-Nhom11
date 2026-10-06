namespace CulinaryBlog.Application.Features.Recipes.DTOs;

/// <summary>
/// DTO thông tin giá trị dinh dưỡng của công thức nấu ăn.
/// </summary>
public class RecipeNutritionDto
{
    public decimal? Calories { get; init; }
    public decimal? Protein { get; init; }
    public decimal? Carbohydrates { get; init; }
    public decimal? Fat { get; init; }
    public decimal? Fiber { get; init; }
    public decimal? Sodium { get; init; }
    public string Source { get; init; } = "Manual";
}
