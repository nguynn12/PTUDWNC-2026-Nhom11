using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Auth.Commands.ResendConfirmationEmail;

public class ResendConfirmationEmailCommandHandler : IRequestHandler<ResendConfirmationEmailCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;

    public ResendConfirmationEmailCommandHandler(IIdentityService identityService, IEmailService emailService)
    {
        _identityService = identityService;
        _emailService = emailService;
    }

    public async Task Handle(ResendConfirmationEmailCommand request, CancellationToken cancellationToken)
    {
        var token = await _identityService.GenerateEmailConfirmationTokenAsync(request.Email);
        
        if (token != null)
        {
            await _emailService.SendEmailAsync(
                request.Email, 
                "Confirm Email", 
                $"Your email confirmation token is: {token}"
            );
        }
    }
}
