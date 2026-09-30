using MediatR;
using FluentValidation;

namespace CulinaryBlog.Application.Auth.Commands.ConfirmEmail;

public record ConfirmEmailCommand(string Email, string Token) : IRequest;

public class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is invalid.");
            
        RuleFor(v => v.Token)
            .NotEmpty().WithMessage("Token is required.");
    }
}
