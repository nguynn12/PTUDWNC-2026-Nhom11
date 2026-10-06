using CulinaryBlog.Application.Features.Categories.DTOs;
using MediatR;

namespace CulinaryBlog.Application.Features.Categories.Commands.UpdateCategory;

/// <summary>
/// Command yêu cầu cập nhật thông tin danh mục món ăn (FR-CAT-004).
/// Lưu ý: Thuộc tính Slug được giữ nguyên để bảo toàn SEO.
/// </summary>
public record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Description = null,
    string? ImageUrl = null,
    int OrderIndex = 0
) : IRequest<CategoryDto>;
