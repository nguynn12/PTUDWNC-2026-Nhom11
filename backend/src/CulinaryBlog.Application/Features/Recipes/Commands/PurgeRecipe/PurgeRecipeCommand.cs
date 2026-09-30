namespace CulinaryBlog.Application.Features.Recipes.Commands.PurgeRecipe;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Command dành cho Admin xóa vĩnh viễn (xóa vật lý) công thức đã nằm trong thùng rác.
/// </summary>
public record PurgeRecipeCommand(Guid Id) : IRequest<bool>;

/// <summary>
/// Handler xử lý xóa vĩnh viễn công thức và toàn bộ dữ liệu liên quan khỏi cơ sở dữ liệu.
/// Tuân thủ quy tắc RECIPE_NOT_DELETED (409) theo đặc tả SRS FR-RCP-007.
/// </summary>
public sealed class PurgeRecipeCommandHandler(
    IRepository<Recipe> recipeRepository,
    IUnitOfWork unitOfWork,
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService)
    : IRequestHandler<PurgeRecipeCommand, bool>
{
    public async Task<bool> Handle(
        PurgeRecipeCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Kiểm tra quyền Quản trị viên (Admin)
        if (!string.IsNullOrWhiteSpace(currentUserService.UserId) && !currentUserService.IsAdmin)
        {
            throw new ForbiddenException("Chỉ Quản trị viên mới có quyền xóa vĩnh viễn công thức.", "RECIPE_FORBIDDEN");
        }

        // 2. Tìm công thức trong thùng rác
        var recipe = await dbContext.Recipes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (recipe == null)
        {
            throw new NotFoundException("Recipe", request.Id, "RECIPE_NOT_FOUND");
        }

        // 3. Chỉ cho phép purge nếu công thức đã bị soft-delete (xóa mềm)
        if (!recipe.IsDeleted)
        {
            throw new ConflictException(
                "Chỉ có thể xóa vĩnh viễn đối với công thức đã nằm trong thùng rác.",
                "RECIPE_NOT_DELETED");
        }

        // 4. Xóa vật lý qua IRepository và commit qua IUnitOfWork
        recipeRepository.Remove(recipe);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
