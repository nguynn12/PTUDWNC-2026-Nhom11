namespace CulinaryBlog.API.Endpoints;

using CulinaryBlog.API.Common.Models;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Recipes.DTOs;
using CulinaryBlog.Application.Features.Recipes.Queries.GetRecipeBySlug;
using CulinaryBlog.Application.Features.Recipes.Queries.GetRecipes;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Minimal API Endpoints phục vụ tra cứu và xem công thức nấu ăn theo FR-RCP-001 và FR-RCP-002.
/// </summary>
public static class RecipeEndpoints
{
    public static RouteGroupBuilder MapRecipeEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/recipes")
            .WithTags("Recipes");

        // FR-RCP-001: Duyệt danh sách công thức phân trang và lọc theo Category (Public)
        group.MapGet("", async (
            int? page,
            int? pageSize,
            Guid? categoryId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetRecipesQuery(
                Page: page ?? 1,
                PageSize: pageSize ?? 12,
                CategoryId: categoryId);

            var result = await sender.Send(query, cancellationToken);

            var meta = new
            {
                page = result.Page,
                pageSize = result.PageSize,
                total = result.Total,
                totalPages = result.TotalPages
            };

            return Results.Ok(new ApiResponse<IReadOnlyList<RecipeSummaryDto>>(result.Items, meta));
        })
        .WithName("GetRecipes")
        .WithSummary("Duyệt danh sách công thức nấu ăn")
        .WithDescription("Trả về danh sách công thức đã xuất bản (Published), hỗ trợ phân trang và lọc theo danh mục (FR-RCP-001).")
        .Produces<ApiResponse<IReadOnlyList<RecipeSummaryDto>>>(StatusCodes.Status200OK);

        // FR-RCP-002: Xem chi tiết công thức nấu ăn theo Slug (Public)
        group.MapGet("/{slug}", async (string slug, ISender sender, CancellationToken cancellationToken) =>
        {
            var recipe = await sender.Send(new GetRecipeBySlugQuery(slug), cancellationToken);

            if (recipe is null)
            {
                throw new NotFoundException($"Không tìm thấy công thức nấu ăn với đường dẫn '{slug}'.", "RECIPE_NOT_FOUND");
            }

            return Results.Ok(new ApiResponse<RecipeDetailDto>(recipe));
        })
        .WithName("GetRecipeBySlug")
        .WithSummary("Xem chi tiết công thức nấu ăn theo Slug")
        .WithDescription("Trả về đầy đủ thông tin chi tiết của công thức bao gồm nguyên liệu, các bước nấu, ảnh và dinh dưỡng (FR-RCP-002).")
        .Produces<ApiResponse<RecipeDetailDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}
