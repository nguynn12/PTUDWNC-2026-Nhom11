namespace CulinaryBlog.Application.Features.Recipes.Queries.GetRecipes;

using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes.DTOs;
using CulinaryBlog.Domain.Enums;
using MediatR;

/// <summary>
/// Query lấy danh sách công thức nấu ăn đã xuất bản (Published), có phân trang, hỗ trợ lọc đa tiêu chí và sắp xếp linh hoạt.
/// Tuân thủ đặc tả FR-RCP-001 và FR-SRCH-002/003/004.
/// </summary>
/// <param name="Page">Chỉ số trang hiện tại (mặc định 1, >= 1).</param>
/// <param name="PageSize">Số lượng bài trên mỗi trang (mặc định 12, tối đa 50).</param>
/// <param name="CategoryId">Mã danh mục để lọc (tùy chọn).</param>
/// <param name="Difficulty">Độ khó của món ăn (Easy, Medium, Hard, Expert).</param>
/// <param name="MaxPrepTime">Thời gian sơ chế tối đa (phút).</param>
/// <param name="MaxCookTime">Thời gian nấu tối đa (phút).</param>
/// <param name="MaxTotalTime">Tổng thời gian chuẩn bị và nấu tối đa (phút).</param>
/// <param name="MinCalories">Lượng calo tối thiểu (kcal).</param>
/// <param name="MaxCalories">Lượng calo tối đa (kcal).</param>
/// <param name="SortBy">Trường sắp xếp (publishedAt, newest, totalTime, quickest, prepTime, cookTime, calories, title, servings).</param>
/// <param name="SortOrder">Thứ tự sắp xếp (asc hoặc desc, mặc định desc cho publishedAt).</param>
public record GetRecipesQuery(
    int Page = 1,
    int PageSize = 12,
    Guid? CategoryId = null,
    RecipeDifficulty? Difficulty = null,
    int? MaxPrepTime = null,
    int? MaxCookTime = null,
    int? MaxTotalTime = null,
    decimal? MinCalories = null,
    decimal? MaxCalories = null,
    string? SortBy = null,
    string? SortOrder = null
) : IRequest<PagedResult<RecipeSummaryDto>>;
