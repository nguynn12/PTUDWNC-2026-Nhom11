namespace CulinaryBlog.API.Endpoints;

using System.Security.Claims;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.RecipeIngredients.DTOs;
using CulinaryBlog.Application.Features.RecipeIngredients.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Đăng ký các Minimal API Endpoints quản lý nguyên liệu công thức nấu ăn.
/// Tuân thủ SRS v1.2.0 (mục 8.3, FR-RCP-009) và RESOLVED-CONFLICTS.md (C2).
/// </summary>
public static class RecipeIngredientEndpoints
{
    public static RouteGroupBuilder MapRecipeIngredientEndpoints(this RouteGroupBuilder group)
    {
        var ingredients = group.MapGroup("/recipes/{recipeId:guid}/ingredients")
                               .WithTags("Recipe Ingredients");

        // GET /api/v1/recipes/{recipeId}/ingredients
        ingredients.MapGet("/", async (
            Guid recipeId,
            IRecipeIngredientService ingredientService,
            CancellationToken ct) =>
        {
            var result = await ingredientService.GetIngredientsAsync(recipeId, ct);
            return Results.Ok(ApiResponse<IReadOnlyList<RecipeIngredientDto>>.Success(result));
        })
        .WithName("GetRecipeIngredients")
        .WithSummary("Lấy danh sách toàn bộ nguyên liệu của một công thức.");

        // GET /api/v1/recipes/{recipeId}/ingredients/{ingredientId}
        ingredients.MapGet("/{ingredientId:guid}", async (
            Guid recipeId,
            Guid ingredientId,
            IRecipeIngredientService ingredientService,
            CancellationToken ct) =>
        {
            var result = await ingredientService.GetIngredientByIdAsync(recipeId, ingredientId, ct);
            return Results.Ok(ApiResponse<RecipeIngredientDto>.Success(result));
        })
        .WithName("GetRecipeIngredientById")
        .WithSummary("Lấy thông tin chi tiết một nguyên liệu theo ID.");

        // POST /api/v1/recipes/{recipeId}/ingredients
        ingredients.MapPost("/", async (
            Guid recipeId,
            [FromBody] CreateRecipeIngredientRequest request,
            IRecipeIngredientService ingredientService,
            ClaimsPrincipal user,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var (userId, isAdmin) = ExtractUserContext(user, httpContext);
            var result = await ingredientService.AddIngredientAsync(recipeId, request, userId, isAdmin, ct);
            return Results.Created($"/api/v1/recipes/{recipeId}/ingredients", ApiResponse<RecipeIngredientDto>.Success(result));
        })
        .WithName("AddRecipeIngredient")
        .WithSummary("Thêm nguyên liệu mới vào công thức nấu ăn.");

        // PUT /api/v1/recipes/{recipeId}/ingredients/{ingredientId}
        ingredients.MapPut("/{ingredientId:guid}", async (
            Guid recipeId,
            Guid ingredientId,
            [FromBody] UpdateRecipeIngredientRequest request,
            IRecipeIngredientService ingredientService,
            ClaimsPrincipal user,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var (userId, isAdmin) = ExtractUserContext(user, httpContext);
            var result = await ingredientService.UpdateIngredientAsync(recipeId, ingredientId, request, userId, isAdmin, ct);
            return Results.Ok(ApiResponse<RecipeIngredientDto>.Success(result));
        })
        .WithName("UpdateRecipeIngredient")
        .WithSummary("Cập nhật thông tin chi tiết một nguyên liệu trong công thức.");

        // DELETE /api/v1/recipes/{recipeId}/ingredients/{ingredientId}
        ingredients.MapDelete("/{ingredientId:guid}", async (
            Guid recipeId,
            Guid ingredientId,
            IRecipeIngredientService ingredientService,
            ClaimsPrincipal user,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var (userId, isAdmin) = ExtractUserContext(user, httpContext);
            await ingredientService.DeleteIngredientAsync(recipeId, ingredientId, userId, isAdmin, ct);
            return Results.NoContent();
        })
        .WithName("DeleteRecipeIngredient")
        .WithSummary("Xóa mềm một nguyên liệu khỏi công thức nấu ăn.");

        return group;
    }

    private static (string? UserId, bool IsAdmin) ExtractUserContext(ClaimsPrincipal user, HttpContext httpContext)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? user.FindFirstValue("sub")
                  ?? user.FindFirstValue("id")
                  ?? httpContext.Request.Headers["X-User-Id"].FirstOrDefault();

        var isAdmin = user.IsInRole("Admin")
                   || user.IsInRole("Administrator")
                   || string.Equals(httpContext.Request.Headers["X-User-Role"].FirstOrDefault(), "Admin", StringComparison.OrdinalIgnoreCase);

        return (userId, isAdmin);
    }
}
