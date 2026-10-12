namespace CulinaryBlog.Application.Features.Recipes.Commands.RestoreRecipe;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Recipes.DTOs;
using CulinaryBlog.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Command dành cho Admin khôi phục công thức đã xóa mềm trong vòng 30 ngày.
/// </summary>
public record RestoreRecipeCommand(Guid Id) : IRequest<RecipeResponseDto>;

/// <summary>
/// Handler xử lý khôi phục công thức nấu ăn từ thùng rác.
/// Tuân thủ quy tắc RECIPE_NOT_DELETED (409) và RECIPE_RESTORE_WINDOW_EXPIRED (409) theo đặc tả SRS FR-RCP-007.
/// </summary>
public sealed class RestoreRecipeCommandHandler(
    IRepository<Recipe> recipeRepository,
    IUnitOfWork unitOfWork,
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService)
    : IRequestHandler<RestoreRecipeCommand, RecipeResponseDto>
{
    public async Task<RecipeResponseDto> Handle(
        RestoreRecipeCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Kiểm tra quyền Quản trị viên (Admin)
        if (!string.IsNullOrWhiteSpace(currentUserService.UserId) && !currentUserService.IsAdmin)
        {
            throw new ForbiddenException("Chỉ Quản trị viên mới có quyền khôi phục công thức.", "RECIPE_FORBIDDEN");
        }

        // 2. Tìm công thức (bỏ qua Global Query Filter vì công thức đang IsDeleted = true)
        var recipe = await dbContext.Recipes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (recipe == null)
        {
            throw new NotFoundException("Recipe", request.Id, "RECIPE_NOT_FOUND");
        }

        // 3. Kiểm tra: nếu công thức chưa bị xóa mềm thì không thể khôi phục
        if (!recipe.IsDeleted)
        {
            throw new ConflictException("Công thức chưa bị xóa mềm, không cần khôi phục.", "RECIPE_NOT_DELETED");
        }

        // 4. Kiểm tra thời hạn khôi phục 30 ngày (SRS FR-RCP-007)
        if (recipe.DeletedAt.HasValue && (DateTimeOffset.UtcNow - recipe.DeletedAt.Value).TotalDays > 30)
        {
            throw new ConflictException(
                "Công thức đã quá thời hạn khôi phục 30 ngày. Vui lòng liên hệ quản trị hệ thống.",
                "RECIPE_RESTORE_WINDOW_EXPIRED");
        }

        // 5. Khôi phục trạng thái
        recipe.IsDeleted = false;
        recipe.DeletedAt = null;
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        recipeRepository.Update(recipe);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RecipeResponseDto
        {
            Id = recipe.Id,
            Title = recipe.Title,
            Slug = recipe.Slug,
            Description = recipe.Description,
            PrepTimeMinutes = recipe.PrepTimeMinutes,
            CookTimeMinutes = recipe.CookTimeMinutes,
            Servings = recipe.Servings,
            Difficulty = recipe.Difficulty,
            Status = recipe.Status,
            CategoryId = recipe.CategoryId,
            AuthorId = recipe.AuthorId,
            PublishedAt = recipe.PublishedAt,
            CreatedAt = recipe.CreatedAt,
            UpdatedAt = recipe.UpdatedAt
        };
    }
}
