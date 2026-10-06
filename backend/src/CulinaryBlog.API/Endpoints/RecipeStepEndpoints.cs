namespace CulinaryBlog.API.Endpoints;

using System.Security.Claims;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.RecipeSteps.DTOs;
using CulinaryBlog.Application.Features.RecipeSteps.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Đăng ký các Minimal API Endpoints quản lý các bước thực hiện công thức nấu ăn.
/// Tuân thủ SRS v1.2.0 (mục 8.3, FR-RCP-010) và RESOLVED-CONFLICTS.md (C2).
/// </summary>
public static class RecipeStepEndpoints
{
    public static RouteGroupBuilder MapRecipeStepEndpoints(this RouteGroupBuilder group)
    {
        var steps = group.MapGroup("/recipes/{recipeId:guid}/steps")
                         .WithTags("Recipe Steps");

        // GET /api/v1/recipes/{recipeId}/steps
        steps.MapGet("/", async (
            Guid recipeId,
            IRecipeStepService stepService,
            CancellationToken ct) =>
        {
            var result = await stepService.GetStepsAsync(recipeId, ct);
            return Results.Ok(ApiResponse<IReadOnlyList<RecipeStepDto>>.Success(result));
        })
        .WithName("GetRecipeSteps")
        .WithSummary("Lấy danh sách các bước thực hiện của công thức, sắp xếp theo StepNumber.");

        // POST /api/v1/recipes/{recipeId}/steps
        steps.MapPost("/", async (
            Guid recipeId,
            [FromBody] CreateRecipeStepRequest request,
            IRecipeStepService stepService,
            ClaimsPrincipal user,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var (userId, isAdmin) = ExtractUserContext(user, httpContext);
            var result = await stepService.AddStepAsync(recipeId, request, userId, isAdmin, ct);
            return Results.Created($"/api/v1/recipes/{recipeId}/steps", ApiResponse<RecipeStepDto>.Success(result));
        })
        .WithName("AddRecipeStep")
        .WithSummary("Thêm bước thực hiện mới vào công thức nấu ăn.");

        // PUT /api/v1/recipes/{recipeId}/steps/reorder
        steps.MapPut("/reorder", async (
            Guid recipeId,
            [FromBody] ReorderRecipeStepsRequest request,
            IRecipeStepService stepService,
            ClaimsPrincipal user,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var (userId, isAdmin) = ExtractUserContext(user, httpContext);
            var result = await stepService.ReorderStepsAsync(recipeId, request, userId, isAdmin, ct);
            return Results.Ok(ApiResponse<IReadOnlyList<RecipeStepDto>>.Success(result));
        })
        .WithName("ReorderRecipeSteps")
        .WithSummary("Sắp xếp lại thứ tự toàn bộ các bước thực hiện theo danh sách IDs mới.");

        // PUT /api/v1/recipes/{recipeId}/steps/{stepId}
        steps.MapPut("/{stepId:guid}", async (
            Guid recipeId,
            Guid stepId,
            [FromBody] UpdateRecipeStepRequest request,
            IRecipeStepService stepService,
            ClaimsPrincipal user,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var (userId, isAdmin) = ExtractUserContext(user, httpContext);
            var result = await stepService.UpdateStepAsync(recipeId, stepId, request, userId, isAdmin, ct);
            return Results.Ok(ApiResponse<RecipeStepDto>.Success(result));
        })
        .WithName("UpdateRecipeStep")
        .WithSummary("Cập nhật thông tin chi tiết một bước thực hiện.");

        // DELETE /api/v1/recipes/{recipeId}/steps/{stepId}
        steps.MapDelete("/{stepId:guid}", async (
            Guid recipeId,
            Guid stepId,
            IRecipeStepService stepService,
            ClaimsPrincipal user,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var (userId, isAdmin) = ExtractUserContext(user, httpContext);
            await stepService.DeleteStepAsync(recipeId, stepId, userId, isAdmin, ct);
            return Results.NoContent();
        })
        .WithName("DeleteRecipeStep")
        .WithSummary("Xóa một bước thực hiện và tự động đánh lại số thứ tự (renumber) các bước còn lại.");

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
