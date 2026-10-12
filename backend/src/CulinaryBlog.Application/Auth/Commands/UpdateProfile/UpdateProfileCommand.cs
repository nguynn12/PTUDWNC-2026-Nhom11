using CulinaryBlog.Application.Auth.Dtos;
using MediatR;

namespace CulinaryBlog.Application.Auth.Commands.UpdateProfile;

/// <summary>
/// SRS FR-AUTH-007: <c>PATCH /api/v1/auth/me</c> với <c>displayName?</c>, <c>avatarUrl?</c>, <c>bio?</c>.
/// Field null (hoặc không gửi) thì giữ nguyên giá trị cũ. Email/username không đổi qua endpoint này.
/// </summary>
public record UpdateProfileCommand(string? DisplayName, string? AvatarUrl, string? Bio) : IRequest<UserDto>;
