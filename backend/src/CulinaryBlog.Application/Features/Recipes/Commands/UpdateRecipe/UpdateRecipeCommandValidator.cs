namespace CulinaryBlog.Application.Features.Recipes.Commands.UpdateRecipe;

using FluentValidation;

/// <summary>
/// Validator cho UpdateRecipeCommand.
/// </summary>
public sealed class UpdateRecipeCommandValidator : AbstractValidator<UpdateRecipeCommand>
{
    public UpdateRecipeCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Mã công thức không được để trống.");

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
