using System.Security.Cryptography;
using System.Text;
using CulinaryBlog.Domain.Repositories;
using MediatR;

namespace CulinaryBlog.Application.Auth.Commands.Logout;

/// <summary>SRS FR-AUTH-005 + RESOLVED-CONFLICTS D3: luôn thành công (204), kể cả token không tồn tại/đã thu hồi.</summary>
public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = ComputeTokenHash(request.RefreshToken);
        var token = await _unitOfWork.RefreshTokens.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (token != null && !token.IsRevoked)
        {
            token.Revoke("logout");
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private static string ComputeTokenHash(string rawToken)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
