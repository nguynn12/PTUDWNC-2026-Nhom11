using CulinaryBlog.Application.Auth.Dtos;
using MediatR;

namespace CulinaryBlog.Application.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;
