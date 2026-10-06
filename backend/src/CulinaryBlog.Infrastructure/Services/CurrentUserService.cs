namespace CulinaryBlog.Infrastructure.Services;

using System.Security.Claims;
using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Dịch vụ trích xuất thông tin người dùng từ HttpContext của request hiện tại.
/// </summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? UserId =>
        httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? httpContextAccessor.HttpContext?.User?.FindFirstValue("sub")
        ?? httpContextAccessor.HttpContext?.User?.FindFirstValue("uid");

    public bool IsAdmin =>
        httpContextAccessor.HttpContext?.User?.IsInRole("Admin") ?? false;
}
