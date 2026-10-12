using MediatR;
using FluentValidation;

namespace CulinaryBlog.Application.Auth.Commands.ConfirmEmail;

/// <summary>SRS FR-AUTH-008: <c>POST /api/v1/auth/email/confirm</c> với <c>userId</c>, <c>token</c>.</summary>
public record ConfirmEmailCommand(string UserId, string Token) : IRequest;

public class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("UserId là bắt buộc.");

        RuleFor(v => v.Token)
            .NotEmpty().WithMessage("Token là bắt buộc.");
    }
}
