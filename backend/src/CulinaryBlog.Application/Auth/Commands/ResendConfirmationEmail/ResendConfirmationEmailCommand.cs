using MediatR;
using FluentValidation;

namespace CulinaryBlog.Application.Auth.Commands.ResendConfirmationEmail;

/// <summary>SRS FR-AUTH-009: <c>POST /api/v1/auth/email/resend</c> với <c>email</c>.</summary>
public record ResendConfirmationEmailCommand(string Email) : IRequest;

public class ResendConfirmationEmailCommandValidator : AbstractValidator<ResendConfirmationEmailCommand>
{
    public ResendConfirmationEmailCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email là bắt buộc.")
            .EmailAddress().WithMessage("Email không đúng định dạng.");
    }
}
