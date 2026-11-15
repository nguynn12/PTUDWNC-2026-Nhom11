using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IAccountEmailSender _emailSender;

    public ForgotPasswordCommandHandler(IIdentityService identityService, IAccountEmailSender emailSender)
    {
        _identityService = identityService;
        _emailSender = emailSender;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // OPS-08: kiểm tra user trước, chỉ sinh token và xếp email khi tài khoản tồn tại; endpoint luôn trả 204
        // nên phản hồi giống hệt nhau dù email có tồn tại hay không (chống dò tài khoản).
        if (await _identityService.GetUserDetailsByEmailAsync(request.Email) is null)
        {
            return;
        }

        var token = await _identityService.GeneratePasswordResetTokenAsync(request.Email);
        if (token != null)
        {
            await _emailSender.SendPasswordResetAsync(request.Email, token, cancellationToken);
        }
    }
}
