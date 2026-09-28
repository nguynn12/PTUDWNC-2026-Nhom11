using MediatR;

namespace CulinaryBlog.Application.Features.Categories.Commands.DeleteCategory;

/// <summary>
/// Command yêu cầu xóa mềm danh mục món ăn (FR-CAT-005).
/// </summary>
/// <param name="Id">Mã định danh duy nhất của danh mục cần xóa.</param>
public record DeleteCategoryCommand(Guid Id) : IRequest<bool>;
