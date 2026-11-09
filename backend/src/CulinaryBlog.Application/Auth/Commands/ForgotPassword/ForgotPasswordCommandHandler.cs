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
        var token = await _identityService.GeneratePasswordResetTokenAsync(request.Email);
        
        if (token != null && await _identityService.GetUserDetailsByEmailAsync(request.Email) is not null)
        {
            await _emailSender.SendPasswordResetAsync(request.Email, token, cancellationToken);
        }
    }
}
