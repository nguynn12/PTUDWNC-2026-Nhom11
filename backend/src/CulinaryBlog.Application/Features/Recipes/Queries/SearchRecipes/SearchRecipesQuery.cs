namespace CulinaryBlog.Application.Features.Recipes.Queries.SearchRecipes;

using System;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes.DTOs;
using CulinaryBlog.Domain.Enums;
using MediatR;

/// <summary>
/// Query tìm kiếm toàn văn công thức nấu ăn (Full-Text Search) theo FR-SRCH-001/002/003/004.
/// </summary>
/// <param name="Q">Từ khóa tìm kiếm (bắt buộc, dài từ 2 đến 100 ký tự).</param>
/// <param name="Page">Trang hiện tại (mặc định: 1).</param>
/// <param name="PageSize">Số lượng phần tử mỗi trang (mặc định: 12, tối đa: 50).</param>
/// <param name="CategoryId">Lọc theo danh mục (tùy chọn).</param>
/// <param name="Difficulty">Lọc theo độ khó (tùy chọn: Easy, Medium, Hard, Expert).</param>
/// <param name="MaxPrepTime">Thời gian chuẩn bị tối đa tính bằng phút (tùy chọn).</param>
/// <param name="MaxCookTime">Thời gian nấu tối đa tính bằng phút (tùy chọn).</param>
/// <param name="MaxTotalTime">Tổng thời gian tối đa (Prep + Cook) tính bằng phút (tùy chọn).</param>
/// <param name="MinCalories">Lượng calo tối thiểu (tùy chọn).</param>
/// <param name="MaxCalories">Lượng calo tối đa (tùy chọn).</param>
/// <param name="SortBy">Tiêu chí sắp xếp: relevance (mặc định), publishedAt, createdAt, title, prepTime, cookTime, servings, totalTime, calories.</param>
/// <param name="SortOrder">Thứ tự sắp xếp: desc (mặc định cho relevance) hoặc asc.</param>
public record SearchRecipesQuery(
    string Q = "",
    int Page = 1,
    int PageSize = 12,
    Guid? CategoryId = null,
    RecipeDifficulty? Difficulty = null,
    int? MaxPrepTime = null,
    int? MaxCookTime = null,
    int? MaxTotalTime = null,
    decimal? MinCalories = null,
    decimal? MaxCalories = null,
    string? SortBy = "relevance",
    string? SortOrder = "desc"
) : IRequest<PagedResult<RecipeSummaryDto>>;
