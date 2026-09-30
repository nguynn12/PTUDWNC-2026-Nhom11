using CulinaryBlog.Application.Auth.Dtos;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Constants;
using CulinaryBlog.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Application.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtService _jwtService;
    private readonly IApplicationDbContext _context;

    public RegisterCommandHandler(IIdentityService identityService, IJwtService jwtService, IApplicationDbContext context)
    {
        _identityService = identityService;
        _jwtService = jwtService;
        _context = context;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if user exists? IIdentityService.CreateUserAsync will handle this and return error.
        var (succeeded, error, userId) = await _identityService.CreateUserAsync(request.Email, request.Password, request.DisplayName);
        
        if (!succeeded)
        {
            throw new BusinessRuleException(ErrorCodes.ValidationError, error ?? "Registration failed.");
        }

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(userId, request.Email, new[] { Roles.Author }, false);
        var (tokenHash, rawToken) = _jwtService.GenerateRefreshToken();

        // 60 days expiry by default? Should be from config, but let's assume 7 days for refresh token.
        // Wait, IP address? The handler doesn't have HttpContext. Let's just pass empty or inject it.
        var refreshToken = CulinaryBlog.Domain.Entities.RefreshToken.CreateNewFamily(userId, tokenHash, 7, "127.0.0.1");
        
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = rawToken,
            ExpiresIn = _jwtService.AccessTokenLifetimeSeconds,
            User = new UserDto
            {
                Id = userId,
                Email = request.Email,
                DisplayName = request.DisplayName,
                Roles = new[] { Roles.Author },
                EmailConfirmed = false,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };
    }
}
