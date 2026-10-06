using CulinaryBlog.Application.Auth.Dtos;
using MediatR;

namespace CulinaryBlog.Application.Auth.Queries.GetProfile;

public record GetProfileQuery() : IRequest<UserDto>;
