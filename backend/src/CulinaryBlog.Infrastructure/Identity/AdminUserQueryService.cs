using CulinaryBlog.Application.Admin.Dtos;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Identity;

public sealed class AdminUserQueryService(CulinaryBlogDbContext context) : IAdminUserQueryService
{
    public async Task<(IReadOnlyList<AdminUserDto> Users, int TotalCount)> GetUsersAsync(
        int page,
        int pageSize,
        string? search,
        string? role,
        string? status,
        string? sortBy,
        CancellationToken cancellationToken)
    {
        var query = context.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(user =>
                user.Email!.Contains(term) || user.DisplayName.Contains(term));
        }

        if (status?.Equals("active", StringComparison.OrdinalIgnoreCase) == true)
            query = query.Where(user => user.IsActive);
        else if (status?.Equals("inactive", StringComparison.OrdinalIgnoreCase) == true)
            query = query.Where(user => !user.IsActive);

        if (role is "Admin" or "Author")
        {
            var roleId = await context.Roles
                .Where(item => item.Name == role)
                .Select(item => item.Id)
                .SingleOrDefaultAsync(cancellationToken);
            query = roleId is null
                ? query.Where(_ => false)
                : query.Where(user => context.UserRoles.Any(link => link.UserId == user.Id && link.RoleId == roleId));
        }

        query = sortBy?.ToLowerInvariant() switch
        {
            "email" => query.OrderBy(user => user.Email),
            "displayname" or "name" => query.OrderBy(user => user.DisplayName),
            "status" => query.OrderByDescending(user => user.IsActive).ThenBy(user => user.DisplayName),
            _ => query.OrderByDescending(user => user.CreatedAt),
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var selected = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new
            {
                user.Id,
                Email = user.Email!,
                user.DisplayName,
                user.AvatarUrl,
                user.EmailConfirmed,
                user.IsActive,
                user.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        var ids = selected.Select(user => user.Id).ToArray();
        var roles = await (
            from link in context.UserRoles
            join identityRole in context.Roles on link.RoleId equals identityRole.Id
            where ids.Contains(link.UserId)
            select new { link.UserId, Role = identityRole.Name! })
            .ToListAsync(cancellationToken);

        var result = selected.Select(user => new AdminUserDto(
            user.Id,
            user.Email,
            user.DisplayName,
            user.AvatarUrl,
            roles.Where(item => item.UserId == user.Id).Select(item => item.Role).ToArray(),
            user.EmailConfirmed,
            user.IsActive,
            user.CreatedAt)).ToList();

        return (result, totalCount);
    }
}
