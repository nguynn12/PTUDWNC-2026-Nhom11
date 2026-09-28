using CulinaryBlog.Application.Features.Categories.DTOs;
using MediatR;

namespace CulinaryBlog.Application.Features.Categories.Commands.CreateCategory;

/// <summary>
/// Command yêu cầu tạo mới một danh mục món ăn (FR-CAT-003).
/// </summary>
public record CreateCategoryCommand(
    string Name,
    string? Description = null,
    string? ImageUrl = null,
    int OrderIndex = 0
) : IRequest<CategoryDto>;
