using MediatR;

namespace CulinaryBlog.Application.Admin.Commands.ToggleUserStatus;

public record ToggleUserStatusCommand(string UserId, bool IsActive) : IRequest;
