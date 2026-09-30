using CulinaryBlog.Application.Auth.Dtos;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Auth.Queries.GetProfile;

/// <summary>SRS FR-AUTH-006 — tài khoản bị vô hiệu hoá không xem được hồ sơ dù access token còn hạn.</summary>
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
            throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Bạn chưa đăng nhập.");
        }

        var user = await _identityService.GetUserDetailsByIdAsync(_currentUser.Id);
        if (user == null)
        {
            throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Tài khoản không còn tồn tại.");
        }

        if (!user.IsActive)
        {
            throw new ForbiddenException(ErrorCodes.AuthAccountDisabled, "Tài khoản đã bị quản trị viên vô hiệu hoá.");
        }

        return user.ToUserDto();
    }
}
