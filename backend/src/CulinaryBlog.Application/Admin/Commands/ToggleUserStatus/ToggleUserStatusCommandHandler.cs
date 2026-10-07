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
    private readonly ICurrentUserService _currentUser;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;

    public ToggleUserStatusCommandHandler(
        IIdentityService identityService,
        ICurrentUserService currentUser,
        IRefreshTokenRepository refreshTokens,
        IUnitOfWork unitOfWork)
    {
        _identityService = identityService;
        _currentUser = currentUser;
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == request.UserId && !request.IsActive)
        {
            throw new BusinessRuleValidationException("Admin không thể tự vô hiệu hoá tài khoản của chính mình.", ErrorCodes.ValidationError);
        }

        var result = await _identityService.ToggleUserStatusAsync(request.UserId, request.IsActive);
        if (!result)
        {
            throw new NotFoundException("Không tìm thấy người dùng.", "USER_NOT_FOUND");
        }

        if (!request.IsActive)
        {
            await RefreshTokenRevoker.RevokeAllAsync(_refreshTokens, request.UserId, "admin-deactivated", cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
