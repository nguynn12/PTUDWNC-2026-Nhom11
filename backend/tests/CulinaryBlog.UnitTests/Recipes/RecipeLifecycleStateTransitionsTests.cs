namespace CulinaryBlog.UnitTests.Recipes;

using System;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using Xunit;

/// <summary>
/// Unit Tests kiểm tra tính toàn vẹn của mô hình Recipe, quy tắc chuyển đổi trạng thái và mã lỗi Domain Exceptions.
/// </summary>
public class RecipeLifecycleStateTransitionsTests
{
    [Fact]
    public void Recipe_ShouldInitializeWithDefaultDraftStatus_AndNotDeleted()
    {
        // Act
        var recipe = new Recipe
        {
            Title = "Bánh Mì Kẹp Thịt",
            Slug = "banh-mi-kep-thit",
            Description = "Bánh mì Việt Nam giòn rụm",
            PrepTimeMinutes = 10,
            CookTimeMinutes = 5,
            Servings = 1,
            Difficulty = RecipeDifficulty.Easy,
            CategoryId = Guid.NewGuid(),
            AuthorId = "author-001"
        };

        // Assert
        Assert.Equal(RecipeStatus.Draft, recipe.Status);
        Assert.Null(recipe.PublishedAt);
        Assert.False(recipe.IsDeleted);
        Assert.Null(recipe.DeletedAt);
        Assert.Null(recipe.UpdatedAt);
    }

    [Fact]
    public void Recipe_SoftDelete_ShouldSetIsDeletedAndDeletedAt()
    {
        // Arrange
        var recipe = new Recipe
        {
            Title = "Chả Giò Rán",
            Slug = "cha-gio-ran",
            Status = RecipeStatus.Draft
        };

        // Act
        var now = DateTimeOffset.UtcNow;
        recipe.IsDeleted = true;
        recipe.DeletedAt = now;
        recipe.UpdatedAt = now;

        // Assert
        Assert.True(recipe.IsDeleted);
        Assert.NotNull(recipe.DeletedAt);
        Assert.Equal(now, recipe.DeletedAt);
    }

    [Fact]
    public void BusinessRuleValidationException_ShouldCarryErrorCode_ForIncompletePublish()
    {
        // Arrange & Act
        var ex = new BusinessRuleValidationException(
            "Công thức cần có ít nhất 1 nguyên liệu và 1 bước thực hiện để được xuất bản.",
            "RECIPE_PUBLISH_INCOMPLETE");

        // Assert
        Assert.Equal("RECIPE_PUBLISH_INCOMPLETE", ex.ErrorCode);
    }

    [Fact]
    public void ForbiddenException_ShouldCarryErrorCode_ForOwnershipViolation()
    {
        // Arrange & Act
        var ex = new ForbiddenException(
            "Bạn không có quyền chỉnh sửa công thức của người khác.",
            "RECIPE_FORBIDDEN");

        // Assert
        Assert.Equal("RECIPE_FORBIDDEN", ex.ErrorCode);
    }

    [Fact]
    public void ConflictException_ShouldCarryErrorCode_ForConcurrencyConflict()
    {
        // Arrange & Act
        var ex = new ConflictException(
            "Dữ liệu công thức đã bị thay đổi bởi phiên làm việc khác.",
            "RECIPE_CONCURRENCY_CONFLICT");

        // Assert
        Assert.Equal("RECIPE_CONCURRENCY_CONFLICT", ex.ErrorCode);
    }

    [Fact]
    public void RecipeSlugHistory_ShouldRecordOldSlugAndRecipeId()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        const string oldSlug = "pho-bo-cu";

        // Act
        var history = new RecipeSlugHistory
        {
            RecipeId = recipeId,
            OldSlug = oldSlug,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Assert
        Assert.Equal(recipeId, history.RecipeId);
        Assert.Equal(oldSlug, history.OldSlug);
        Assert.False(history.IsDeleted);
    }
}
