using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Auth.Commands.ConfirmEmail;

/// <summary>
/// SRS FR-AUTH-008: token hợp lệ → xác nhận email, endpoint trả 204; thao tác idempotent.
/// Token sai/hết hạn hoặc userId không tồn tại → 422.
/// </summary>
public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand>
{
    public const string InvalidTokenMessage = "Liên kết xác nhận email không hợp lệ hoặc đã hết hạn.";

    private readonly IIdentityService _identityService;

    public ConfirmEmailCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var confirmed = await _identityService.ConfirmEmailAsync(request.UserId, request.Token);

        if (!confirmed)
        {
            throw new BusinessRuleValidationException(InvalidTokenMessage, ErrorCodes.ValidationError);
        }
    }
}
