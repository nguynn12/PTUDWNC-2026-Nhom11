namespace CulinaryBlog.Application.Features.RecipeIngredients.Services;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.RecipeIngredients.DTOs;
using CulinaryBlog.Application.Features.RecipeIngredients.Validators;
using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Hiện thực nghiệp vụ quản lý nguyên liệu công thức nấu ăn.
/// Tuân thủ Clean Architecture, sử dụng IApplicationDbContext và kiểm soát phân quyền tác giả.
/// </summary>
public class RecipeIngredientService : IRecipeIngredientService
{
    private readonly IApplicationDbContext _context;

    public RecipeIngredientService(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RecipeIngredientDto>> GetIngredientsAsync(
        Guid recipeId,
        CancellationToken cancellationToken = default)
    {
        var recipeExists = await _context.Recipes
            .AnyAsync(r => r.Id == recipeId, cancellationToken);

        if (!recipeExists)
        {
            throw new NotFoundException(nameof(Recipe), recipeId);
        }

        var ingredients = await _context.RecipeIngredients
            .AsNoTracking()
            .Where(i => i.RecipeId == recipeId)
            .OrderBy(i => i.OrderIndex)
            .Select(i => MapToDto(i))
            .ToListAsync(cancellationToken);

        return ingredients;
    }

    /// <inheritdoc />
    public async Task<RecipeIngredientDto> AddIngredientAsync(
        Guid recipeId,
        CreateRecipeIngredientRequest request,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        RecipeIngredientValidator.ValidateCreate(request);

        var recipe = await _context.Recipes
            .FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);

        if (recipe == null)
        {
            throw new NotFoundException(nameof(Recipe), recipeId);
        }

        VerifyRecipeOwnerOrAdmin(recipe, currentUserId, isAdmin);

        int orderIndex;
        if (request.OrderIndex.HasValue)
        {
            orderIndex = request.OrderIndex.Value;
        }
        else
        {
            var maxOrder = await _context.RecipeIngredients
                .Where(i => i.RecipeId == recipeId)
                .Select(i => (int?)i.OrderIndex)
                .MaxAsync(cancellationToken);

            orderIndex = (maxOrder ?? -1) + 1;
        }

        var ingredient = new RecipeIngredient
        {
            Id = Guid.NewGuid(),
            RecipeId = recipeId,
            Name = request.Name.Trim(),
            Quantity = request.Quantity,
            Unit = request.Unit?.Trim(),
            Notes = request.Notes?.Trim(),
            OrderIndex = orderIndex,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.RecipeIngredients.Add(ingredient);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(ingredient);
    }

    /// <inheritdoc />
    public async Task<RecipeIngredientDto> UpdateIngredientAsync(
        Guid recipeId,
        Guid ingredientId,
        UpdateRecipeIngredientRequest request,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        RecipeIngredientValidator.ValidateUpdate(request);

        var recipe = await _context.Recipes
            .FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);

        if (recipe == null)
        {
            throw new NotFoundException(nameof(Recipe), recipeId);
        }

        VerifyRecipeOwnerOrAdmin(recipe, currentUserId, isAdmin);

        var ingredient = await _context.RecipeIngredients
            .FirstOrDefaultAsync(i => i.Id == ingredientId && i.RecipeId == recipeId, cancellationToken);

        if (ingredient == null)
        {
            throw new NotFoundException(nameof(RecipeIngredient), ingredientId);
        }

        ingredient.Name = request.Name.Trim();
        ingredient.Quantity = request.Quantity;
        ingredient.Unit = request.Unit?.Trim();
        ingredient.Notes = request.Notes?.Trim();
        ingredient.OrderIndex = request.OrderIndex;
        ingredient.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(ingredient);
    }

    /// <inheritdoc />
    public async Task DeleteIngredientAsync(
        Guid recipeId,
        Guid ingredientId,
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

        var ingredient = await _context.RecipeIngredients
            .FirstOrDefaultAsync(i => i.Id == ingredientId && i.RecipeId == recipeId, cancellationToken);

        if (ingredient == null)
        {
            throw new NotFoundException(nameof(RecipeIngredient), ingredientId);
        }

        // Xóa mềm theo quy chuẩn BaseEntity
        ingredient.IsDeleted = true;
        ingredient.DeletedAt = DateTimeOffset.UtcNow;

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

    private static RecipeIngredientDto MapToDto(RecipeIngredient ingredient)
    {
        return new RecipeIngredientDto
        {
            Id = ingredient.Id,
            RecipeId = ingredient.RecipeId,
            Name = ingredient.Name,
            Quantity = ingredient.Quantity,
            Unit = ingredient.Unit,
            Notes = ingredient.Notes,
            OrderIndex = ingredient.OrderIndex,
            CreatedAt = ingredient.CreatedAt,
            UpdatedAt = ingredient.UpdatedAt
        };
    }
}
