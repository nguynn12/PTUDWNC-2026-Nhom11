using CulinaryBlog.Application.Auth.Dtos;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Auth.Queries.GetProfile;

/// <summary>SRS FR-AUTH-006 — tài khoản bị vô hiệu hoá không xem được hồ sơ dù access token còn hạn.</summary>
public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, UserDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;

    public GetProfileQueryHandler(ICurrentUserService currentUser, IIdentityService identityService)
    {
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<UserDto> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { Length: > 0 } userId)
        {
            throw new UnauthorizedException("Bạn chưa đăng nhập.", ErrorCodes.AuthTokenInvalid);
        }

        var user = await _identityService.GetUserDetailsByIdAsync(userId);
        if (user == null)
        {
            throw new UnauthorizedException("Tài khoản không còn tồn tại.", ErrorCodes.AuthTokenInvalid);
        }

        if (!user.IsActive)
        {
            throw new ForbiddenException("Tài khoản đã bị quản trị viên vô hiệu hoá.", ErrorCodes.AuthAccountDisabled);
        }

        return user.ToUserDto();
    }
}
