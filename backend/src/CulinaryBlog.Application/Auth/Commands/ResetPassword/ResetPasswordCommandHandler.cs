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
    private readonly IApplicationDbContext _context;

    public ResetPasswordCommandHandler(IIdentityService identityService, IApplicationDbContext context)
    {
        _identityService = identityService;
        _context = context;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
        if (!result)
        {
            throw new BusinessRuleException("AUTH_RESET_TOKEN_INVALID", "Token đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");
        }

        var user = await _identityService.GetUserDetailsByEmailAsync(request.Email);
        if (user != null)
        {
            await RefreshTokenRevoker.RevokeAllActiveAsync(_context, user.Id, "password-reset", cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
