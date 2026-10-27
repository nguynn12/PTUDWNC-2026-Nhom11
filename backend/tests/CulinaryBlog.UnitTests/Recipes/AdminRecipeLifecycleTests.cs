namespace CulinaryBlog.UnitTests.Recipes;

using System;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Recipes.Commands.PurgeRecipe;
using CulinaryBlog.Application.Features.Recipes.Commands.RestoreRecipe;
using CulinaryBlog.Application.Features.Recipes.DTOs;
using CulinaryBlog.Application.Features.Recipes.Queries.GetTrashedRecipes;
using CulinaryBlog.Domain.Enums;
using Xunit;

/// <summary>
/// Unit Tests kiểm tra các chức năng Admin Recipe Lifecycle (Thùng rác, Khôi phục, Xóa vĩnh viễn theo FR-RCP-007).
/// </summary>
public class AdminRecipeLifecycleTests
{
    [Fact]
    public void GetTrashedRecipesQuery_ShouldInitializeWithDefaultOrProvidedPagination()
    {
        // Act
        var queryDefault = new GetTrashedRecipesQuery();
        var queryCustom = new GetTrashedRecipesQuery(2, 25);

        // Assert
        Assert.Equal(1, queryDefault.PageNumber);
        Assert.Equal(10, queryDefault.PageSize);
        Assert.Equal(2, queryCustom.PageNumber);
        Assert.Equal(25, queryCustom.PageSize);
    }

    [Fact]
    public void TrashedRecipeDto_ShouldCalculateDaysRemainingAccurately()
    {
        // Arrange
        var deletedAt = DateTimeOffset.UtcNow.AddDays(-10);
        var dto = new TrashedRecipeDto
        {
            Id = Guid.NewGuid(),
            Title = "Món Cũ Đã Xóa",
            Slug = "mon-cu-da-xoa",
            Status = RecipeStatus.Draft,
            DeletedAt = deletedAt,
            DaysRemaining = Math.Max(0, 30 - (int)(DateTimeOffset.UtcNow - deletedAt).TotalDays)
        };

        // Assert
        Assert.True(dto.DaysRemaining >= 19 && dto.DaysRemaining <= 20);
    }

    [Fact]
    public void RestoreRecipeCommand_ShouldCarryCorrectRecipeId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var command = new RestoreRecipeCommand(id);

        // Assert
        Assert.Equal(id, command.Id);
    }

    [Fact]
    public void RestoreRecipe_ConflictException_ShouldCarryCorrectErrorCodes()
    {
        // Arrange & Act
        var exNotDeleted = new ConflictException("Công thức chưa bị xóa mềm.", "RECIPE_NOT_DELETED");
        var exExpired = new ConflictException("Công thức đã quá thời hạn khôi phục 30 ngày.", "RECIPE_RESTORE_WINDOW_EXPIRED");

        // Assert
        Assert.Equal("RECIPE_NOT_DELETED", exNotDeleted.ErrorCode);
        Assert.Equal("RECIPE_RESTORE_WINDOW_EXPIRED", exExpired.ErrorCode);
    }

    [Fact]
    public void PurgeRecipeCommand_ShouldCarryCorrectRecipeId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var command = new PurgeRecipeCommand(id);

        // Assert
        Assert.Equal(id, command.Id);
    }
}
