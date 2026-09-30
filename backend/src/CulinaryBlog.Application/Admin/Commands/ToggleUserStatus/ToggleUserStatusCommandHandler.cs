using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Admin.Commands.ToggleUserStatus;

public class ToggleUserStatusCommandHandler : IRequestHandler<ToggleUserStatusCommand>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUser _currentUser;

    public ToggleUserStatusCommandHandler(IIdentityService identityService, ICurrentUser currentUser)
    {
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async Task Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Id == request.UserId && !request.IsActive)
        {
            throw new BusinessRuleException("AUTH_CANNOT_DEACTIVATE_SELF", "Admin cannot deactivate their own account.");
        }

        var result = await _identityService.ToggleUserStatusAsync(request.UserId, request.IsActive);

        if (!result)
        {
            throw new NotFoundException("USER_NOT_FOUND", "User not found.");
        }
    }
}
