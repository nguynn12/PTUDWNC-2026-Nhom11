namespace CulinaryBlog.Application.Features.Recipes.DTOs;

/// <summary>
/// DTO thông tin hình ảnh minh họa công thức.
/// </summary>
public class RecipeImageDto
{
    public Guid Id { get; init; }
    public string OriginalUrl { get; init; } = string.Empty;
    public string? MediumUrl { get; init; }
    public string? ThumbnailUrl { get; init; }
    public string? AltText { get; init; }
    public bool IsPrimary { get; init; }
    public int OrderIndex { get; init; }
}
