using FluentValidation;

namespace CulinaryBlog.Application.Auth.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(v => v.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(100).WithMessage("Display name must not exceed 100 characters.");
            
        RuleFor(v => v.Bio)
            .MaximumLength(500).WithMessage("Bio must not exceed 500 characters.");

        RuleFor(v => v.AvatarUrl)
            .MaximumLength(500).WithMessage("Avatar URL must not exceed 500 characters.");
    }
}
