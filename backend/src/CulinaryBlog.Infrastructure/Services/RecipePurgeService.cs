namespace CulinaryBlog.Infrastructure.Services;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Dịch vụ xóa vĩnh viễn công thức và dọn dẹp tài nguyên (FR-JOB-004, FR-RCP-007).
/// Đảm bảo tính nhất quán giữa thao tác xóa thủ công của Admin và tiến trình Hangfire dọn rác 30 ngày.
/// </summary>
public class RecipePurgeService(
    IApplicationDbContext dbContext,
    IUnitOfWork unitOfWork,
    IRepository<Recipe> recipeRepository,
    IFileStorageService fileStorageService,
    ILogger<RecipePurgeService> logger) : IRecipePurgeService
{
    public async Task<bool> PurgeRecipeAsync(Guid recipeId, CancellationToken cancellationToken = default)
    {
        var recipe = await dbContext.Recipes
            .IgnoreQueryFilters()
            .Include(r => r.Images)
            .FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);

        if (recipe == null)
        {
            throw new NotFoundException("Recipe", recipeId, "RECIPE_NOT_FOUND");
        }

        if (!recipe.IsDeleted)
        {
            throw new ConflictException(
                "Chỉ có thể xóa vĩnh viễn đối với công thức đã nằm trong thùng rác.",
                "RECIPE_NOT_DELETED");
        }

        // Dọn dẹp các tệp ảnh liên quan trên bộ lưu trữ (xóa cả Original, Medium, Thumbnail theo MEDIA-02)
        if (recipe.Images != null)
        {
            foreach (var img in recipe.Images)
            {
                var urlsToDelete = new[] { img.OriginalUrl, img.MediumUrl, img.ThumbnailUrl }
                    .Where(u => !string.IsNullOrWhiteSpace(u));

                foreach (var url in urlsToDelete)
                {
                    try
                    {
                        await fileStorageService.DeleteAsync(url!, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Không thể xóa tệp ảnh {Url} khi purge công thức {RecipeId}", url, recipeId);
                    }
                }
            }
        }

        recipeRepository.Remove(recipe);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[PURGE] Công thức {RecipeId} ({Title}) đã được xóa vĩnh viễn khỏi hệ thống", recipeId, recipe.Title);
        return true;
    }

    public async Task<int> PurgeExpiredRecipesAsync(int daysThreshold = 30, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-daysThreshold);

        var expiredRecipeIds = await dbContext.Recipes
            .IgnoreQueryFilters()
            .Where(r => r.IsDeleted && r.DeletedAt.HasValue && r.DeletedAt.Value <= cutoff)
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        var purgedCount = 0;
        foreach (var id in expiredRecipeIds)
        {
            try
            {
                await PurgeRecipeAsync(id, cancellationToken);
                purgedCount++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi xảy ra khi tự động purge công thức quá hạn {RecipeId}", id);
            }
        }

        logger.LogInformation(
            "[PURGE-JOB] Hoàn thành dọn rác tự động 30 ngày. Đã xóa vĩnh viễn {PurgedCount}/{TotalCount} công thức",
            purgedCount,
            expiredRecipeIds.Count);

        return purgedCount;
    }
}

