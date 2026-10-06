using MediatR;

namespace CulinaryBlog.Application.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest;
