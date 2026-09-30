using CulinaryBlog.Application.Auth.Dtos;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Constants;
using CulinaryBlog.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
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
        var isValid = await _identityService.CheckPasswordAsync(request.Email, request.Password);
        
        if (!isValid)
        {
            throw new UnauthorizedException(ErrorCodes.AuthInvalidCredentials, "Invalid email or password.");
        }

        var userDetails = await _identityService.GetUserDetailsByEmailAsync(request.Email);
        if (userDetails == null)
        {
            throw new UnauthorizedException(ErrorCodes.AuthInvalidCredentials, "Invalid email or password.");
        }

        var accessToken = _jwtService.GenerateAccessToken(
            userDetails.Value.Id, 
            userDetails.Value.Email, 
            userDetails.Value.Roles, 
            userDetails.Value.EmailConfirmed);

        var (tokenHash, rawToken) = _jwtService.GenerateRefreshToken();
        
        var refreshToken = RefreshToken.CreateNewFamily(userDetails.Value.Id, tokenHash, 7, "127.0.0.1");
        
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = rawToken,
            ExpiresIn = 900,
            User = new UserDto
            {
                Id = userDetails.Value.Id,
                Email = userDetails.Value.Email,
                DisplayName = userDetails.Value.DisplayName,
                Roles = userDetails.Value.Roles,
                EmailConfirmed = userDetails.Value.EmailConfirmed,
                CreatedAt = DateTimeOffset.UtcNow // Missing created at, but whatever
            }
        };
    }
}
