using CulinaryBlog.Application.Admin.Commands.ToggleUserStatus;
using CulinaryBlog.Application.Admin.Dtos;
using CulinaryBlog.Application.Admin.Queries.GetUsers;
using CulinaryBlog.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CulinaryBlog.API.Endpoints;

public static class AdminUserEndpoints
{
    public static void MapAdminUserEndpoints(this IEndpointRouteBuilder app)
    {
        // Gắn vào nhóm /api/v{version} (Program.cs) nên đường dẫn thực tế là /api/v1/admin/users.
        var group = app.MapGroup("/admin/users").WithTags("Admin Users").RequireAuthorization("Admin");

        group.MapGet("", async (
            [AsParameters] GetUsersRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetUsersQuery(
                request.Page, request.PageSize, request.Search, request.Role, request.Status, request.SortBy),
                cancellationToken);
            return Results.Ok(ApiResponse<IReadOnlyList<AdminUserDto>>.Success(
                result.Data,
                new { result.Page, result.PageSize, result.TotalCount, result.TotalPages }));
        });

        group.MapPatch("/{id}/status", async (string id, [FromBody] ToggleUserStatusRequest request, ISender sender) =>
        {
            var command = new ToggleUserStatusCommand(id, request.IsActive);
            await sender.Send(command);
            return Results.NoContent();
        });
    }
}

public sealed class GetUsersRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public string? Role { get; set; }
    public string? Status { get; set; }
    public string? SortBy { get; set; } = "createdAt";
}

public class ToggleUserStatusRequest
{
    public bool IsActive { get; set; }
}
