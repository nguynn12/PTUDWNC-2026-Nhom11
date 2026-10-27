namespace CulinaryBlog.Application.Features.Recipes.Queries.GetRecipeBySlug;

using CulinaryBlog.Application.Features.Recipes.DTOs;
using MediatR;

/// <summary>
/// Query lấy thông tin chi tiết một công thức nấu ăn dựa trên Slug chuẩn SEO theo FR-RCP-002.
/// </summary>
/// <param name="Slug">Đường dẫn định danh công thức.</param>
public record GetRecipeBySlugQuery(string Slug) : IRequest<RecipeDetailDto?>;
