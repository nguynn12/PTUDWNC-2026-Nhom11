using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Auth.Commands.ResendConfirmationEmail;

/// <summary>
/// SRS FR-AUTH-009: luôn trả 202 với message chung (<see cref="GenericMessage"/>) để tránh user
/// enumeration — handler KHÔNG ném lỗi khi email không tồn tại hoặc đã xác nhận, chỉ bỏ qua.
/// </summary>
public class ResendConfirmationEmailCommandHandler : IRequestHandler<ResendConfirmationEmailCommand>
{
    public const string GenericMessage =
        "Nếu email đã đăng ký và chưa được xác nhận, hệ thống sẽ gửi lại email xác nhận.";

    private readonly IIdentityService _identityService;
    private readonly IAccountEmailSender _accountEmailSender;

    public ResendConfirmationEmailCommandHandler(IIdentityService identityService, IAccountEmailSender accountEmailSender)
    {
        _identityService = identityService;
        _accountEmailSender = accountEmailSender;
    }

    public async Task Handle(ResendConfirmationEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.GetUserDetailsByEmailAsync(request.Email.Trim());
        if (user is null || user.EmailConfirmed || !user.IsActive)
        {
            return;
        }

        var token = await _identityService.GenerateEmailConfirmationTokenAsync(user.Id);
        if (token is null)
        {
            return;
        }

        await _accountEmailSender.SendEmailConfirmationAsync(
            user.Id, user.Email, user.DisplayName, token, cancellationToken);
    }
}
