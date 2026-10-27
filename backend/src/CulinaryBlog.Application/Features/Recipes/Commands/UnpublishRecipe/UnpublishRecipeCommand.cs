namespace CulinaryBlog.Application.Features.Recipes.Commands.UnpublishRecipe;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Recipes.DTOs;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using MediatR;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Command hủy xuất bản công thức nấu ăn (Published -> Draft).
/// </summary>
public record UnpublishRecipeCommand(Guid Id, uint? ConcurrencyToken = null) : IRequest<RecipeResponseDto>;

/// <summary>
/// Handler xử lý hủy xuất bản công thức nấu ăn.
/// </summary>
public sealed class UnpublishRecipeCommandHandler(
    IRepository<Recipe> recipeRepository,
    IUnitOfWork unitOfWork,
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService)
    : IRequestHandler<UnpublishRecipeCommand, RecipeResponseDto>
{
    public async Task<RecipeResponseDto> Handle(
        UnpublishRecipeCommand request,
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
            throw new UnauthorizedException("Vui lòng đăng nhập để hủy xuất bản công thức.", "AUTH_UNAUTHORIZED");
        }

        if (!currentUserService.IsAdmin &&
            !string.Equals(recipe.AuthorId, currentUserService.UserId, StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException("Bạn không có quyền hủy xuất bản công thức này.", "RECIPE_FORBIDDEN");
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

        // 3. Chỉ công thức đang Published mới được Unpublish về Draft
        if (recipe.Status != RecipeStatus.Published)
        {
            throw new ConflictException("Chỉ có thể hủy xuất bản đối với công thức đang ở trạng thái Published.", "RECIPE_INVALID_STATUS_TRANSITION");
        }

        recipe.Status = RecipeStatus.Draft;
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        recipeRepository.Update(recipe);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedXmin = await dbContext.Recipes
            .Where(r => r.Id == recipe.Id)
            .Select(r => EF.Property<uint>(r, "xmin"))
            .FirstOrDefaultAsync(cancellationToken);

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
            UpdatedAt = recipe.UpdatedAt,
            ConcurrencyToken = updatedXmin
        };
    }
}
