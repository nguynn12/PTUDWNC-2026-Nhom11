namespace CulinaryBlog.Application.Features.Recipes.Commands.PurgeRecipe;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

/// <summary>
/// Command dành cho Admin xóa vĩnh viễn (xóa vật lý) công thức đã nằm trong thùng rác.
/// </summary>
public record PurgeRecipeCommand(Guid Id) : IRequest<bool>;

/// <summary>
/// Handler xử lý xóa vĩnh viễn công thức và toàn bộ dữ liệu liên quan khỏi cơ sở dữ liệu.
/// Tái sử dụng IRecipePurgeService dùng chung với Hangfire PurgeExpiredRecipesJob (FR-JOB-004, FR-RCP-007).
/// </summary>
public sealed class PurgeRecipeCommandHandler(
    IRecipePurgeService recipePurgeService,
    ICurrentUserService currentUserService)
    : IRequestHandler<PurgeRecipeCommand, bool>
{
    public async Task<bool> Handle(
        PurgeRecipeCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Kiểm tra xác thực và quyền Quản trị viên (Admin)
        if (string.IsNullOrWhiteSpace(currentUserService.UserId))
        {
            throw new UnauthorizedException("Vui lòng đăng nhập để xóa vĩnh viễn công thức.", "AUTH_UNAUTHORIZED");
        }

        if (!currentUserService.IsAdmin)
        {
            throw new ForbiddenException("Chỉ Quản trị viên mới có quyền xóa vĩnh viễn công thức.", "RECIPE_FORBIDDEN");
        }

        return await recipePurgeService.PurgeRecipeAsync(request.Id, cancellationToken);
    }
}
