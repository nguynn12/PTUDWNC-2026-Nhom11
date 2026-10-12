namespace CulinaryBlog.Application.Features.Recipes.DTOs;

/// <summary>
/// DTO thông tin các bước chế biến món ăn.
/// </summary>
public class RecipeStepDto
{
    public Guid Id { get; init; }
    public int StepNumber { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int? DurationMinutes { get; init; }
    public string? ImageUrl { get; init; }
}
