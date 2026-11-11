using CulinaryBlog.Application.Admin.Dtos;

namespace CulinaryBlog.Application.Common.Interfaces;

public interface IAdminUserQueryService
{
    Task<(IReadOnlyList<AdminUserDto> Users, int TotalCount)> GetUsersAsync(
        int page,
        int pageSize,
        string? search,
        string? role,
        string? status,
        string? sortBy,
        CancellationToken cancellationToken);
}
