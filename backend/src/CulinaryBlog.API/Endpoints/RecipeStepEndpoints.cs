namespace CulinaryBlog.API.Endpoints;

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.RecipeSteps.DTOs;
using CulinaryBlog.Application.Features.RecipeSteps.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Đăng ký các Minimal API Endpoints quản lý các bước thực hiện công thức nấu ăn.
/// Tuân thủ SRS v1.2.0 (mục 8.3, FR-RCP-010) và kế hoạch sửa lỗi SEC-01, SEC-02, API-03, API-05, API-08, API-17.
/// </summary>
public static class RecipeStepEndpoints
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

    public static RouteGroupBuilder MapRecipeStepEndpoints(this RouteGroupBuilder group)
    {
        var steps = group.MapGroup("/recipes/{recipeId:guid}/steps")
                         .WithTags("Recipe Steps");

        // GET /api/v1/recipes/{recipeId}/steps (SEC-02)
        steps.MapGet("/", async (
            Guid recipeId,
            IRecipeStepService stepService,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var (userId, isAdmin) = ExtractUserContext(user);
            var result = await stepService.GetStepsAsync(recipeId, userId, isAdmin, ct);
            return Results.Ok(new { data = result });
        })
        .WithName("GetRecipeSteps")
        .WithSummary("Lấy danh sách các bước thực hiện của công thức, sắp xếp theo StepNumber.");

        // POST /api/v1/recipes/{recipeId}/steps (API-03, SEC-01, API-17)
        steps.MapPost("/", async (
            Guid recipeId,
            [FromBody] CreateRecipeStepRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            IRecipeStepService stepService,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var (userId, isAdmin) = ExtractUserContext(user);
            var concurrencyToken = ParseIfMatchHeader(ifMatch);
            var result = await stepService.AddStepAsync(recipeId, request, userId, isAdmin, concurrencyToken, ct);
            return Results.Created($"/api/v1/recipes/{recipeId}/steps/{result.Id}", new { data = result });
        })
        .RequireAuthorization()
        .WithName("AddRecipeStep")
        .WithSummary("Thêm bước thực hiện mới vào công thức nấu ăn. Hỗ trợ If-Match.");

        // PUT /api/v1/recipes/{recipeId}/steps/reorder (API-03, API-05)
        steps.MapPut("/reorder", async (
            Guid recipeId,
            [FromBody] ReorderRecipeStepsRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            IRecipeStepService stepService,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var (userId, isAdmin) = ExtractUserContext(user);
            var concurrencyToken = ParseIfMatchHeader(ifMatch);
            var result = await stepService.ReorderStepsAsync(recipeId, request, userId, isAdmin, concurrencyToken, ct);
            return Results.Ok(new { data = result });
        })
        .RequireAuthorization()
        .WithName("ReorderRecipeSteps")
        .WithSummary("Sắp xếp lại thứ tự toàn bộ các bước thực hiện theo danh sách IDs mới (2-stage update an toàn).");

        // PUT /api/v1/recipes/{recipeId}/steps/{stepId} (API-03)
        steps.MapPut("/{stepId:guid}", async (
            Guid recipeId,
            Guid stepId,
            [FromBody] UpdateRecipeStepRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            IRecipeStepService stepService,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var (userId, isAdmin) = ExtractUserContext(user);
            var concurrencyToken = ParseIfMatchHeader(ifMatch);
            var result = await stepService.UpdateStepAsync(recipeId, stepId, request, userId, isAdmin, concurrencyToken, ct);
            return Results.Ok(new { data = result });
        })
        .RequireAuthorization()
        .WithName("UpdateRecipeStep")
        .WithSummary("Cập nhật thông tin chi tiết một bước thực hiện. Hỗ trợ If-Match.");

        // DELETE /api/v1/recipes/{recipeId}/steps/{stepId} (API-03)
        steps.MapDelete("/{stepId:guid}", async (
            Guid recipeId,
            Guid stepId,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            IRecipeStepService stepService,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var (userId, isAdmin) = ExtractUserContext(user);
            var concurrencyToken = ParseIfMatchHeader(ifMatch);
            await stepService.DeleteStepAsync(recipeId, stepId, userId, isAdmin, concurrencyToken, ct);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("DeleteRecipeStep")
        .WithSummary("Xóa một bước thực hiện và tự động đánh lại số thứ tự (renumber) các bước còn lại.");

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
