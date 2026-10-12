namespace CulinaryBlog.Application.Features.Recipes.Queries.GetTrashedRecipes;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Query dành cho Admin lấy danh sách các Recipe đã bị xóa mềm trong thùng rác (phân trang theo DeletedAt giảm dần).
/// </summary>
public record GetTrashedRecipesQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PagedResult<TrashedRecipeDto>>;

/// <summary>
/// Handler xử lý truy vấn danh sách thùng rác Recipe.
/// Sử dụng IApplicationDbContext.Recipes.IgnoreQueryFilters() để đọc các bản ghi IsDeleted = true.
/// </summary>
public sealed class GetTrashedRecipesQueryHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetTrashedRecipesQuery, PagedResult<TrashedRecipeDto>>
{
    public async Task<PagedResult<TrashedRecipeDto>> Handle(
        GetTrashedRecipesQuery request,
        CancellationToken cancellationToken)
    {
        // Kiểm tra quyền Admin (nếu có context user đăng nhập)
        if (!string.IsNullOrWhiteSpace(currentUserService.UserId) && !currentUserService.IsAdmin)
        {
            throw new ForbiddenException("Chỉ Quản trị viên mới có quyền xem thùng rác.", "RECIPE_FORBIDDEN");
        }

        var page = request.PageNumber > 0 ? request.PageNumber : 1;
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;

        var query = dbContext.Recipes
            .IgnoreQueryFilters()
            .Where(r => r.IsDeleted)
            .OrderByDescending(r => r.DeletedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new TrashedRecipeDto
            {
                Id = r.Id,
                Title = r.Title,
                Slug = r.Slug,
                Status = r.Status,
                AuthorId = r.AuthorId,
                CategoryId = r.CategoryId,
                CreatedAt = r.CreatedAt,
                DeletedAt = r.DeletedAt,
                DaysRemaining = r.DeletedAt.HasValue
                    ? Math.Max(0, 30 - (int)(DateTimeOffset.UtcNow - r.DeletedAt.Value).TotalDays)
                    : 0
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<TrashedRecipeDto>
        {
            Items = items,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
