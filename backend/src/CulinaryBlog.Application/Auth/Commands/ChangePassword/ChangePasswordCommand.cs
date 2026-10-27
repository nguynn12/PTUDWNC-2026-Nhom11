using FluentValidation;
using MediatR;
using CulinaryBlog.Application.Auth.Dtos;

namespace CulinaryBlog.Application.Auth.Commands.ChangePassword;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword) : IRequest<AuthResponseDto>;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(v => v.CurrentPassword)
            .NotEmpty().WithMessage("Mật khẩu hiện tại là bắt buộc.");

        RuleFor(v => v.NewPassword)
            .NotEmpty().WithMessage("Mật khẩu mới là bắt buộc.")
            .MinimumLength(8).WithMessage("Mật khẩu mới tối thiểu 8 ký tự.")
            .Matches("[A-Z]").WithMessage("Mật khẩu mới phải có ít nhất 1 chữ hoa.")
            .Matches("[a-z]").WithMessage("Mật khẩu mới phải có ít nhất 1 chữ thường.")
            .Matches("[0-9]").WithMessage("Mật khẩu mới phải có ít nhất 1 chữ số.")
            .Matches("[^A-Za-z0-9]").WithMessage("Mật khẩu mới phải có ít nhất 1 ký tự đặc biệt.");
    }
}
