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
            .NotEmpty().WithMessage("New Password is required.")
            .MinimumLength(8).WithMessage("New Password must be at least 8 characters long.")
            .Matches("[A-Z]").WithMessage("New Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("New Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("New Password must contain a digit.")
            .Matches("[^A-Za-z0-9]").WithMessage("New Password must contain a special character.");
    }
}
