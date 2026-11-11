using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Admin.Queries.GetUsers;

public sealed class GetUsersQueryHandler(IAdminUserQueryService users)
    : IRequestHandler<GetUsersQuery, GetUsersResult>
{
    public async Task<GetUsersResult> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Clamp(request.Page, 1, 10_000);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var (items, totalCount) = await users.GetUsersAsync(
            page, pageSize, request.Search, request.Role, request.Status, request.SortBy, cancellationToken);

        return new GetUsersResult(
            items,
            page,
            pageSize,
            totalCount,
            (int)Math.Ceiling(totalCount / (double)pageSize));
    }
}
