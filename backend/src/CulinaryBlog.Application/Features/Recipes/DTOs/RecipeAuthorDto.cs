namespace CulinaryBlog.Application.Features.Recipes.DTOs;

/// <summary>
/// DTO thông tin tác giả của công thức nấu ăn.
/// </summary>
public class RecipeAuthorDto
{
    public string Id { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? UserName { get; init; }
    public string? AvatarUrl { get; init; }
}
