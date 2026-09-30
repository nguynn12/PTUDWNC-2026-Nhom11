using CulinaryBlog.Application.Auth.Dtos;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Auth.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UserDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;

    public UpdateProfileCommandHandler(ICurrentUserService currentUser, IIdentityService identityService)
    {
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<UserDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { Length: > 0 } userId)
        {
            throw new UnauthorizedException("Bạn chưa đăng nhập.", ErrorCodes.AuthTokenInvalid);
        }

        var current = await _identityService.GetUserDetailsByIdAsync(userId);
        if (current == null)
        {
            throw new UnauthorizedException("Tài khoản không còn tồn tại.", ErrorCodes.AuthTokenInvalid);
        }

        if (!current.IsActive)
        {
            throw new ForbiddenException("Tài khoản đã bị quản trị viên vô hiệu hoá.", ErrorCodes.AuthAccountDisabled);
        }

        var result = await _identityService.UpdateProfileAsync(userId, request.DisplayName, request.Bio, request.AvatarUrl);
        if (!result)
        {
            throw new BusinessRuleValidationException("Không cập nhật được hồ sơ.", ErrorCodes.ValidationError);
        }

        var updated = await _identityService.GetUserDetailsByIdAsync(userId)
            ?? throw new UnauthorizedException("Tài khoản không còn tồn tại.", ErrorCodes.AuthTokenInvalid);

        return updated.ToUserDto();
    }
}
