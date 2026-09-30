using CulinaryBlog.Application.Auth.Dtos;
using MediatR;

namespace CulinaryBlog.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponseDto>;
