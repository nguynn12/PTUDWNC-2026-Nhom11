using FluentValidation;

namespace CulinaryBlog.Application.Admin.Commands.ToggleUserStatus;

public class ToggleUserStatusCommandValidator : AbstractValidator<ToggleUserStatusCommand>
{
    public ToggleUserStatusCommandValidator()
    {
        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
