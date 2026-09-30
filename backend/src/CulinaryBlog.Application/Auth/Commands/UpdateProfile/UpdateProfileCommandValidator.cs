using FluentValidation;

namespace CulinaryBlog.Application.Auth.Commands.UpdateProfile;

/// <summary>SRS FR-AUTH-007: displayName 2-100 ký tự; avatarUrl là URL hợp lệ; bio tối đa 1000 ký tự.</summary>
public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public const int AvatarUrlMaxLength = 500; // khớp cột AvatarUrl varchar(500)

    public UpdateProfileCommandValidator()
    {
        RuleFor(v => v.DisplayName)
            .Must(name => name!.Trim().Length is >= 2 and <= 100)
            .WithMessage("Tên hiển thị phải từ 2 đến 100 ký tự.")
            .When(v => v.DisplayName is not null);

        RuleFor(v => v.AvatarUrl)
            .MaximumLength(AvatarUrlMaxLength).WithMessage($"URL ảnh đại diện tối đa {AvatarUrlMaxLength} ký tự.")
            .Must(BeHttpUrl).WithMessage("URL ảnh đại diện không hợp lệ (phải bắt đầu bằng http:// hoặc https://).")
            .When(v => !string.IsNullOrWhiteSpace(v.AvatarUrl));

        RuleFor(v => v.Bio)
            .MaximumLength(1000).WithMessage("Giới thiệu tối đa 1000 ký tự.");
    }

    private static bool BeHttpUrl(string? value) =>
        Uri.TryCreate(value?.Trim(), UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
