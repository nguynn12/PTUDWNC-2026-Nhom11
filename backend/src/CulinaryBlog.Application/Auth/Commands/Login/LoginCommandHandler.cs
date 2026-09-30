using CulinaryBlog.Application.Auth.Dtos;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using MediatR;

namespace CulinaryBlog.Application.Auth.Commands.Login;

/// <summary>
/// SRS FR-AUTH-002: 401 chung cho sai email/mật khẩu, 423 khi bị khoá tạm (5 lần sai → 15 phút),
/// 403 khi tài khoản bị Admin vô hiệu hoá (FR-AUTH-010).
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    public const string InvalidCredentialsMessage = "Email hoặc mật khẩu không đúng.";
    public const string AccountDisabledMessage = "Tài khoản đã bị quản trị viên vô hiệu hoá.";

    private const int RefreshTokenExpiryDays = 7;
    private const string ClientIp = "127.0.0.1"; // TODO: lấy IP thật qua ICurrentUser

    private readonly IIdentityService _identityService;
    private readonly IJwtService _jwtService;
    private readonly IApplicationDbContext _context;

    public LoginCommandHandler(IIdentityService identityService, IJwtService jwtService, IApplicationDbContext context)
    {
        _identityService = identityService;
        _jwtService = jwtService;
        _context = context;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var check = await _identityService.CheckCredentialsAsync(request.Email, request.Password);

        if (check.Status == CredentialCheckStatus.LockedOut)
        {
            throw new LockedException(ErrorCodes.AuthAccountLocked, BuildLockedMessage(check.LockoutEnd));
        }

        if (check.Status == CredentialCheckStatus.Disabled)
        {
            throw new ForbiddenException(ErrorCodes.AuthAccountDisabled, AccountDisabledMessage);
        }

        if (check.Status != CredentialCheckStatus.Success || check.User is not { } user)
        {
            throw new UnauthorizedException(ErrorCodes.AuthInvalidCredentials, InvalidCredentialsMessage);
        }

        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email, user.Roles, user.EmailConfirmed);
        var (tokenHash, rawToken) = _jwtService.GenerateRefreshToken();

        _context.RefreshTokens.Add(CulinaryBlog.Domain.Entities.RefreshToken.CreateNewFamily(
            user.Id, tokenHash, RefreshTokenExpiryDays, ClientIp));
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = rawToken,
            ExpiresIn = _jwtService.AccessTokenLifetimeSeconds,
            User = user.ToUserDto(),
        };
    }

    public static string BuildLockedMessage(DateTimeOffset? lockoutEnd)
    {
        const string prefix = "Tài khoản đang bị tạm khoá do đăng nhập sai nhiều lần.";
        if (lockoutEnd is null)
        {
            return prefix;
        }

        var minutes = Math.Max(1, (int)Math.Ceiling((lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes));
        return $"{prefix} Vui lòng thử lại sau {minutes} phút.";
    }
}
