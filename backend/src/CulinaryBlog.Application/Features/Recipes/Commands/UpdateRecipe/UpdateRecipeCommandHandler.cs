namespace CulinaryBlog.Application.Features.Recipes.Commands.UpdateRecipe;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Helpers;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Recipes.DTOs;
using CulinaryBlog.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Handler xử lý cập nhật thông tin công thức nấu ăn.
/// Tuân thủ quy tắc quyền tác giả, slug history (C7), và kiểm soát concurrency bằng xmin (C6).
/// </summary>
public sealed class UpdateRecipeCommandHandler(
    IRepository<Recipe> recipeRepository,
    IRepository<RecipeSlugHistory> slugHistoryRepository,
    IUnitOfWork unitOfWork,
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService)
    : IRequestHandler<UpdateRecipeCommand, RecipeResponseDto>
{
    public async Task<RecipeResponseDto> Handle(
        UpdateRecipeCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Tìm công thức qua IRepository
        var recipe = await recipeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (recipe == null)
        {
            throw new NotFoundException("Recipe", request.Id, "RECIPE_NOT_FOUND");
        }

        // 2. Kiểm tra quyền sở hữu (Tác giả hoặc Quản trị viên)
        if (!string.IsNullOrWhiteSpace(currentUserService.UserId) &&
            !currentUserService.IsAdmin &&
            !string.Equals(recipe.AuthorId, currentUserService.UserId, StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException("Bạn không có quyền chỉnh sửa công thức này.", "RECIPE_FORBIDDEN");
        }

        // 3. Kiểm soát Concurrency Token qua PostgreSQL xmin (RESOLVED-CONFLICTS C6)
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

        // 4. Kiểm tra danh mục món ăn nếu thay đổi
        if (recipe.CategoryId != request.CategoryId)
        {
            var categoryExists = await dbContext.Categories
                .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

            if (!categoryExists)
            {
                throw new NotFoundException("Category", request.CategoryId, "CATEGORY_NOT_FOUND");
            }

            recipe.CategoryId = request.CategoryId;
        }

        // 5. Xử lý cập nhật Tiêu đề và Slug (RESOLVED-CONFLICTS C7)
        var newTitle = request.Title.Trim();
        var titleChanged = !string.Equals(recipe.Title, newTitle, StringComparison.Ordinal);

        if (titleChanged)
        {
            // Nếu công thức CHƯA TỪNG xuất bản (PublishedAt == null), cho phép đổi slug và lưu slug cũ vào lịch sử
            if (!recipe.PublishedAt.HasValue)
            {
                var baseSlug = SlugHelper.GenerateSlug(newTitle);
                var newSlug = baseSlug;
                var suffix = 1;

                while (await dbContext.Recipes.AnyAsync(r => r.Id != recipe.Id && r.Slug == newSlug, cancellationToken) ||
                       await dbContext.RecipeSlugHistories.AnyAsync(h => h.OldSlug == newSlug, cancellationToken))
                {
                    newSlug = $"{baseSlug}-{suffix++}";
                }

                if (!string.Equals(recipe.Slug, newSlug, StringComparison.Ordinal))
                {
                    slugHistoryRepository.Add(new RecipeSlugHistory
                    {
                        RecipeId = recipe.Id,
                        OldSlug = recipe.Slug,
                        CreatedAt = DateTimeOffset.UtcNow
                    });

                    recipe.Slug = newSlug;
                }
            }
            // Nếu ĐÃ TỪNG xuất bản, slug là bất biến (C7) => không thay đổi recipe.Slug

            recipe.Title = newTitle;
        }

        // 6. Cập nhật các trường thông tin khác
        recipe.Description = request.Description.Trim();
        recipe.PrepTimeMinutes = request.PrepTimeMinutes;
        recipe.CookTimeMinutes = request.CookTimeMinutes;
        recipe.Servings = request.Servings;
        recipe.Difficulty = request.Difficulty;
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        // 7. Cập nhật qua IRepository và commit qua IUnitOfWork
        recipeRepository.Update(recipe);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 8. Lấy giá trị xmin mới sau khi commit
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
