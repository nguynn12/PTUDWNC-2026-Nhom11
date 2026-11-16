namespace CulinaryBlog.Application.Features.RecipeSteps.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.RecipeSteps.DTOs;
using CulinaryBlog.Application.Features.RecipeSteps.Validators;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Hiện thực nghiệp vụ quản lý các bước thực hiện công thức nấu ăn.
/// Tuân thủ SRS v1.2.0 (FR-RCP-010), RESOLVED-CONFLICTS.md (E5, E8)
/// và kế hoạch sửa lỗi SEC-01, SEC-02, API-03, API-05, API-08, API-17.
/// </summary>
public class RecipeStepService : IRecipeStepService
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheInvalidator? _cacheInvalidator;

    public RecipeStepService(
        IApplicationDbContext context,
        ICacheInvalidator? cacheInvalidator = null)
    {
        _context = context;
        _cacheInvalidator = cacheInvalidator;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RecipeStepDto>> GetStepsAsync(
        Guid recipeId,
        string? currentUserId = null,
        bool isAdmin = false,
        CancellationToken cancellationToken = default)
    {
        var recipe = await _context.Recipes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);

        if (recipe == null)
        {
            throw new NotFoundException(nameof(Recipe), recipeId, "RECIPE_NOT_FOUND");
        }

        // SEC-02: Nếu Recipe không phải Published thì chỉ tác giả sở hữu hoặc Admin mới được xem
        if (recipe.Status != RecipeStatus.Published)
        {
            if (!isAdmin && (string.IsNullOrWhiteSpace(currentUserId) ||
                !string.Equals(recipe.AuthorId, currentUserId, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ForbiddenException("Bạn không có quyền xem các bước của công thức chưa xuất bản.");
            }
        }

        var steps = await _context.RecipeSteps
            .AsNoTracking()
            .Where(s => s.RecipeId == recipeId && !s.IsDeleted)
            .OrderBy(s => s.StepNumber)
            .Select(s => MapToDto(s))
            .ToListAsync(cancellationToken);

        return steps;
    }

    /// <inheritdoc />
    public async Task<RecipeStepDto> AddStepAsync(
        Guid recipeId,
        CreateRecipeStepRequest request,
        string? currentUserId,
        bool isAdmin,
        uint? concurrencyToken = null,
        CancellationToken cancellationToken = default)
    {
        RecipeStepValidator.ValidateCreate(request);

        var recipe = await _context.Recipes
            .FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);

        if (recipe == null)
        {
            throw new NotFoundException(nameof(Recipe), recipeId, "RECIPE_NOT_FOUND");
        }

        VerifyRecipeOwnerOrAdmin(recipe, currentUserId, isAdmin);
        await VerifyConcurrencyTokenAsync(recipe, concurrencyToken, cancellationToken);

        // API-17: Server cấp StepNumber tự động (max + 1), bỏ qua số client gửi lên
        var maxStep = await _context.RecipeSteps
            .Where(s => s.RecipeId == recipeId && !s.IsDeleted)
            .Select(s => (int?)s.StepNumber)
            .MaxAsync(cancellationToken);

        int targetStepNumber = (maxStep ?? 0) + 1;

        var stepTitle = !string.IsNullOrWhiteSpace(request.Title)
            ? request.Title.Trim()
            : $"Bước {targetStepNumber}";

        var step = new RecipeStep
        {
            Id = Guid.NewGuid(),
            RecipeId = recipeId,
            StepNumber = targetStepNumber,
            Title = stepTitle,
            Description = request.Description.Trim(),
            DurationMinutes = request.DurationMinutes,
            ImageUrl = request.ImageUrl?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.RecipeSteps.Add(step);
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // API-08: Hủy cache Recipe/Search
        if (_cacheInvalidator != null)
        {
            await _cacheInvalidator.InvalidateRecipeAsync(recipeId, recipe.Slug, cancellationToken);
        }

        return MapToDto(step);
    }

    /// <inheritdoc />
    public async Task<RecipeStepDto> UpdateStepAsync(
        Guid recipeId,
        Guid stepId,
        UpdateRecipeStepRequest request,
        string? currentUserId,
        bool isAdmin,
        uint? concurrencyToken = null,
        CancellationToken cancellationToken = default)
    {
        RecipeStepValidator.ValidateUpdate(request);

        var recipe = await _context.Recipes
            .FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);

        if (recipe == null)
        {
            throw new NotFoundException(nameof(Recipe), recipeId, "RECIPE_NOT_FOUND");
        }

        VerifyRecipeOwnerOrAdmin(recipe, currentUserId, isAdmin);
        await VerifyConcurrencyTokenAsync(recipe, concurrencyToken, cancellationToken);

        var step = await _context.RecipeSteps
            .FirstOrDefaultAsync(s => s.Id == stepId && s.RecipeId == recipeId && !s.IsDeleted, cancellationToken);

        if (step == null)
        {
            throw new NotFoundException(nameof(RecipeStep), stepId, "STEP_NOT_FOUND");
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            step.Title = request.Title.Trim();
        }

        step.Description = request.Description.Trim();
        step.DurationMinutes = request.DurationMinutes;
        step.ImageUrl = request.ImageUrl?.Trim();
        step.UpdatedAt = DateTimeOffset.UtcNow;
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // API-08: Hủy cache Recipe/Search
        if (_cacheInvalidator != null)
        {
            await _cacheInvalidator.InvalidateRecipeAsync(recipeId, recipe.Slug, cancellationToken);
        }

        return MapToDto(step);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RecipeStepDto>> ReorderStepsAsync(
        Guid recipeId,
        ReorderRecipeStepsRequest request,
        string? currentUserId,
        bool isAdmin,
        uint? concurrencyToken = null,
        CancellationToken cancellationToken = default)
    {
        RecipeStepValidator.ValidateReorder(request);

        var recipe = await _context.Recipes
            .FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);

        if (recipe == null)
        {
            throw new NotFoundException(nameof(Recipe), recipeId, "RECIPE_NOT_FOUND");
        }

        VerifyRecipeOwnerOrAdmin(recipe, currentUserId, isAdmin);
        await VerifyConcurrencyTokenAsync(recipe, concurrencyToken, cancellationToken);

        var existingSteps = await _context.RecipeSteps
            .Where(s => s.RecipeId == recipeId && !s.IsDeleted)
            .ToListAsync(cancellationToken);

        if (existingSteps.Count != request.StepIds.Count)
        {
            throw new BadRequestException(
                $"Số lượng bước trong yêu cầu ({request.StepIds.Count}) không khớp với số lượng bước thực tế của công thức ({existingSteps.Count}).",
                "INVALID_STEP_COUNT");
        }

        var existingStepIds = existingSteps.Select(s => s.Id).ToHashSet();
        if (!request.StepIds.All(id => existingStepIds.Contains(id)))
        {
            throw new BadRequestException("Danh sách sắp xếp phải chứa đúng toàn bộ các bước đang có của công thức.", "INVALID_STEP_IDS");
        }

        await using var transaction = _context is DbContext db && db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(cancellationToken) : null;

        // Hai giai đoạn trong cùng transaction để không để lại số âm khi request thất bại.
        for (int i = 0; i < existingSteps.Count; i++)
        {
            existingSteps[i].StepNumber = -(i + 1);
        }
        await _context.SaveChangesAsync(cancellationToken);

        // Giai đoạn 2: Gán lại StepNumber dương chuẩn từ 1..N theo đúng thứ tự mảng mới
        for (int i = 0; i < request.StepIds.Count; i++)
        {
            var targetId = request.StepIds[i];
            var step = existingSteps.First(s => s.Id == targetId);
            step.StepNumber = i + 1;
            step.UpdatedAt = DateTimeOffset.UtcNow;
        }

        recipe.UpdatedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        // API-08: Hủy cache Recipe/Search
        if (transaction != null) await transaction.CommitAsync(cancellationToken);
        if (_cacheInvalidator != null)
        {
            await _cacheInvalidator.InvalidateRecipeAsync(recipeId, recipe.Slug, cancellationToken);
        }

        return existingSteps
            .OrderBy(s => s.StepNumber)
            .Select(s => MapToDto(s))
            .ToList();
    }

    /// <inheritdoc />
    public async Task DeleteStepAsync(
        Guid recipeId,
        Guid stepId,
        string? currentUserId,
        bool isAdmin,
        uint? concurrencyToken = null,
        CancellationToken cancellationToken = default)
    {
        var recipe = await _context.Recipes
            .FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);

        if (recipe == null)
        {
            throw new NotFoundException(nameof(Recipe), recipeId, "RECIPE_NOT_FOUND");
        }

        VerifyRecipeOwnerOrAdmin(recipe, currentUserId, isAdmin);
        await VerifyConcurrencyTokenAsync(recipe, concurrencyToken, cancellationToken);

        var stepToDelete = await _context.RecipeSteps
            .FirstOrDefaultAsync(s => s.Id == stepId && s.RecipeId == recipeId && !s.IsDeleted, cancellationToken);

        if (stepToDelete == null)
        {
            throw new NotFoundException(nameof(RecipeStep), stepId, "STEP_NOT_FOUND");
        }

        await using var transaction = _context is DbContext db && db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(cancellationToken) : null;

        // Xóa mềm theo quy chuẩn BaseEntity
        stepToDelete.IsDeleted = true;
        stepToDelete.DeletedAt = DateTimeOffset.UtcNow;
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        // Renumber các bước còn lại liên tục từ 1..N qua 2 giai đoạn an toàn
        var remainingSteps = await _context.RecipeSteps
            .Where(s => s.RecipeId == recipeId && s.Id != stepId && !s.IsDeleted)
            .OrderBy(s => s.StepNumber)
            .ToListAsync(cancellationToken);

        for (int i = 0; i < remainingSteps.Count; i++)
        {
            remainingSteps[i].StepNumber = -(i + 1);
        }
        await _context.SaveChangesAsync(cancellationToken);

        for (int i = 0; i < remainingSteps.Count; i++)
        {
            remainingSteps[i].StepNumber = i + 1;
            remainingSteps[i].UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        if (transaction != null) await transaction.CommitAsync(cancellationToken);
        // API-08: Hủy cache Recipe/Search
        if (_cacheInvalidator != null)
        {
            await _cacheInvalidator.InvalidateRecipeAsync(recipeId, recipe.Slug, cancellationToken);
        }
    }

    private static void VerifyRecipeOwnerOrAdmin(Recipe recipe, string? currentUserId, bool isAdmin)
    {
        if (isAdmin)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            throw new UnauthorizedException("Vui lòng đăng nhập để thao tác nội dung công thức.", "AUTH_UNAUTHORIZED");
        }

        if (!string.Equals(recipe.AuthorId, currentUserId, StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException("Bạn không có quyền truy cập hoặc chỉnh sửa nội dung công thức của tác giả khác.", "RECIPE_FORBIDDEN");
        }
    }

    private async Task VerifyConcurrencyTokenAsync(Recipe recipe, uint? expectedConcurrencyToken, CancellationToken ct)
    {
        if (expectedConcurrencyToken.HasValue)
        {
            var currentXmin = await _context.Recipes
                .Where(r => r.Id == recipe.Id)
                .Select(r => EF.Property<uint>(r, "xmin"))
                .FirstOrDefaultAsync(ct);

            if (currentXmin != 0 && expectedConcurrencyToken.Value != currentXmin)
            {
                throw new ConflictException("Dữ liệu công thức đã bị thay đổi bởi phiên làm việc khác. Vui lòng tải lại trang.", "RECIPE_CONCURRENCY_CONFLICT");
            }
        }
    }

    private static RecipeStepDto MapToDto(RecipeStep step)
    {
        return new RecipeStepDto
        {
            Id = step.Id,
            RecipeId = step.RecipeId,
            StepNumber = step.StepNumber,
            Title = step.Title,
            Description = step.Description,
            DurationMinutes = step.DurationMinutes,
            ImageUrl = step.ImageUrl,
            CreatedAt = step.CreatedAt,
            UpdatedAt = step.UpdatedAt
        };
    }
}
