namespace CulinaryBlog.Application.Features.Recipes.Queries.GetRecipeBySlug;

using CulinaryBlog.Application.Features.Recipes.DTOs;
using MediatR;

/// <summary>
/// Kết quả truy vấn công thức theo Slug, hỗ trợ chuyển hướng 301 khi slug đã đổi (RESOLVED-CONFLICTS C7).
/// </summary>
public record GetRecipeBySlugResult
{
    public RecipeDetailDto? Recipe { get; init; }
    public string? RedirectSlug { get; init; }
    public bool IsRedirect => !string.IsNullOrEmpty(RedirectSlug);

    public static GetRecipeBySlugResult Found(RecipeDetailDto recipe) => new() { Recipe = recipe };
    public static GetRecipeBySlugResult Redirect(string targetSlug) => new() { RedirectSlug = targetSlug };
    public static GetRecipeBySlugResult NotFound() => new();
}

/// <summary>
/// Query lấy thông tin chi tiết một công thức nấu ăn dựa trên Slug chuẩn SEO theo FR-RCP-002.
/// </summary>
/// <param name="Slug">Đường dẫn định danh công thức.</param>
public record GetRecipeBySlugQuery(string Slug) : IRequest<GetRecipeBySlugResult>;
