namespace CulinaryBlog.Application.Features.Recipes.Queries.GetRecipes;

using System;
using System.Linq;
using FluentValidation;

/// <summary>
/// Validator kiểm tra tính hợp lệ của tham số duyệt danh sách công thức theo FR-RCP-001 và FR-SRCH-002/003/004.
/// Tự động ném ValidationException (trả về HTTP 422 Unprocessable Entity theo chuẩn RFC 7231 / C3).
/// </summary>
public class GetRecipesQueryValidator : AbstractValidator<GetRecipesQuery>
{
    private static readonly string[] AllowedSortBy =
    {
        "publishedat",
        "newest",
        "createdat",
        "updatedat",
        "title",
        "preptime",
        "cooktime",
        "servings",
        "totaltime",
        "quickest",
        "calories"
    };

    private static readonly string[] AllowedSortOrders = { "asc", "desc" };

    public GetRecipesQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Số trang (page) phải lớn hơn hoặc bằng 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50).WithMessage("Kích thước trang (pageSize) phải nằm trong khoảng từ 1 đến 50.");

        RuleFor(x => x.Difficulty)
            .IsInEnum().When(x => x.Difficulty.HasValue)
            .WithMessage("Mức độ khó không hợp lệ.");

        RuleFor(x => x.Status)
            .IsInEnum().When(x => x.Status.HasValue)
            .WithMessage("Trạng thái công thức không hợp lệ.");

        RuleFor(x => x.SortBy)
            .Must(sortBy => string.IsNullOrWhiteSpace(sortBy) || AllowedSortBy.Contains(sortBy.Trim().ToLowerInvariant()))
            .WithMessage($"Tiêu chí sắp xếp không hợp lệ. Danh sách hỗ trợ: {string.Join(", ", AllowedSortBy)}");

        RuleFor(x => x.SortOrder)
            .Must(sortOrder => string.IsNullOrWhiteSpace(sortOrder) || AllowedSortOrders.Contains(sortOrder.Trim().ToLowerInvariant()))
            .WithMessage("Thứ tự sắp xếp chỉ chấp nhận 'asc' hoặc 'desc'.");

        RuleFor(x => x.MaxPrepTime)
            .GreaterThanOrEqualTo(0).When(x => x.MaxPrepTime.HasValue)
            .WithMessage("Thời gian sơ chế tối đa phải lớn hơn hoặc bằng 0.");

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
