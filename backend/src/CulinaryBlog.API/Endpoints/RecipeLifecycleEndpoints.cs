namespace CulinaryBlog.API.Endpoints;

using CulinaryBlog.Application.Features.Recipes.Commands.ArchiveRecipe;
using CulinaryBlog.Application.Features.Recipes.Commands.CreateRecipe;
using CulinaryBlog.Application.Features.Recipes.Commands.DeleteRecipe;
using CulinaryBlog.Application.Features.Recipes.Commands.PublishRecipe;
using CulinaryBlog.Application.Features.Recipes.Commands.UnarchiveRecipe;
using CulinaryBlog.Application.Features.Recipes.Commands.UnpublishRecipe;
using CulinaryBlog.Application.Features.Recipes.Commands.UpdateRecipe;
using CulinaryBlog.Domain.Constants;
using CulinaryBlog.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Các Minimal API endpoints quản lý vòng đời công thức nấu ăn (Recipe Lifecycle)
/// theo phân công Thành viên 3 (Tạ Nhật Nguyên) trong Lab 3 & Lab 5.
/// </summary>
public static class RecipeLifecycleEndpoints
{
    private static uint? ParseIfMatchHeader(string? ifMatch)
    {
        if (string.IsNullOrWhiteSpace(ifMatch)) return null;
        var cleanToken = ifMatch.Trim().Trim('"');
        return uint.TryParse(cleanToken, out var parsed) ? parsed : null;
    }

