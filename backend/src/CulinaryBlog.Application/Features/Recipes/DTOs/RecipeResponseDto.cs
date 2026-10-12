namespace CulinaryBlog.Application.Features.Recipes.DTOs;

using CulinaryBlog.Domain.Enums;

/// <summary>
/// Data Transfer Object phản hồi thông tin công thức sau các thao tác vòng đời (Recipe Lifecycle).
/// </summary>
public class RecipeResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Servings { get; set; }
    public RecipeDifficulty Difficulty { get; set; }
    public RecipeStatus Status { get; set; }
    public Guid CategoryId { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public uint? ConcurrencyToken { get; set; }
}
