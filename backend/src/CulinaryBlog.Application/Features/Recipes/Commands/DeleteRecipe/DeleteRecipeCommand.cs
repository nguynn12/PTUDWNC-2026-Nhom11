namespace CulinaryBlog.Application.Features.Recipes.Commands.DeleteRecipe;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using MediatR;

/// <summary>
/// Command xóa mềm (Soft Delete) công thức nấu ăn.
/// </summary>
public record DeleteRecipeCommand(Guid Id) : IRequest<bool>;

/// <summary>
/// Handler xử lý xóa mềm công thức nấu ăn.
/// Đánh dấu IsDeleted = true và ghi nhận thời điểm DeletedAt theo đặc tả SRS 7.1.
/// </summary>
public sealed class DeleteRecipeCommandHandler(
    IRepository<Recipe> recipeRepository,
    IUnitOfWork unitOfWork,
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

        // Kiểm tra quyền sở hữu
        if (!string.IsNullOrWhiteSpace(currentUserService.UserId) &&
            !currentUserService.IsAdmin &&
            !string.Equals(recipe.AuthorId, currentUserService.UserId, StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException("Bạn không có quyền xóa công thức này.", "RECIPE_FORBIDDEN");
        }

        // Thực hiện xóa mềm (Soft delete)
        recipe.IsDeleted = true;
        recipe.DeletedAt = DateTimeOffset.UtcNow;
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        recipeRepository.Update(recipe);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
