namespace CulinaryBlog.Application.Features.Recipes.Queries.GetRecipeById;

using CulinaryBlog.Application.Features.Recipes.DTOs;
using MediatR;

/// <summary>
/// Query lấy chi tiết công thức nấu ăn theo ID phục vụ màn hình chỉnh sửa (Author/Admin).
/// Hỗ trợ đọc công thức ở mọi trạng thái vòng đời (Draft, Published, Archived) của chính tác giả.
/// </summary>
public record GetRecipeByIdQuery(Guid Id) : IRequest<RecipeDetailDto>;
