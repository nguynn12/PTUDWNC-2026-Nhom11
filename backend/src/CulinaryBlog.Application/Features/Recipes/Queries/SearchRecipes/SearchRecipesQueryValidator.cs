namespace CulinaryBlog.Application.Features.Recipes.Queries.SearchRecipes;

using System;
using System.Linq;
using FluentValidation;

/// <summary>
/// Validator kiểm tra tính hợp lệ của tham số tìm kiếm công thức theo FR-SRCH-001/002/003/004.
/// </summary>
public class SearchRecipesQueryValidator : AbstractValidator<SearchRecipesQuery>
{
    private static readonly string[] AllowedSortBy =
    {
        "relevance",
        "publishedat",
        "newest",
        "createdat",
        "title",
        "preptime",
        "cooktime",
        "servings",
        "totaltime",
        "quickest",
        "calories"
    };

    private static readonly string[] AllowedSortOrders = { "asc", "desc" };

    public SearchRecipesQueryValidator()
    {
        // 1. Kiểm tra từ khóa tìm kiếm (FR-SRCH-001: độ dài 2-100 ký tự)
        RuleFor(x => x.Q)
            .NotEmpty().WithMessage("Từ khóa tìm kiếm không được để trống.")
            .MinimumLength(2).WithMessage("Từ khóa tìm kiếm phải có độ dài tối thiểu từ 2 ký tự.")
            .MaximumLength(100).WithMessage("Từ khóa tìm kiếm không được vượt quá 100 ký tự.");

        // 2. Kiểm tra phân trang (FR-SRCH-004)
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Số trang (page) phải lớn hơn hoặc bằng 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50).WithMessage("Kích thước trang (pageSize) phải nằm trong khoảng từ 1 đến 50.");

        // 3. Kiểm tra tiêu chí sắp xếp (FR-SRCH-003)
        RuleFor(x => x.SortBy)
            .Must(sortBy => string.IsNullOrWhiteSpace(sortBy) || AllowedSortBy.Contains(sortBy.Trim().ToLowerInvariant()))
            .WithMessage($"Tiêu chí sắp xếp không hợp lệ. Danh sách hỗ trợ: {string.Join(", ", AllowedSortBy)}");

        RuleFor(x => x.SortOrder)
            .Must(sortOrder => string.IsNullOrWhiteSpace(sortOrder) || AllowedSortOrders.Contains(sortOrder.Trim().ToLowerInvariant()))
            .WithMessage("Thứ tự sắp xếp chỉ chấp nhận 'asc' hoặc 'desc'.");

        // 4. Kiểm tra các bộ lọc thời gian và calo
        RuleFor(x => x.MaxPrepTime)
            .GreaterThanOrEqualTo(0).When(x => x.MaxPrepTime.HasValue)
            .WithMessage("Thời gian chuẩn bị tối đa phải lớn hơn hoặc bằng 0.");

        RuleFor(x => x.MaxCookTime)
            .GreaterThanOrEqualTo(0).When(x => x.MaxCookTime.HasValue)
            .WithMessage("Thời gian nấu tối đa phải lớn hơn hoặc bằng 0.");

        RuleFor(x => x.MaxTotalTime)
            .GreaterThanOrEqualTo(0).When(x => x.MaxTotalTime.HasValue)
            .WithMessage("Tổng thời gian tối đa phải lớn hơn hoặc bằng 0.");

        RuleFor(x => x.MinCalories)
            .GreaterThanOrEqualTo(0).When(x => x.MinCalories.HasValue)
            .WithMessage("Lượng calo tối thiểu phải lớn hơn hoặc bằng 0.");

        RuleFor(x => x.MaxCalories)
            .GreaterThanOrEqualTo(0).When(x => x.MaxCalories.HasValue)
            .WithMessage("Lượng calo tối đa phải lớn hơn hoặc bằng 0.");

        RuleFor(x => x)
            .Must(x => !x.MinCalories.HasValue || !x.MaxCalories.HasValue || x.MinCalories.Value <= x.MaxCalories.Value)
            .WithMessage("Lượng calo tối thiểu không được lớn hơn lượng calo tối đa.")
            .WithName("CaloriesRange");
    }
}