    public static RouteGroupBuilder MapRecipeLifecycleEndpoints(this RouteGroupBuilder group)
    {
        var recipes = group.MapGroup("/recipes");

        // 1. Tạo mới công thức (trạng thái Draft) -> Yêu cầu role Author/Admin
        recipes.MapPost("/", async (
            CreateRecipeRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateRecipeCommand
            {
                Title = request.Title,
                Description = request.Description,
                PrepTimeMinutes = request.PrepTimeMinutes,
                CookTimeMinutes = request.CookTimeMinutes,
                Servings = request.Servings,
                Difficulty = request.Difficulty,
                CategoryId = request.CategoryId
            };

            var result = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/v1/recipes/{result.Id}", new { data = result });
        })
        .RequireAuthorization(Roles.Author)
        .WithName("CreateRecipe")
        .WithSummary("Tạo công thức nấu ăn mới ở trạng thái Draft (Yêu cầu Author).");

        // 2. Cập nhật thông tin công thức -> Yêu cầu role Author/Admin
        recipes.MapPut("/{id:guid}", async (
            Guid id,
            UpdateRecipeRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            HttpContext httpContext,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            uint? concurrencyToken = ParseIfMatchHeader(ifMatch) ?? request.ConcurrencyToken;

            var command = new UpdateRecipeCommand
            {
                Id = id,
                Title = request.Title,
                Description = request.Description,
                PrepTimeMinutes = request.PrepTimeMinutes,
                CookTimeMinutes = request.CookTimeMinutes,
                Servings = request.Servings,
                Difficulty = request.Difficulty,
                CategoryId = request.CategoryId,
                ConcurrencyToken = concurrencyToken
            };

            var result = await sender.Send(command, cancellationToken);

            if (result.ConcurrencyToken.HasValue)
            {
                httpContext.Response.Headers.ETag = $"\"{result.ConcurrencyToken.Value}\"";
            }

            return Results.Ok(new { data = result });
        })
        .RequireAuthorization(Roles.Author)
        .WithName("UpdateRecipe")
        .WithSummary("Cập nhật thông tin công thức nấu ăn kèm kiểm tra If-Match/xmin.");

        // 3. Xuất bản công thức nấu ăn -> Yêu cầu VerifiedAuthor (email confirmed hoặc Admin theo FR-AUTH-008)
        recipes.MapMethods("/{id:guid}/publish", ["PATCH", "POST"], async (
            Guid id,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            HttpContext httpContext,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            uint? concurrencyToken = ParseIfMatchHeader(ifMatch);
            var result = await sender.Send(new PublishRecipeCommand(id, concurrencyToken), cancellationToken);

            if (result.ConcurrencyToken.HasValue)
            {
                httpContext.Response.Headers.ETag = $"\"{result.ConcurrencyToken.Value}\"";
            }

            return Results.Ok(new { data = result });
        })
        .RequireAuthorization(Policies.VerifiedAuthor)
        .WithName("PublishRecipe")
        .WithSummary("Xuất bản công thức nấu ăn (Yêu cầu VerifiedAuthor/Admin). Hỗ trợ If-Match/ETag.");

        // 4. Hủy xuất bản công thức nấu ăn -> Yêu cầu VerifiedAuthor/Admin
        recipes.MapMethods("/{id:guid}/unpublish", ["PATCH", "POST"], async (
            Guid id,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            HttpContext httpContext,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            uint? concurrencyToken = ParseIfMatchHeader(ifMatch);
            var result = await sender.Send(new UnpublishRecipeCommand(id, concurrencyToken), cancellationToken);

            if (result.ConcurrencyToken.HasValue)
            {
                httpContext.Response.Headers.ETag = $"\"{result.ConcurrencyToken.Value}\"";
            }

            return Results.Ok(new { data = result });
        })
        .RequireAuthorization(Policies.VerifiedAuthor)
        .WithName("UnpublishRecipe")
        .WithSummary("Hủy xuất bản công thức nấu ăn (Yêu cầu VerifiedAuthor/Admin). Hỗ trợ If-Match/ETag.");

        // 5. Lưu trữ công thức nấu ăn -> Yêu cầu Author/Admin
        recipes.MapMethods("/{id:guid}/archive", ["PATCH", "POST"], async (
            Guid id,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            HttpContext httpContext,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            uint? concurrencyToken = ParseIfMatchHeader(ifMatch);
            var result = await sender.Send(new ArchiveRecipeCommand(id, concurrencyToken), cancellationToken);

            if (result.ConcurrencyToken.HasValue)
            {
                httpContext.Response.Headers.ETag = $"\"{result.ConcurrencyToken.Value}\"";
            }

            return Results.Ok(new { data = result });
        })
        .RequireAuthorization(Roles.Author)
        .WithName("ArchiveRecipe")
        .WithSummary("Lưu trữ công thức nấu ăn (Yêu cầu Author). Hỗ trợ If-Match/ETag.");

        // 6. Khôi phục công thức từ lưu trữ -> Yêu cầu Author/Admin
        recipes.MapMethods("/{id:guid}/unarchive", ["PATCH", "POST"], async (
            Guid id,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            HttpContext httpContext,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            uint? concurrencyToken = ParseIfMatchHeader(ifMatch);
            var result = await sender.Send(new UnarchiveRecipeCommand(id, concurrencyToken), cancellationToken);

            if (result.ConcurrencyToken.HasValue)
            {
                httpContext.Response.Headers.ETag = $"\"{result.ConcurrencyToken.Value}\"";
            }

            return Results.Ok(new { data = result });
        })
        .RequireAuthorization(Roles.Author)
        .WithName("UnarchiveRecipe")
        .WithSummary("Khôi phục công thức nấu ăn từ lưu trữ (Yêu cầu Author). Hỗ trợ If-Match/ETag.");

        // 7. Xóa mềm công thức nấu ăn -> Yêu cầu Author/Admin
        recipes.MapDelete("/{id:guid}", async (
            Guid id,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            uint? concurrencyToken = ParseIfMatchHeader(ifMatch);
            await sender.Send(new DeleteRecipeCommand(id, concurrencyToken), cancellationToken);
            return Results.NoContent();
        })
        .RequireAuthorization(Roles.Author)
        .WithName("DeleteRecipe")
        .WithSummary("Xóa mềm công thức nấu ăn (Yêu cầu Author). Hỗ trợ If-Match.");

        // ── 3 API dành riêng cho Quản trị viên (Admin) theo FR-RCP-007 ───────────────
        var adminRecipes = group.MapGroup("/admin/recipes");

        // 8. Xem danh sách công thức trong thùng rác -> Yêu cầu Admin
        adminRecipes.MapGet("/trash", async (
            int page = 1,
            int pageSize = 10,
            ISender sender = null!,
            CancellationToken cancellationToken = default) =>
        {
            var result = await sender.Send(
                new CulinaryBlog.Application.Features.Recipes.Queries.GetTrashedRecipes.GetTrashedRecipesQuery(page, pageSize),
                cancellationToken);

            return Results.Ok(new
            {
                data = result.Items,
                meta = new
                {
                    pageNumber = result.PageNumber,
                    pageSize = result.PageSize,
                    totalCount = result.TotalCount,
                    totalPages = result.TotalPages,
                    hasNextPage = result.HasNextPage,
                    hasPreviousPage = result.HasPreviousPage
                }
            });
        })
        .RequireAuthorization(Roles.Admin)
        .WithName("GetTrashedRecipes")
        .WithSummary("Admin xem danh sách công thức đã bị xóa mềm trong thùng rác.");

        // 9. Khôi phục công thức từ thùng rác (trong thời hạn 30 ngày) -> Yêu cầu Admin
        adminRecipes.MapPost("/{id:guid}/restore", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new CulinaryBlog.Application.Features.Recipes.Commands.RestoreRecipe.RestoreRecipeCommand(id),
                cancellationToken);

            return Results.Ok(new { data = result });
        })
        .RequireAuthorization(Roles.Admin)
        .WithName("RestoreRecipe")
        .WithSummary("Admin khôi phục công thức đã xóa mềm trong vòng 30 ngày.");

        // 10. Xóa vĩnh viễn (xóa vật lý) công thức khỏi hệ thống -> Yêu cầu Admin
        adminRecipes.MapDelete("/{id:guid}/purge", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            await sender.Send(
                new CulinaryBlog.Application.Features.Recipes.Commands.PurgeRecipe.PurgeRecipeCommand(id),
                cancellationToken);

            return Results.NoContent();
        })
        .RequireAuthorization(Roles.Admin)
        .WithName("PurgeRecipe")
        .WithSummary("Admin xóa vật lý vĩnh viễn một công thức đã xóa mềm.");

        return group;
    }
}

public record CreateRecipeRequest(
    string Title,
    string Description,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    RecipeDifficulty Difficulty,
    Guid CategoryId);

public record UpdateRecipeRequest(
    string Title,
    string Description,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    RecipeDifficulty Difficulty,
    Guid CategoryId,
    uint? ConcurrencyToken);
