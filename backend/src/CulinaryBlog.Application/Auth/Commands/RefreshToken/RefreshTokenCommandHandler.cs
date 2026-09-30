using System.Security.Cryptography;
using System.Text;
using CulinaryBlog.Application.Auth.Dtos;
using CulinaryBlog.Application.Auth.Shared;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Application.Auth.Commands.RefreshToken;

/// <summary>
/// SRS FR-AUTH-004: token hợp lệ khi chưa hết hạn, chưa revoked VÀ user còn active.
/// Dùng lại token đã revoke → thu hồi cả family (reuse detection).
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private const int RefreshTokenExpiryDays = 7;
    private const string ClientIp = "127.0.0.1"; // TODO: lấy IP thật qua ICurrentUser

    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IIdentityService _identityService;

    public RefreshTokenCommandHandler(IApplicationDbContext context, IJwtService jwtService, IIdentityService identityService)
    {
        _context = context;
        _jwtService = jwtService;
        _identityService = identityService;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Hash the incoming raw token to query DB
        var tokenHash = ComputeTokenHash(request.RefreshToken);

        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (token == null)
        {
            throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Refresh token không hợp lệ.");
        }

        if (token.IsRevoked)
        {
            // Reuse detected: revoke the whole family
            var familyTokens = await _context.RefreshTokens
                .Where(rt => rt.FamilyId == token.FamilyId && rt.RevokedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var familyToken in familyTokens)
            {
                familyToken.Revoke("reuse-detected");
            }

            await _context.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException(ErrorCodes.AuthRefreshTokenRevoked, "Refresh token đã bị thu hồi.");
        }

        if (token.IsExpired)
        {
            throw new UnauthorizedException(ErrorCodes.AuthRefreshTokenExpired, "Refresh token đã hết hạn.");
        }

        // Kiểm tra user TRƯỚC khi xoay vòng token: tài khoản bị vô hiệu hoá không được cấp token mới.
        var user = await _identityService.GetUserDetailsByIdAsync(token.UserId);
        if (user == null)
        {
            throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Tài khoản không còn tồn tại.");
        }

        if (!user.IsActive)
        {
            await RefreshTokenRevoker.RevokeAllActiveAsync(_context, user.Id, "account-disabled", cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            throw new ForbiddenException(ErrorCodes.AuthAccountDisabled, "Tài khoản đã bị quản trị viên vô hiệu hoá.");
        }

        // Token is valid, rotate it
        var (newTokenHash, newRawToken) = _jwtService.GenerateRefreshToken();
        var newToken = CulinaryBlog.Domain.Entities.RefreshToken.CreateRotated(
            token.UserId,
            newTokenHash,
            token.FamilyId,
            RefreshTokenExpiryDays,
            ClientIp);

        token.Revoke("rotated", null, newTokenHash);
        _context.RefreshTokens.Add(newToken);

        await _context.SaveChangesAsync(cancellationToken);

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
