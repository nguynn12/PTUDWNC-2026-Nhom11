namespace CulinaryBlog.Application.Features.Recipes.Queries.GetRecipes;

using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes.DTOs;
using MediatR;

/// <summary>
/// Query lấy danh sách công thức nấu ăn đã xuất bản (Published), có phân trang và hỗ trợ lọc theo CategoryId.
/// Tuân thủ đặc tả FR-RCP-001.
/// </summary>
/// <param name="Page">Chỉ số trang hiện tại (mặc định 1, >= 1).</param>
/// <param name="PageSize">Số lượng bài trên mỗi trang (mặc định 12, tối đa 50).</param>
/// <param name="CategoryId">Mã danh mục để lọc (tùy chọn).</param>
public record GetRecipesQuery(
    int Page = 1,
    int PageSize = 12,
    Guid? CategoryId = null
) : IRequest<PagedResult<RecipeSummaryDto>>;
