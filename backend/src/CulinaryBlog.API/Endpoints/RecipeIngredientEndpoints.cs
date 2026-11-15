namespace CulinaryBlog.API.Endpoints;

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.RecipeIngredients.DTOs;
using CulinaryBlog.Application.Features.RecipeIngredients.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Đăng ký các Minimal API Endpoints quản lý nguyên liệu công thức nấu ăn.
/// Tuân thủ SRS v1.2.0 (mục 8.3, FR-RCP-009) và kế hoạch sửa lỗi SEC-01, SEC-02, API-03, API-08.
/// </summary>
public static class RecipeIngredientEndpoints
{
    private static uint? ParseIfMatchHeader(string? ifMatch)
    {
        if (string.IsNullOrWhiteSpace(ifMatch)) return null;
        var cleanToken = ifMatch.Trim().Trim('"');
        if (!uint.TryParse(cleanToken, out var parsed))
        {
            throw new BadRequestException("Tiêu đề If-Match không đúng định dạng.", "INVALID_IF_MATCH_HEADER");
        }
        return parsed;
    }

    public static RouteGroupBuilder MapRecipeIngredientEndpoints(this RouteGroupBuilder group)
    {
        var ingredients = group.MapGroup("/recipes/{recipeId:guid}/ingredients")
                               .WithTags("Recipe Ingredients");

        // GET /api/v1/recipes/{recipeId}/ingredients (SEC-02)
        ingredients.MapGet("/", async (
            Guid recipeId,
            IRecipeIngredientService ingredientService,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var (userId, isAdmin) = ExtractUserContext(user);
            var result = await ingredientService.GetIngredientsAsync(recipeId, userId, isAdmin, ct);
            return Results.Ok(new { data = result });
        })
        .WithName("GetRecipeIngredients")
        .WithSummary("Lấy danh sách toàn bộ nguyên liệu của một công thức.");

        // GET /api/v1/recipes/{recipeId}/ingredients/{ingredientId} (SEC-02)
        ingredients.MapGet("/{ingredientId:guid}", async (
            Guid recipeId,
            Guid ingredientId,
            IRecipeIngredientService ingredientService,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var (userId, isAdmin) = ExtractUserContext(user);
            var result = await ingredientService.GetIngredientByIdAsync(recipeId, ingredientId, userId, isAdmin, ct);
            return Results.Ok(new { data = result });
        })
        .WithName("GetRecipeIngredientById")
        .WithSummary("Lấy thông tin chi tiết một nguyên liệu theo ID.");

        // POST /api/v1/recipes/{recipeId}/ingredients (API-03, SEC-01)
        ingredients.MapPost("/", async (
            Guid recipeId,
            [FromBody] CreateRecipeIngredientRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            IRecipeIngredientService ingredientService,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var (userId, isAdmin) = ExtractUserContext(user);
            var concurrencyToken = ParseIfMatchHeader(ifMatch);
            var result = await ingredientService.AddIngredientAsync(recipeId, request, userId, isAdmin, concurrencyToken, ct);
            return Results.Created($"/api/v1/recipes/{recipeId}/ingredients/{result.Id}", new { data = result });
        })
        .RequireAuthorization()
        .WithName("AddRecipeIngredient")
        .WithSummary("Thêm nguyên liệu mới vào công thức nấu ăn. Hỗ trợ If-Match.");

        // PUT /api/v1/recipes/{recipeId}/ingredients/{ingredientId} (API-03, SEC-01)
        ingredients.MapPut("/{ingredientId:guid}", async (
            Guid recipeId,
            Guid ingredientId,
            [FromBody] UpdateRecipeIngredientRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            IRecipeIngredientService ingredientService,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var (userId, isAdmin) = ExtractUserContext(user);
            var concurrencyToken = ParseIfMatchHeader(ifMatch);
            var result = await ingredientService.UpdateIngredientAsync(recipeId, ingredientId, request, userId, isAdmin, concurrencyToken, ct);
            return Results.Ok(new { data = result });
        })
        .RequireAuthorization()
        .WithName("UpdateRecipeIngredient")
        .WithSummary("Cập nhật thông tin chi tiết một nguyên liệu trong công thức. Hỗ trợ If-Match.");

        // DELETE /api/v1/recipes/{recipeId}/ingredients/{ingredientId} (API-03, SEC-01)
        ingredients.MapDelete("/{ingredientId:guid}", async (
            Guid recipeId,
            Guid ingredientId,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            IRecipeIngredientService ingredientService,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var (userId, isAdmin) = ExtractUserContext(user);
            var concurrencyToken = ParseIfMatchHeader(ifMatch);
            await ingredientService.DeleteIngredientAsync(recipeId, ingredientId, userId, isAdmin, concurrencyToken, ct);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("DeleteRecipeIngredient")
        .WithSummary("Xóa mềm một nguyên liệu khỏi công thức nấu ăn. Hỗ trợ If-Match.");

        return group;
    }

    private static (string? UserId, bool IsAdmin) ExtractUserContext(ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? user.FindFirstValue("sub")
                  ?? user.FindFirstValue("id");

        var isAdmin = user.IsInRole("Admin")
                   || user.IsInRole("Administrator");

        return (userId, isAdmin);
    }
}
