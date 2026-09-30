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
            throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Bạn chưa đăng nhập.");
        }

        var current = await _identityService.GetUserDetailsByIdAsync(_currentUser.Id);
        if (current == null)
        {
            throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Tài khoản không còn tồn tại.");
        }

        if (!current.IsActive)
        {
            throw new ForbiddenException(ErrorCodes.AuthAccountDisabled, "Tài khoản đã bị quản trị viên vô hiệu hoá.");
        }

        var result = await _identityService.UpdateProfileAsync(_currentUser.Id, request.DisplayName, request.Bio, request.AvatarUrl);
        if (!result)
        {
            throw new BusinessRuleException(ErrorCodes.ValidationError, "Không cập nhật được hồ sơ.");
        }

        var updated = await _identityService.GetUserDetailsByIdAsync(_currentUser.Id)
            ?? throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Tài khoản không còn tồn tại.");

        return updated.ToUserDto();
    }
}
