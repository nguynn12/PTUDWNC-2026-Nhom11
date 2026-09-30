using System.Security.Cryptography;
using System.Text;
using CulinaryBlog.Application.Auth.Dtos;
using CulinaryBlog.Application.Auth.Shared;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Exceptions;
using MediatR;

namespace CulinaryBlog.Application.Auth.Commands.RefreshToken;

/// <summary>
/// SRS FR-AUTH-004: token hợp lệ khi chưa hết hạn, chưa revoked VÀ user còn active.
/// Dùng lại token đã revoke → thu hồi cả family (reuse detection).
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private const int RefreshTokenExpiryDays = 7;
    private const string ClientIp = "127.0.0.1"; // TODO: lấy IP thật từ HttpContext

    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IIdentityService _identityService;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokens,
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IIdentityService identityService)
    {
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _identityService = identityService;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Hash the incoming raw token to query DB
        var tokenHash = ComputeTokenHash(request.RefreshToken);

        var token = await _refreshTokens.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (token == null)
        {
            throw new UnauthorizedException("Refresh token không hợp lệ.", ErrorCodes.AuthTokenInvalid);
        }

        if (token.IsRevoked)
        {
            // Reuse detected: revoke the whole family
            var familyTokens = await _refreshTokens.GetNotRevokedByFamilyIdAsync(token.FamilyId, cancellationToken);

            foreach (var familyToken in familyTokens)
            {
                familyToken.Revoke("reuse-detected");
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw new RefreshTokenRevokedException();
        }

        // Domain tự bảo vệ quy tắc: token hết hạn → RefreshTokenExpiredException (401).
        token.EnsureUsable();

        // Kiểm tra user TRƯỚC khi xoay vòng token: tài khoản bị vô hiệu hoá không được cấp token mới.
        var user = await _identityService.GetUserDetailsByIdAsync(token.UserId);
        if (user == null)
        {
            throw new UnauthorizedException("Tài khoản không còn tồn tại.", ErrorCodes.AuthTokenInvalid);
        }

        if (!user.IsActive)
        {
            await RefreshTokenRevoker.RevokeAllAsync(_refreshTokens, user.Id, "account-disabled", cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw new ForbiddenException("Tài khoản đã bị quản trị viên vô hiệu hoá.", ErrorCodes.AuthAccountDisabled);
        }

        // Token is valid, rotate it
        var (newTokenHash, newRawToken) = _jwtService.GenerateRefreshToken();
        var newToken = token.Rotate(newTokenHash, RefreshTokenExpiryDays, ClientIp);
        _refreshTokens.Add(newToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email, user.Roles, user.EmailConfirmed);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRawToken,
            ExpiresIn = _jwtService.AccessTokenLifetimeSeconds,
            User = user.ToUserDto(),
        };
    }

    private static string ComputeTokenHash(string rawToken)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
