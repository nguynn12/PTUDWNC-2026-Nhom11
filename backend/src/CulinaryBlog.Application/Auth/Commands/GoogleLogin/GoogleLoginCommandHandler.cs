using CulinaryBlog.Application.Auth.Dtos;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using MediatR;

namespace CulinaryBlog.Application.Auth.Commands.GoogleLogin;

/// <summary>
/// SRS FR-AUTH-003:
/// - Google token sai/hết hạn → 401 <c>AUTH_GOOGLE_TOKEN_INVALID</c>.
/// - Đã liên kết Google → đăng nhập user đó; email đã tồn tại và Google báo verified → liên kết;
///   chưa có tài khoản → tạo user mới role Author (tên/ảnh lấy từ claim).
/// - Email đã tồn tại nhưng Google chưa xác minh → 409 <c>AUTH_EMAIL_EXISTS</c> (không tự liên kết).
/// - Tài khoản bị Admin vô hiệu hoá → 403 <c>AUTH_ACCOUNT_DISABLED</c>.
/// Thành công trả 200 với cặp token giống đăng nhập bằng mật khẩu.
/// </summary>
public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, AuthResponseDto>
{
    public const string InvalidTokenMessage = "Google ID token không hợp lệ hoặc đã hết hạn.";
    public const string EmailNotVerifiedMessage =
        "Email này đã được đăng ký. Vui lòng đăng nhập bằng mật khẩu (tài khoản Google chưa xác minh email).";
    public const string AccountDisabledMessage = "Tài khoản đã bị quản trị viên vô hiệu hoá.";

    private const int RefreshTokenExpiryDays = 7;

    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly IIdentityService _identityService;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClientInfoService _clientInfo;

    public GoogleLoginCommandHandler(
        IGoogleTokenValidator googleTokenValidator,
        IIdentityService identityService,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokens,
        IUnitOfWork unitOfWork,
        IClientInfoService clientInfo)
    {
        _googleTokenValidator = googleTokenValidator;
        _identityService = identityService;
        _jwtService = jwtService;
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
        _clientInfo = clientInfo;
    }

    public async Task<AuthResponseDto> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var googleUser = await _googleTokenValidator.ValidateAsync(request.IdToken, request.Nonce, cancellationToken)
            ?? throw new UnauthorizedException(InvalidTokenMessage, ErrorCodes.AuthGoogleTokenInvalid);

        var result = await _identityService.FindOrCreateGoogleUserAsync(googleUser);

        if (result.Status == GoogleLoginStatus.EmailNotVerified || result.User is not { } user)
        {
            throw new ConflictException(EmailNotVerifiedMessage, ErrorCodes.AuthEmailExists);
        }

        if (!user.IsActive)
        {
            throw new ForbiddenException(AccountDisabledMessage, ErrorCodes.AuthAccountDisabled);
        }

        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email, user.Roles, user.EmailConfirmed);
        var (tokenHash, rawToken) = _jwtService.GenerateRefreshToken();

        _refreshTokens.Add(
            CulinaryBlog.Domain.Entities.RefreshToken.CreateNewFamily(user.Id, tokenHash, RefreshTokenExpiryDays, _clientInfo.IpAddress));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = rawToken,
            ExpiresIn = _jwtService.AccessTokenLifetimeSeconds,
            User = user.ToUserDto(),
        };
    }
}
