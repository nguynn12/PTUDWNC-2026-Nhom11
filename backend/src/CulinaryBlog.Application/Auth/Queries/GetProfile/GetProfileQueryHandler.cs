using CulinaryBlog.Application.Auth.Dtos;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Auth.Queries.GetProfile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, UserDto>
{
    private readonly ICurrentUser _currentUser;
    private readonly IIdentityService _identityService;

    public GetProfileQueryHandler(ICurrentUser currentUser, IIdentityService identityService)
    {
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<UserDto> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "You are not authorized.");
        }

        var userDetails = await _identityService.GetUserDetailsByIdAsync(_currentUser.Id);
        
        if (userDetails == null)
        {
            throw new NotFoundException(ErrorCodes.AuthAccountDisabled, "User not found.");
        }

        return new UserDto
        {
            Id = userDetails.Value.Id,
            Email = userDetails.Value.Email,
            DisplayName = userDetails.Value.DisplayName,
            Roles = userDetails.Value.Roles,
            EmailConfirmed = userDetails.Value.EmailConfirmed,
            CreatedAt = DateTimeOffset.UtcNow // Placeholder, ideally fetch from DB
        };
    }
}
