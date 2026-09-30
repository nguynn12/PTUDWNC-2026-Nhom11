using MediatR;
using FluentValidation;

namespace CulinaryBlog.Application.Auth.Commands.ResendConfirmationEmail;

public record ResendConfirmationEmailCommand(string Email) : IRequest;

public class ResendConfirmationEmailCommandValidator : AbstractValidator<ResendConfirmationEmailCommand>
{
    public ResendConfirmationEmailCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is invalid.");
    }
}
