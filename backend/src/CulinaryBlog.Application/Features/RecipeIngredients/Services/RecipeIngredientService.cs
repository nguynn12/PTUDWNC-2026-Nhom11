namespace CulinaryBlog.Application.Features.RecipeIngredients.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.RecipeIngredients.DTOs;
using CulinaryBlog.Application.Features.RecipeIngredients.Validators;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Hiện thực nghiệp vụ quản lý nguyên liệu công thức nấu ăn.
/// Tuân thủ Clean Architecture, sử dụng IApplicationDbContext, kiểm soát phân quyền tác giả (SEC-01, SEC-02),
/// kiểm soát concurrency qua xmin của Recipe (API-03) và hủy cache Redis khi có thay đổi (API-08).
/// </summary>
public class RecipeIngredientService : IRecipeIngredientService
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheInvalidator? _cacheInvalidator;

    public RecipeIngredientService(
        IApplicationDbContext context,
        ICacheInvalidator? cacheInvalidator = null)
    {
        _context = context;
        _cacheInvalidator = cacheInvalidator;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RecipeIngredientDto>> GetIngredientsAsync(
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
                throw new ForbiddenException("Bạn không có quyền xem nguyên liệu của công thức chưa xuất bản.");
            }
        }

        var ingredients = await _context.RecipeIngredients
            .AsNoTracking()
            .Where(i => i.RecipeId == recipeId && !i.IsDeleted)
            .OrderBy(i => i.OrderIndex)
            .Select(i => MapToDto(i))
            .ToListAsync(cancellationToken);

        return ingredients;
    }

    /// <inheritdoc />
    public async Task<RecipeIngredientDto> GetIngredientByIdAsync(
        Guid recipeId,
        Guid ingredientId,
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
                throw new ForbiddenException("Bạn không có quyền xem nguyên liệu của công thức chưa xuất bản.");
            }
        }

        var ingredient = await _context.RecipeIngredients
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == ingredientId && i.RecipeId == recipeId && !i.IsDeleted, cancellationToken);

        if (ingredient == null)
        {
            throw new NotFoundException(nameof(RecipeIngredient), ingredientId, "INGREDIENT_NOT_FOUND");
        }

        return MapToDto(ingredient);
    }

    /// <inheritdoc />
    public async Task<RecipeIngredientDto> AddIngredientAsync(
        Guid recipeId,
        CreateRecipeIngredientRequest request,
        string? currentUserId,
        bool isAdmin,
        uint? concurrencyToken = null,
        CancellationToken cancellationToken = default)
    {
        RecipeIngredientValidator.ValidateCreate(request);

        var recipe = await _context.Recipes
            .FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);

        if (recipe == null)
        {
            throw new NotFoundException(nameof(Recipe), recipeId, "RECIPE_NOT_FOUND");
        }

        VerifyRecipeOwnerOrAdmin(recipe, currentUserId, isAdmin);
        await VerifyConcurrencyTokenAsync(recipe, concurrencyToken, cancellationToken);

        int orderIndex;
        if (request.OrderIndex.HasValue)
        {
            orderIndex = request.OrderIndex.Value;
        }
        else
        {
            var maxOrder = await _context.RecipeIngredients
                .Where(i => i.RecipeId == recipeId && !i.IsDeleted)
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
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // API-08: Hủy cache Recipe/Search khi thêm nguyên liệu
        if (_cacheInvalidator != null)
        {
            await _cacheInvalidator.InvalidateRecipeAsync(recipeId, recipe.Slug, cancellationToken);
        }

        return MapToDto(ingredient);
    }

    /// <inheritdoc />
    public async Task<RecipeIngredientDto> UpdateIngredientAsync(
        Guid recipeId,
        Guid ingredientId,
        UpdateRecipeIngredientRequest request,
        string? currentUserId,
        bool isAdmin,
        uint? concurrencyToken = null,
        CancellationToken cancellationToken = default)
    {
        RecipeIngredientValidator.ValidateUpdate(request);

        var recipe = await _context.Recipes
            .FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);

        if (recipe == null)
        {
            throw new NotFoundException(nameof(Recipe), recipeId, "RECIPE_NOT_FOUND");
        }

        VerifyRecipeOwnerOrAdmin(recipe, currentUserId, isAdmin);
        await VerifyConcurrencyTokenAsync(recipe, concurrencyToken, cancellationToken);

        var ingredient = await _context.RecipeIngredients
            .FirstOrDefaultAsync(i => i.Id == ingredientId && i.RecipeId == recipeId && !i.IsDeleted, cancellationToken);

        if (ingredient == null)
        {
            throw new NotFoundException(nameof(RecipeIngredient), ingredientId, "INGREDIENT_NOT_FOUND");
        }

        ingredient.Name = request.Name.Trim();
        ingredient.Quantity = request.Quantity;
        ingredient.Unit = request.Unit?.Trim();
        ingredient.Notes = request.Notes?.Trim();
        ingredient.OrderIndex = request.OrderIndex;
        ingredient.UpdatedAt = DateTimeOffset.UtcNow;
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // API-08: Hủy cache Recipe/Search khi sửa nguyên liệu
        if (_cacheInvalidator != null)
        {
            await _cacheInvalidator.InvalidateRecipeAsync(recipeId, recipe.Slug, cancellationToken);
        }

        return MapToDto(ingredient);
    }

    /// <inheritdoc />
    public async Task DeleteIngredientAsync(
        Guid recipeId,
        Guid ingredientId,
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

        var ingredient = await _context.RecipeIngredients
            .FirstOrDefaultAsync(i => i.Id == ingredientId && i.RecipeId == recipeId && !i.IsDeleted, cancellationToken);

        if (ingredient == null)
        {
            throw new NotFoundException(nameof(RecipeIngredient), ingredientId, "INGREDIENT_NOT_FOUND");
        }

        // Xóa mềm theo quy chuẩn BaseEntity
        ingredient.IsDeleted = true;
        ingredient.DeletedAt = DateTimeOffset.UtcNow;
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // API-08: Hủy cache Recipe/Search khi xóa nguyên liệu
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
