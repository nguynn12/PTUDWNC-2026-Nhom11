namespace CulinaryBlog.Application.Features.Recipes.DTOs;

using CulinaryBlog.Application.Features.Categories.DTOs;

/// <summary>
/// DTO chi tiết toàn bộ công thức nấu ăn phục vụ hiển thị chi tiết theo FR-RCP-002.
/// Bao gồm thông tin cơ bản, danh mục, tác giả, dinh dưỡng, nguyên liệu, các bước nấu và bộ ảnh.
/// </summary>
public class RecipeDetailDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int PrepTimeMinutes { get; init; }
    public int CookTimeMinutes { get; init; }
    public int Servings { get; init; }
    public string Difficulty { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset? PublishedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }

    public CategoryDto? Category { get; init; }
    public RecipeAuthorDto? Author { get; init; }
    public RecipeNutritionDto? Nutrition { get; init; }

    public IReadOnlyList<RecipeIngredientDto> Ingredients { get; init; } = Array.Empty<RecipeIngredientDto>();
    public IReadOnlyList<RecipeStepDto> Steps { get; init; } = Array.Empty<RecipeStepDto>();
    public IReadOnlyList<RecipeImageDto> Images { get; init; } = Array.Empty<RecipeImageDto>();
}
