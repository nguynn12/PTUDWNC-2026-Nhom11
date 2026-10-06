using CulinaryBlog.Application.Admin.Commands.ToggleUserStatus;
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
        var group = app.MapGroup("/api/v1/admin/users").WithTags("Admin Users").RequireAuthorization("Admin");

        group.MapPatch("/{id}/status", async (string id, [FromBody] ToggleUserStatusRequest request, ISender sender) =>
        {
            var command = new ToggleUserStatusCommand(id, request.IsActive);
            await sender.Send(command);
            return Results.NoContent();
        });
    }
}

public class ToggleUserStatusRequest
{
    public bool IsActive { get; set; }
}
