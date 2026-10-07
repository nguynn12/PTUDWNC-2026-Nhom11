using System.Security.Cryptography;
using System.Text;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Auth.Commands.Logout;

/// <summary>
/// SRS FR-AUTH-005 + RESOLVED-CONFLICTS D3: luôn thành công (204), kể cả token không tồn tại/đã thu hồi.
/// Logout một thiết bị thu hồi toàn bộ token family của phiên đó (các phiên/thiết bị khác giữ nguyên).
/// </summary>
public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(IRefreshTokenRepository refreshTokens, IUnitOfWork unitOfWork)
    {
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = ComputeTokenHash(request.RefreshToken);
        var token = await _refreshTokens.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (token == null)
        {
            return;
        }

        var familyTokens = await _refreshTokens.GetNotRevokedByFamilyIdAsync(token.FamilyId, cancellationToken);
        if (familyTokens.Count == 0)
        {
            return;
        }

        foreach (var familyToken in familyTokens)
        {
            familyToken.Revoke("logout");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string ComputeTokenHash(string rawToken)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
