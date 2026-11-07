using CulinaryBlog.Application.Admin.Dtos;
using MediatR;

namespace CulinaryBlog.Application.Admin.Queries.GetUsers;

public sealed record GetUsersQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    string? Role = null,
    string? Status = null,
    string? SortBy = "createdAt") : IRequest<GetUsersResult>;

public sealed record GetUsersResult(
    IReadOnlyList<AdminUserDto> Data,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
