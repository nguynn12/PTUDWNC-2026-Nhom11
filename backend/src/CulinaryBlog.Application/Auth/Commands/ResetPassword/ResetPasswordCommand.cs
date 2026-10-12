using MediatR;
using FluentValidation;

namespace CulinaryBlog.Application.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is invalid.");
            
        RuleFor(v => v.Token)
            .NotEmpty().WithMessage("Token is required.");
            
        RuleFor(v => v.NewPassword)
            .NotEmpty().WithMessage("New Password is required.");
    }
}
