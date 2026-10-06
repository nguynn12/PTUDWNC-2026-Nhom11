using CulinaryBlog.Application.Auth.Shared;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Auth.Commands.ResetPassword;

/// <summary>
/// Đặt lại mật khẩu bằng token gửi qua email. Sau khi đổi thành công phải thu hồi mọi refresh
/// token cũ để các phiên đăng nhập trước đó (có thể của kẻ gian) bị đăng xuất.
/// </summary>
public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordCommandHandler(
        IIdentityService identityService,
        IRefreshTokenRepository refreshTokens,
        IUnitOfWork unitOfWork)
    {
        _identityService = identityService;
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
        if (!result)
        {
            throw new BusinessRuleValidationException("Token đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.", "AUTH_RESET_TOKEN_INVALID");
        }

        var user = await _identityService.GetUserDetailsByEmailAsync(request.Email);
        if (user != null)
        {
            await RefreshTokenRevoker.RevokeAllAsync(_refreshTokens, user.Id, "password-reset", cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
