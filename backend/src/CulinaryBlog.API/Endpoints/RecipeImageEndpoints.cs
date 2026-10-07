namespace CulinaryBlog.API.Endpoints;

using System.Security.Claims;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.RecipeImages.DTOs;
using CulinaryBlog.Application.Features.RecipeImages.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Đăng ký các Minimal API Endpoints quản lý hình ảnh minh họa công thức nấu ăn.
/// Tuân thủ SRS v1.2.0 (mục 8.3, FR-RCP-008) và RESOLVED-CONFLICTS.md (E6, E7, C2).
/// </summary>
public static class RecipeImageEndpoints
{
    public static RouteGroupBuilder MapRecipeImageEndpoints(this RouteGroupBuilder group)
    {
        var images = group.MapGroup("/recipes/{recipeId:guid}/images")
                          .WithTags("Recipe Images");

        // GET /api/v1/recipes/{recipeId}/images
        images.MapGet("/", async (
            Guid recipeId,
            IRecipeImageService imageService,
            CancellationToken ct) =>
        {
            var result = await imageService.GetImagesAsync(recipeId, ct);
            return Results.Ok(ApiResponse<IReadOnlyList<RecipeImageDto>>.Success(result));
        })
        .WithName("GetRecipeImages")
        .WithSummary("Lấy danh sách hình ảnh minh họa của một công thức.");

        // POST /api/v1/recipes/{recipeId}/images
        images.MapPost("/", async (
            Guid recipeId,
            IFormFile? file,
            [FromForm] string? altText,
            [FromForm] bool? isPrimary,
            IRecipeImageService imageService,
            ClaimsPrincipal user,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            if (file == null || file.Length == 0)
            {
                throw new BadRequestException("Tệp tin hình ảnh không được để trống.");
            }

            var (userId, isAdmin) = ExtractUserContext(user, httpContext);

            using var stream = file.OpenReadStream();
            var result = await imageService.UploadImageAsync(
                recipeId,
                stream,
                file.FileName,
                file.ContentType,
                file.Length,
                altText,
                isPrimary,
                userId,
                isAdmin,
                ct);

            return Results.Created($"/api/v1/recipes/{recipeId}/images", ApiResponse<RecipeImageDto>.Success(result));
        })
        .DisableAntiforgery()
        .WithName("UploadRecipeImage")
        .WithSummary("Tải lên hình ảnh minh họa mới cho công thức (Multipart Form-Data).");

        // PATCH /api/v1/recipes/{recipeId}/images/{imageId}
        images.MapPatch("/{imageId:guid}", async (
            Guid recipeId,
            Guid imageId,
            [FromBody] UpdateRecipeImageRequest request,
            IRecipeImageService imageService,
            ClaimsPrincipal user,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var (userId, isAdmin) = ExtractUserContext(user, httpContext);
            var result = await imageService.UpdateImageAsync(recipeId, imageId, request, userId, isAdmin, ct);
            return Results.Ok(ApiResponse<RecipeImageDto>.Success(result));
        })
        .WithName("UpdateRecipeImage")
        .WithSummary("Cập nhật metadata hoặc đặt ảnh chính (IsPrimary) cho công thức.");

        // DELETE /api/v1/recipes/{recipeId}/images/{imageId}
        images.MapDelete("/{imageId:guid}", async (
            Guid recipeId,
            Guid imageId,
            IRecipeImageService imageService,
            ClaimsPrincipal user,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var (userId, isAdmin) = ExtractUserContext(user, httpContext);
            await imageService.DeleteImageAsync(recipeId, imageId, userId, isAdmin, ct);
            return Results.NoContent();
        })
        .WithName("DeleteRecipeImage")
        .WithSummary("Xóa một hình ảnh minh họa khỏi công thức nấu ăn.");

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
