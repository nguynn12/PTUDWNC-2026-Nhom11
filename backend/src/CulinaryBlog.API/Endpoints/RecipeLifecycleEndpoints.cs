namespace CulinaryBlog.API.Endpoints;

using CulinaryBlog.Application.Features.Recipes.Commands.ArchiveRecipe;
using CulinaryBlog.Application.Features.Recipes.Commands.CreateRecipe;
using CulinaryBlog.Application.Features.Recipes.Commands.DeleteRecipe;
using CulinaryBlog.Application.Features.Recipes.Commands.PublishRecipe;
using CulinaryBlog.Application.Features.Recipes.Commands.UnarchiveRecipe;
using CulinaryBlog.Application.Features.Recipes.Commands.UnpublishRecipe;
using CulinaryBlog.Application.Features.Recipes.Commands.UpdateRecipe;
using CulinaryBlog.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Các Minimal API endpoints quản lý vòng đời công thức nấu ăn (Recipe Lifecycle)
/// theo phân công Thành viên 3 (Tạ Nhật Nguyên) trong Lab 3.
/// </summary>
public static class RecipeLifecycleEndpoints
{
    public static RouteGroupBuilder MapRecipeLifecycleEndpoints(this RouteGroupBuilder group)
    {
        var recipes = group.MapGroup("/recipes");

        // 1. Tạo mới công thức (trạng thái Draft)
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
        .WithName("CreateRecipe")
        .WithSummary("Tạo công thức nấu ăn mới ở trạng thái Draft.");

        // 2. Cập nhật thông tin công thức (kiểm tra quyền sở hữu và Concurrency qua If-Match/xmin)
        recipes.MapPut("/{id:guid}", async (
            Guid id,
            UpdateRecipeRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            HttpContext httpContext,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            uint? concurrencyToken = null;
            if (!string.IsNullOrWhiteSpace(ifMatch))
            {
                var cleanToken = ifMatch.Trim('"');
                if (uint.TryParse(cleanToken, out var parsedToken))
                {
                    concurrencyToken = parsedToken;
                }
            }
            else if (request.ConcurrencyToken.HasValue)
            {
                concurrencyToken = request.ConcurrencyToken;
            }

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
        .WithName("UpdateRecipe")
        .WithSummary("Cập nhật thông tin công thức nấu ăn kèm kiểm tra If-Match/xmin.");

        // 3. Xuất bản công thức nấu ăn (Draft -> Published) - Hỗ trợ cả PATCH (theo SRS FR-RCP-005) và POST
        recipes.MapMethods("/{id:guid}/publish", ["PATCH", "POST"], async (
            Guid id,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new PublishRecipeCommand(id), cancellationToken);
            return Results.Ok(new { data = result });
        })
        .WithName("PublishRecipe")
        .WithSummary("Xuất bản công thức nấu ăn (Draft -> Published). Hỗ trợ PATCH/POST.");

        // 4. Hủy xuất bản công thức nấu ăn (Published -> Draft) - Hỗ trợ cả PATCH (theo SRS FR-RCP-005) và POST
        recipes.MapMethods("/{id:guid}/unpublish", ["PATCH", "POST"], async (
            Guid id,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new UnpublishRecipeCommand(id), cancellationToken);
            return Results.Ok(new { data = result });
        })
        .WithName("UnpublishRecipe")
        .WithSummary("Hủy xuất bản công thức nấu ăn (Published -> Draft). Hỗ trợ PATCH/POST.");

        // 5. Lưu trữ công thức nấu ăn (Draft/Published -> Archived) - Hỗ trợ cả PATCH (theo SRS FR-RCP-006) và POST
        recipes.MapMethods("/{id:guid}/archive", ["PATCH", "POST"], async (
            Guid id,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new ArchiveRecipeCommand(id), cancellationToken);
            return Results.Ok(new { data = result });
        })
        .WithName("ArchiveRecipe")
        .WithSummary("Lưu trữ công thức nấu ăn (Draft/Published -> Archived). Hỗ trợ PATCH/POST.");

        // 6. Khôi phục công thức từ lưu trữ (Archived -> Draft) - Hỗ trợ cả PATCH (theo SRS FR-RCP-006) và POST
        recipes.MapMethods("/{id:guid}/unarchive", ["PATCH", "POST"], async (
            Guid id,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new UnarchiveRecipeCommand(id), cancellationToken);
            return Results.Ok(new { data = result });
        })
        .WithName("UnarchiveRecipe")
        .WithSummary("Khôi phục công thức nấu ăn từ lưu trữ (Archived -> Draft). Hỗ trợ PATCH/POST.");

        // 7. Xóa mềm công thức nấu ăn (Soft Delete theo SRS FR-RCP-007)
        recipes.MapDelete("/{id:guid}", async (
            Guid id,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            await sender.Send(new DeleteRecipeCommand(id), cancellationToken);
            return Results.NoContent();
        })
        .WithName("DeleteRecipe")
        .WithSummary("Xóa mềm công thức nấu ăn (Soft Delete). Hỗ trợ If-Match.");

        // ── 3 API dành riêng cho Quản trị viên (Admin) theo FR-RCP-007 ───────────────
        var adminRecipes = group.MapGroup("/admin/recipes");

        // 8. Xem danh sách công thức trong thùng rác
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
        .WithName("GetTrashedRecipes")
        .WithSummary("Admin xem danh sách công thức đã bị xóa mềm trong thùng rác.");

        // 9. Khôi phục công thức từ thùng rác (trong thời hạn 30 ngày)
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
        .WithName("RestoreRecipe")
        .WithSummary("Admin khôi phục công thức đã xóa mềm trong vòng 30 ngày.");

        // 10. Xóa vĩnh viễn (xóa vật lý) công thức khỏi hệ thống
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
