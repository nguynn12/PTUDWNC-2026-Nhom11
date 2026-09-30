using CulinaryBlog.Application.Auth.Dtos;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Auth.Commands.GoogleLogin;

/// <summary>
/// SRS FR-AUTH-003: <c>POST /api/v1/auth/google</c> với <c>idToken</c> lấy từ Google Identity
/// Services ở frontend. <c>nonce</c> tuỳ chọn — nếu frontend có truyền nonce khi khởi tạo GIS thì
/// gửi kèm để backend đối chiếu.
/// </summary>
public record GoogleLoginCommand(string IdToken, string? Nonce = null) : IRequest<AuthResponseDto>;

public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(v => v.IdToken)
            .NotEmpty().WithMessage("idToken là bắt buộc.");
    }
}
