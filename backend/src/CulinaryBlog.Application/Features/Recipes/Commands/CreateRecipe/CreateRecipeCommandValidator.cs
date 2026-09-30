namespace CulinaryBlog.Application.Features.Recipes.Commands.CreateRecipe;

using FluentValidation;

/// <summary>
/// Validator xác thực dữ liệu đầu vào cho CreateRecipeCommand theo đặc tả SRS v1.2.0 (mục 7.2).
/// </summary>
public sealed class CreateRecipeCommandValidator : AbstractValidator<CreateRecipeCommand>
{
    public CreateRecipeCommandValidator()
    {
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Tiêu đề công thức không được để trống.")
            .MaximumLength(200).WithMessage("Tiêu đề không được vượt quá 200 ký tự.");

        RuleFor(v => v.Description)
            .NotEmpty().WithMessage("Mô tả công thức không được để trống.")
            .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự.");

        RuleFor(v => v.PrepTimeMinutes)
            .GreaterThan(0).WithMessage("Thời gian sơ chế phải lớn hơn 0 phút.");

        RuleFor(v => v.CookTimeMinutes)
            .GreaterThanOrEqualTo(0).WithMessage("Thời gian nấu nướng không được âm.");

        RuleFor(v => v.Servings)
            .GreaterThan(0).WithMessage("Khẩu phần ăn phải lớn hơn 0 người.");

        RuleFor(v => v.Difficulty)
            .IsInEnum().WithMessage("Mức độ khó không hợp lệ.");

        RuleFor(v => v.CategoryId)
            .NotEmpty().WithMessage("Danh mục món ăn không được để trống.");
    }
}
