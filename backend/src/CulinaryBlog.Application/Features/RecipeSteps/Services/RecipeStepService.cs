namespace CulinaryBlog.Application.Features.RecipeSteps.Services;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.RecipeSteps.DTOs;
using CulinaryBlog.Application.Features.RecipeSteps.Validators;
using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Hiện thực nghiệp vụ quản lý các bước thực hiện công thức nấu ăn.
/// Tuân thủ SRS v1.2.0 (FR-RCP-010), RESOLVED-CONFLICTS.md (E5, E8)
/// và tự động duy trì tính liên tục của StepNumber trong transaction.
/// </summary>
public class RecipeStepService : IRecipeStepService
{
    private readonly IApplicationDbContext _context;

    public RecipeStepService(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RecipeStepDto>> GetStepsAsync(
        Guid recipeId,
        CancellationToken cancellationToken = default)
    {
        var recipeExists = await _context.Recipes
            .AnyAsync(r => r.Id == recipeId, cancellationToken);

        if (!recipeExists)
        {
            throw new NotFoundException(nameof(Recipe), recipeId);
        }

        var steps = await _context.RecipeSteps
            .AsNoTracking()
            .Where(s => s.RecipeId == recipeId)
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
        CancellationToken cancellationToken = default)
    {
        RecipeStepValidator.ValidateCreate(request);

        var recipe = await _context.Recipes
            .FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);

        if (recipe == null)
        {
            throw new NotFoundException(nameof(Recipe), recipeId);
        }

        VerifyRecipeOwnerOrAdmin(recipe, currentUserId, isAdmin);

        int targetStepNumber;
        if (request.StepNumber.HasValue && request.StepNumber.Value > 0)
        {
            targetStepNumber = request.StepNumber.Value;

            // Nếu số bước yêu cầu chen vào giữa danh sách hiện có, dịch chuyển các bước phía sau lên 1 vị trí
            var stepsToShift = await _context.RecipeSteps
                .Where(s => s.RecipeId == recipeId && s.StepNumber >= targetStepNumber)
                .OrderByDescending(s => s.StepNumber)
                .ToListAsync(cancellationToken);

            foreach (var existingStep in stepsToShift)
            {
                existingStep.StepNumber += 1;
                existingStep.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }
        else
        {
            var maxStep = await _context.RecipeSteps
                .Where(s => s.RecipeId == recipeId)
                .Select(s => (int?)s.StepNumber)
                .MaxAsync(cancellationToken);

            targetStepNumber = (maxStep ?? 0) + 1;
        }

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
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(step);
    }

    /// <inheritdoc />
    public async Task<RecipeStepDto> UpdateStepAsync(
        Guid recipeId,
        Guid stepId,
        UpdateRecipeStepRequest request,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        RecipeStepValidator.ValidateUpdate(request);

        var recipe = await _context.Recipes
            .FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);

        if (recipe == null)
        {
            throw new NotFoundException(nameof(Recipe), recipeId);
        }

        VerifyRecipeOwnerOrAdmin(recipe, currentUserId, isAdmin);

        var step = await _context.RecipeSteps
            .FirstOrDefaultAsync(s => s.Id == stepId && s.RecipeId == recipeId, cancellationToken);

        if (step == null)
        {
            throw new NotFoundException(nameof(RecipeStep), stepId);
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            step.Title = request.Title.Trim();
        }

        step.Description = request.Description.Trim();
        step.DurationMinutes = request.DurationMinutes;
        step.ImageUrl = request.ImageUrl?.Trim();
        step.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(step);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RecipeStepDto>> ReorderStepsAsync(
        Guid recipeId,
        ReorderRecipeStepsRequest request,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        RecipeStepValidator.ValidateReorder(request);

        var recipe = await _context.Recipes
            .FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);

        if (recipe == null)
        {
            throw new NotFoundException(nameof(Recipe), recipeId);
        }

        VerifyRecipeOwnerOrAdmin(recipe, currentUserId, isAdmin);

        var existingSteps = await _context.RecipeSteps
            .Where(s => s.RecipeId == recipeId)
            .ToListAsync(cancellationToken);

        if (existingSteps.Count != request.StepIds.Count)
        {
            throw new BadRequestException(
                $"Số lượng bước trong yêu cầu ({request.StepIds.Count}) không khớp với số lượng bước thực tế của công thức ({existingSteps.Count}).");
        }

        var existingStepIds = existingSteps.Select(s => s.Id).ToHashSet();
        if (!request.StepIds.All(id => existingStepIds.Contains(id)))
        {
            throw new BadRequestException("Danh sách sắp xếp phải chứa đúng toàn bộ các bước đang có của công thức.");
        }

        // Tự động gán lại StepNumber liên tục từ 1..N theo thứ tự mảng mới
        for (int i = 0; i < request.StepIds.Count; i++)
        {
            var targetId = request.StepIds[i];
            var step = existingSteps.First(s => s.Id == targetId);
            step.StepNumber = i + 1;
            step.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

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
        CancellationToken cancellationToken = default)
    {
        var recipe = await _context.Recipes
            .FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);

        if (recipe == null)
        {
            throw new NotFoundException(nameof(Recipe), recipeId);
        }

        VerifyRecipeOwnerOrAdmin(recipe, currentUserId, isAdmin);

        var stepToDelete = await _context.RecipeSteps
            .FirstOrDefaultAsync(s => s.Id == stepId && s.RecipeId == recipeId, cancellationToken);

        if (stepToDelete == null)
        {
            throw new NotFoundException(nameof(RecipeStep), stepId);
        }

        // Xóa mềm theo quy chuẩn BaseEntity
        stepToDelete.IsDeleted = true;
        stepToDelete.DeletedAt = DateTimeOffset.UtcNow;

        // Tự động đánh lại số thứ tự (renumber) các bước còn lại liên tục từ 1..N trong transaction (FR-RCP-010)
        var remainingSteps = await _context.RecipeSteps
            .Where(s => s.RecipeId == recipeId && s.Id != stepId)
            .OrderBy(s => s.StepNumber)
            .ToListAsync(cancellationToken);

        for (int i = 0; i < remainingSteps.Count; i++)
        {
            remainingSteps[i].StepNumber = i + 1;
            remainingSteps[i].UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static void VerifyRecipeOwnerOrAdmin(Recipe recipe, string? currentUserId, bool isAdmin)
    {
        if (isAdmin)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(currentUserId) ||
            !string.Equals(recipe.AuthorId, currentUserId, StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException("Bạn không có quyền chỉnh sửa nội dung công thức của tác giả khác.");
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
