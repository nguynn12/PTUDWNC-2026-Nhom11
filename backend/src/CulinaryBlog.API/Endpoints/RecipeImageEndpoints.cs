namespace CulinaryBlog.API.Endpoints;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using CulinaryBlog.API.RateLimiting;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.RecipeImages.DTOs;
using CulinaryBlog.Application.Features.RecipeImages.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Đăng ký các Minimal API Endpoints quản lý hình ảnh minh họa công thức nấu ăn.
/// Tuân thủ SRS v1.2.0 (mục 8.3, FR-RCP-008), RESOLVED-CONFLICTS.md (E6, E7, C2)
/// và kế hoạch sửa lỗi SEC-01, SEC-02, API-03, API-08, API-18.
/// </summary>
public static class RecipeImageEndpoints
{
    public static RouteGroupBuilder MapRecipeImageEndpoints(this RouteGroupBuilder group)
    {
        var images = group.MapGroup("/recipes/{recipeId:guid}/images")
                          .WithTags("Recipe Images");

        // GET /api/v1/recipes/{recipeId}/images
        // SEC-02: Kiểm tra quyền xem nếu Recipe ở trạng thái Draft/Archived
        images.MapGet("/", async (
            Guid recipeId,
            IRecipeImageService imageService,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var (userId, isAdmin) = ExtractUserContext(user);
            var result = await imageService.GetImagesAsync(recipeId, userId, isAdmin, ct);
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
            if (file == null)
            {
                throw new BadRequestException("Tệp tin hình ảnh không được để trống.");
            }

            if (file.Length == 0)
            {
                throw new BusinessRuleValidationException("Tệp tin hình ảnh rỗng.", ErrorCodes.FileMimeInvalid);
            }

            // Kiểm tra giới hạn 5 MB trước khi đọc luồng vào bộ nhớ theo SRS và Lab 8
            if (file.Length > 5 * 1024 * 1024)
            {
                throw new BusinessRuleValidationException("Dung lượng tệp tin vượt quá giới hạn tối đa cho phép là 5MB.", ErrorCodes.FileSizeExceeded);
            }

            var (userId, isAdmin) = ExtractUserContext(user);
            var concurrencyToken = ExtractConcurrencyToken(httpContext);

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
                concurrencyToken,
                ct);

            return Results.Created($"/api/v1/recipes/{recipeId}/images", ApiResponse<RecipeImageDto>.Success(result));
        })
        .DisableAntiforgery()
        .RequireAuthorization()
        .RequireRateLimiting(UploadRateLimiting.PolicyName)
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
            var (userId, isAdmin) = ExtractUserContext(user);
            var concurrencyToken = ExtractConcurrencyToken(httpContext);

            var result = await imageService.UpdateImageAsync(recipeId, imageId, request, userId, isAdmin, concurrencyToken, ct);
            return Results.Ok(ApiResponse<RecipeImageDto>.Success(result));
        })
        .RequireAuthorization()
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
            var (userId, isAdmin) = ExtractUserContext(user);
            var concurrencyToken = ExtractConcurrencyToken(httpContext);

            await imageService.DeleteImageAsync(recipeId, imageId, userId, isAdmin, concurrencyToken, ct);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("DeleteRecipeImage")
        .WithSummary("Xóa một hình ảnh minh họa khỏi công thức nấu ăn.");

        return group;
    }

    /// <summary>
    /// SEC-01: Chỉ trích xuất thông tin người dùng từ JWT ClaimsPrincipal hợp lệ.
    /// Tuyệt đối KHÔNG đọc hay fallback về X-User-Role / X-User-Id từ HTTP Request Headers.
    /// </summary>
    private static (string? UserId, bool IsAdmin) ExtractUserContext(ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? user.FindFirstValue("sub")
                  ?? user.FindFirstValue("id");

        var isAdmin = user.IsInRole("Admin")
                   || user.IsInRole("Administrator");

        return (userId, isAdmin);
    }

    /// <summary>
    /// API-03: Trích xuất token phiên bản concurrency từ header If-Match.
    /// </summary>
    private static uint? ExtractConcurrencyToken(HttpContext httpContext)
    {
        var ifMatch = httpContext.Request.Headers["If-Match"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(ifMatch))
        {
            return null;
        }

        var raw = ifMatch.Trim().Trim('"');
        if (raw.StartsWith("W/", StringComparison.OrdinalIgnoreCase))
        {
            raw = raw.Substring(2).Trim('"');
        }

        if (uint.TryParse(raw, out var token))
        {
            return token;
        }

        throw new BadRequestException("Header If-Match không hợp lệ. Token phiên bản phải là số nguyên dương.");
    }
}
