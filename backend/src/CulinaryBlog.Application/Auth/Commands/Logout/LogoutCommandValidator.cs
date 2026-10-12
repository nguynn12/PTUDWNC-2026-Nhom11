using FluentValidation;

namespace CulinaryBlog.Application.Auth.Commands.Logout;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(v => v.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
