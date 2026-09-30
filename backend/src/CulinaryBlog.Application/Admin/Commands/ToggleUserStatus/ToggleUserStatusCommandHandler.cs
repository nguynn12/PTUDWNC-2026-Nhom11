using CulinaryBlog.Application.Auth.Shared;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Admin.Commands.ToggleUserStatus;

/// <summary>
/// SRS FR-AUTH-010: Admin đổi trạng thái user; khi vô hiệu hoá phải thu hồi TOÀN BỘ refresh token
/// để user bị đăng xuất khỏi mọi phiên (access token còn lại tự hết hạn sau tối đa 15 phút).
/// </summary>
public class ToggleUserStatusCommandHandler : IRequestHandler<ToggleUserStatusCommand>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUser _currentUser;
    private readonly IApplicationDbContext _context;

    public ToggleUserStatusCommandHandler(IIdentityService identityService, ICurrentUser currentUser, IApplicationDbContext context)
    {
        _identityService = identityService;
        _currentUser = currentUser;
        _context = context;
    }

    public async Task Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Id == request.UserId && !request.IsActive)
        {
            throw new BusinessRuleException("AUTH_CANNOT_DEACTIVATE_SELF", "Admin không thể tự vô hiệu hoá tài khoản của chính mình.");
        }

        var result = await _identityService.ToggleUserStatusAsync(request.UserId, request.IsActive);
        if (!result)
        {
            throw new NotFoundException("USER_NOT_FOUND", "Không tìm thấy người dùng.");
        }

        if (!request.IsActive)
        {
            await RefreshTokenRevoker.RevokeAllActiveAsync(_context, request.UserId, "admin-deactivated", cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
