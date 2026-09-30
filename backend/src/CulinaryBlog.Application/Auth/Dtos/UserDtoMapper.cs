using CulinaryBlog.Application.Common.Models;

namespace CulinaryBlog.Application.Auth.Dtos;

public static class UserDtoMapper
{
    /// <summary>Map sang <see cref="UserDto"/> đủ field theo SRS FR-AUTH-006 (không có dữ liệu nhạy cảm).</summary>
    public static UserDto ToUserDto(this UserAccount account)
    {
        ArgumentNullException.ThrowIfNull(account);

        return new UserDto
        {
            Id = account.Id,
            Email = account.Email,
            DisplayName = account.DisplayName,
            AvatarUrl = account.AvatarUrl,
            Bio = account.Bio,
            Roles = account.Roles,
            EmailConfirmed = account.EmailConfirmed,
            CreatedAt = account.CreatedAt,
        };
    }
}
