using System.Security.Cryptography;
using System.Text;
using CulinaryBlog.Application.Auth.Dtos;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
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
            throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Invalid refresh token.");
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
            
            throw new UnauthorizedException(ErrorCodes.AuthRefreshTokenRevoked, "Refresh token has been revoked.");
        }

        if (token.IsExpired)
        {
            throw new UnauthorizedException(ErrorCodes.AuthRefreshTokenExpired, "Refresh token has expired.");
        }

        // Token is valid, rotate it
        var (newTokenHash, newRawToken) = _jwtService.GenerateRefreshToken();
        var newToken = CulinaryBlog.Domain.Entities.RefreshToken.CreateRotated(
            token.UserId, 
            newTokenHash, 
            token.FamilyId, 
            7, 
            "127.0.0.1"); // TODO: Use IHttpContextAccessor for IP

        token.Revoke("rotated", null, newTokenHash);

        _context.RefreshTokens.Add(newToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Fetch user info for new access token
        var userDetails = await _identityService.GetUserDetailsByIdAsync(token.UserId);
        if (userDetails == null)
        {
             throw new UnauthorizedException(ErrorCodes.AuthInvalidCredentials, "User not found.");
        }
        
        var accessToken = _jwtService.GenerateAccessToken(
            userDetails.Value.Id, 
            userDetails.Value.Email, 
            userDetails.Value.Roles, 
            userDetails.Value.EmailConfirmed);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRawToken,
            ExpiresIn = 900,
            User = new UserDto
            {
                Id = userDetails.Value.Id,
                Email = userDetails.Value.Email,
                DisplayName = userDetails.Value.DisplayName,
                Roles = userDetails.Value.Roles,
                EmailConfirmed = userDetails.Value.EmailConfirmed,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };
    }

    private string ComputeTokenHash(string rawToken)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
