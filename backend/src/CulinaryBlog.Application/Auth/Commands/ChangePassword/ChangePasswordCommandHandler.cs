using CulinaryBlog.Application.Auth.Dtos;
using CulinaryBlog.Application.Auth.Shared;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Auth.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, AuthResponseDto>
{
    private const int RefreshTokenExpiryDays = 7;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClientInfoService _clientInfo;

    public ChangePasswordCommandHandler(
        ICurrentUserService currentUser,
        IIdentityService identityService,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokens,
        IUnitOfWork unitOfWork,
        IClientInfoService clientInfo)
    {
        _currentUser = currentUser;
        _identityService = identityService;
        _jwtService = jwtService;
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
        _clientInfo = clientInfo;
    }

    public async Task<AuthResponseDto> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedException("Bạn chưa đăng nhập.", ErrorCodes.AuthTokenInvalid);
        }

        var user = await _identityService.GetUserDetailsByIdAsync(userId)
            ?? throw new UnauthorizedException("Tài khoản không còn tồn tại.", ErrorCodes.AuthTokenInvalid);

        if (!user.IsActive)
        {
            throw new ForbiddenException("Tài khoản đã bị quản trị viên vô hiệu hoá.", ErrorCodes.AuthAccountDisabled);
        }

        if (!await _identityService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword))
        {
            throw new UnauthorizedException(
                "Mật khẩu hiện tại không đúng.",
                ErrorCodes.AuthInvalidCredentials);
        }

        await RefreshTokenRevoker.RevokeAllAsync(
            _refreshTokens, userId, "password-change", cancellationToken);

        var (tokenHash, rawToken) = _jwtService.GenerateRefreshToken();
        _refreshTokens.Add(
            Domain.Entities.RefreshToken.CreateNewFamily(
                user.Id, tokenHash, RefreshTokenExpiryDays, _clientInfo.IpAddress));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtService.GenerateAccessToken(
            user.Id, user.Email, user.Roles, user.EmailConfirmed);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = rawToken,
            ExpiresIn = _jwtService.AccessTokenLifetimeSeconds,
            User = user.ToUserDto(),
        };
    }
}
