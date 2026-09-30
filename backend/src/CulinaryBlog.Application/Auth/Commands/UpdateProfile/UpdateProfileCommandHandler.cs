using CulinaryBlog.Application.Auth.Dtos;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Auth.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UserDto>
{
    private readonly ICurrentUser _currentUser;
    private readonly IIdentityService _identityService;

    public UpdateProfileCommandHandler(ICurrentUser currentUser, IIdentityService identityService)
    {
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<UserDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "You are not authorized.");
        }

        var result = await _identityService.UpdateProfileAsync(_currentUser.Id, request.DisplayName, request.Bio, request.AvatarUrl);

        if (!result)
        {
            throw new BusinessRuleException(ErrorCodes.ValidationError, "Failed to update profile.");
        }

        var userDetails = await _identityService.GetUserDetailsByIdAsync(_currentUser.Id);

        return new UserDto
        {
            Id = userDetails.Value.Id,
            Email = userDetails.Value.Email,
            DisplayName = userDetails.Value.DisplayName,
            Roles = userDetails.Value.Roles,
            EmailConfirmed = userDetails.Value.EmailConfirmed,
            CreatedAt = DateTimeOffset.UtcNow // Placeholder
        };
    }
}
