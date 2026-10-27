using CulinaryBlog.Application.Auth.Dtos;
using MediatR;

namespace CulinaryBlog.Application.Auth.Commands.Register;

public record RegisterCommand(string Email, string Password, string DisplayName) : IRequest<AuthResponseDto>;
