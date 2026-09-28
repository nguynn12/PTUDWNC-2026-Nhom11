using CulinaryBlog.API.Common.Models;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Categories.Commands.CreateCategory;
using CulinaryBlog.Application.Features.Categories.Commands.DeleteCategory;
using CulinaryBlog.Application.Features.Categories.Commands.UpdateCategory;
using CulinaryBlog.Application.Features.Categories.DTOs;
using CulinaryBlog.Application.Features.Categories.Queries.GetCategories;
using CulinaryBlog.Application.Features.Categories.Queries.GetCategoryBySlug;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CulinaryBlog.API.Endpoints;

/// <summary>
/// Định nghĩa các Minimal API Endpoints cho phân hệ Quản lý Danh mục (FR-CAT-001 -> FR-CAT-005).
/// </summary>
public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapCategoryEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/categories")
            .WithTags("Categories");

        // FR-CAT-001: Lấy danh sách tất cả danh mục (Public)
        group.MapGet("", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var categories = await sender.Send(new GetCategoriesQuery(), cancellationToken);
            return Results.Ok(new ApiResponse<IEnumerable<CategoryDto>>(categories));
        })
        .WithName("GetCategories")
        .WithSummary("Xem danh sách tất cả danh mục")
        .WithDescription("Trả về danh sách danh mục kèm số lượng công thức Published theo thứ tự OrderIndex và Name (FR-CAT-001).")
        .Produces<ApiResponse<IEnumerable<CategoryDto>>>(StatusCodes.Status200OK);

        // FR-CAT-002: Lấy thông tin chi tiết danh mục theo Slug (Public)
        group.MapGet("/{slug}", async (string slug, ISender sender, CancellationToken cancellationToken) =>
        {
            var category = await sender.Send(new GetCategoryBySlugQuery(slug), cancellationToken);

            if (category is null)
            {
                throw new NotFoundException($"Không tìm thấy danh mục với đường dẫn '{slug}'.");
            }

            return Results.Ok(new ApiResponse<CategoryDetailDto>(category));
        })
        .WithName("GetCategoryBySlug")
        .WithSummary("Xem chi tiết danh mục theo Slug")
        .WithDescription("Trả về thông tin chi tiết của danh mục dựa trên slug chuẩn SEO (FR-CAT-002).")
        .Produces<ApiResponse<CategoryDetailDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // FR-CAT-003: Tạo danh mục mới (Admin)
        group.MapPost("", async (CreateCategoryRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new CreateCategoryCommand(
                request.Name,
                request.Description,
                request.ImageUrl,
                request.OrderIndex);

            var created = await sender.Send(command, cancellationToken);

            return Results.Created(
                $"/api/v1/categories/{created.Slug}",
                new ApiResponse<CategoryDto>(created));
        })
        .WithName("CreateCategory")
        .WithSummary("Tạo danh mục mới (Admin)")
        .WithDescription("Tạo danh mục mới, tự động sinh slug tiếng Việt chuẩn SEO (FR-CAT-003).")
        .Produces<ApiResponse<CategoryDto>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        // FR-CAT-004: Cập nhật thông tin danh mục (Admin)
        group.MapPut("/{id:guid}", async (Guid id, UpdateCategoryRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new UpdateCategoryCommand(
                id,
                request.Name,
                request.Description,
                request.ImageUrl,
                request.OrderIndex);

            var updated = await sender.Send(command, cancellationToken);

            return Results.Ok(new ApiResponse<CategoryDto>(updated));
        })
        .WithName("UpdateCategory")
        .WithSummary("Cập nhật danh mục (Admin)")
        .WithDescription("Cập nhật tên, mô tả, ảnh, thứ tự. Slug giữ nguyên bất biến theo SRS C7 (FR-CAT-004).")
        .Produces<ApiResponse<CategoryDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        // FR-CAT-005: Xóa mềm danh mục (Admin)
        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new DeleteCategoryCommand(id);
            await sender.Send(command, cancellationToken);

            return Results.NoContent();
        })
        .WithName("DeleteCategory")
        .WithSummary("Xóa danh mục (Admin)")
        .WithDescription("Xóa mềm danh mục nếu không còn công thức nào liên kết. Chặn xóa 409 nếu còn Recipe (FR-CAT-005).")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }
}
