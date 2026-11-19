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
            .MinimumLength(3).WithMessage("Tiêu đề phải có ít nhất 3 ký tự.")
            .MaximumLength(200).WithMessage("Tiêu đề không được vượt quá 200 ký tự.");

        RuleFor(v => v.Description)
            .NotEmpty().WithMessage("Mô tả công thức không được để trống.")
            .MinimumLength(10).WithMessage("Mô tả phải có ít nhất 10 ký tự.")
            .MaximumLength(2000).WithMessage("Mô tả không được vượt quá 2000 ký tự.");

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

        When(v => v.Nutrition != null, () =>
        {
            RuleFor(v => v.Nutrition!.Calories)
                .GreaterThanOrEqualTo(0).WithMessage("Lượng Calories không được âm.")
                .LessThanOrEqualTo(999999.99m).WithMessage("Lượng Calories vượt quá giới hạn cho phép.");

            RuleFor(v => v.Nutrition!.Protein)
                .GreaterThanOrEqualTo(0).WithMessage("Lượng Protein không được âm.")
                .LessThanOrEqualTo(999999.99m).WithMessage("Lượng Protein vượt quá giới hạn cho phép.");

            RuleFor(v => v.Nutrition!.Carbohydrates)
                .GreaterThanOrEqualTo(0).WithMessage("Lượng Carbohydrates không được âm.")
                .LessThanOrEqualTo(999999.99m).WithMessage("Lượng Carbohydrates vượt quá giới hạn cho phép.");

            RuleFor(v => v.Nutrition!.Fat)
                .GreaterThanOrEqualTo(0).WithMessage("Lượng Fat không được âm.")
                .LessThanOrEqualTo(999999.99m).WithMessage("Lượng Fat vượt quá giới hạn cho phép.");

            RuleFor(v => v.Nutrition!.Fiber)
                .GreaterThanOrEqualTo(0).WithMessage("Lượng Fiber không được âm.")
                .LessThanOrEqualTo(999999.99m).WithMessage("Lượng Fiber vượt quá giới hạn cho phép.");

            RuleFor(v => v.Nutrition!.Sodium)
                .GreaterThanOrEqualTo(0).WithMessage("Lượng Sodium không được âm.")
                .LessThanOrEqualTo(999999.99m).WithMessage("Lượng Sodium vượt quá giới hạn cho phép.");
        });
    }
}
