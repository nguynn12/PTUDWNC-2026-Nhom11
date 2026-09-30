using CulinaryBlog.Application.Auth.Dtos;
using MediatR;

namespace CulinaryBlog.Application.Auth.Commands.UpdateProfile;

public record UpdateProfileCommand(string DisplayName, string? Bio, string? AvatarUrl) : IRequest<UserDto>;
