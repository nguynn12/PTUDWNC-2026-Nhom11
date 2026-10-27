namespace CulinaryBlog.Application.Features.Recipes.Commands.DeleteRecipe;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Command xóa mềm (Soft Delete) công thức nấu ăn.
/// </summary>
public record DeleteRecipeCommand(Guid Id, uint? ConcurrencyToken = null) : IRequest<bool>;

/// <summary>
/// Handler xử lý xóa mềm công thức nấu ăn.
/// Đánh dấu IsDeleted = true và ghi nhận thời điểm DeletedAt theo đặc tả SRS 7.1.
/// </summary>
public sealed class DeleteRecipeCommandHandler(
    IRepository<Recipe> recipeRepository,
    IUnitOfWork unitOfWork,
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService)
    : IRequestHandler<DeleteRecipeCommand, bool>
{
    public async Task<bool> Handle(
        DeleteRecipeCommand request,
        CancellationToken cancellationToken)
    {
        var recipe = await recipeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (recipe == null)
        {
            throw new NotFoundException("Recipe", request.Id, "RECIPE_NOT_FOUND");
        }

        // 1. Kiểm tra xác thực và quyền sở hữu (bắt buộc có UserId, không cho phép ẩn danh)
        if (string.IsNullOrWhiteSpace(currentUserService.UserId))
        {
            throw new UnauthorizedException("Vui lòng đăng nhập để xóa công thức.", "AUTH_UNAUTHORIZED");
        }

        if (!currentUserService.IsAdmin &&
            !string.Equals(recipe.AuthorId, currentUserService.UserId, StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException("Bạn không có quyền xóa công thức này.", "RECIPE_FORBIDDEN");
        }

        // 2. Kiểm tra Concurrency Token qua xmin
        if (request.ConcurrencyToken.HasValue)
        {
            var currentXmin = await dbContext.Recipes
                .Where(r => r.Id == recipe.Id)
                .Select(r => EF.Property<uint>(r, "xmin"))
                .FirstOrDefaultAsync(cancellationToken);

            if (currentXmin != 0 && request.ConcurrencyToken.Value != currentXmin)
            {
                throw new ConflictException("Dữ liệu công thức đã bị thay đổi bởi phiên làm việc khác. Vui lòng tải lại trang.", "RECIPE_CONCURRENCY_CONFLICT");
            }
        }

        // 3. Thực hiện xóa mềm (Soft delete)
        recipe.IsDeleted = true;
        recipe.DeletedAt = DateTimeOffset.UtcNow;
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        recipeRepository.Update(recipe);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
