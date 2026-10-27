namespace CulinaryBlog.Application.Features.Recipes.DTOs;

using CulinaryBlog.Domain.Enums;

/// <summary>
/// DTO đại diện cho công thức nấu ăn đã bị xóa mềm nằm trong thùng rác của Admin.
/// </summary>
public class TrashedRecipeDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public RecipeStatus Status { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public int DaysRemaining { get; set; }
}
