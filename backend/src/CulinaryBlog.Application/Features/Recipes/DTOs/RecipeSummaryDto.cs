namespace CulinaryBlog.Application.Features.Recipes.DTOs;

/// <summary>
/// DTO tóm tắt công thức nấu ăn phục vụ hiển thị danh sách dạng Card theo FR-RCP-001.
/// </summary>
public class RecipeSummaryDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public int PrepTimeMinutes { get; init; }
    public int CookTimeMinutes { get; init; }
    public int Servings { get; init; }
    public string Difficulty { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public string? CategorySlug { get; init; }
    public string AuthorId { get; init; } = string.Empty;
    public string? AuthorName { get; set; }
    public DateTimeOffset? PublishedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
