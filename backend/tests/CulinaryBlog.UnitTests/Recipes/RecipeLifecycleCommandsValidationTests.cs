namespace CulinaryBlog.UnitTests.Recipes;

using System;
using CulinaryBlog.Application.Features.Recipes.Commands.CreateRecipe;
using CulinaryBlog.Application.Features.Recipes.Commands.UpdateRecipe;
using CulinaryBlog.Domain.Enums;
using Xunit;

/// <summary>
/// Unit Tests kiểm tra tính hợp lệ của dữ liệu đầu vào cho các Command trong Recipe Lifecycle.
/// </summary>
public class RecipeLifecycleCommandsValidationTests
{
    private readonly CreateRecipeCommandValidator _createValidator = new();
    private readonly UpdateRecipeCommandValidator _updateValidator = new();

    [Fact]
    public void CreateRecipeCommandValidator_ShouldPass_WhenAllFieldsAreValid()
    {
        // Arrange
        var command = new CreateRecipeCommand
        {
            Title = "Cơm Tấm Sườn Bì Chả",
            Description = "Món ăn đặc sản Sài Gòn nức tiếng",
            PrepTimeMinutes = 20,
            CookTimeMinutes = 30,
            Servings = 2,
            Difficulty = RecipeDifficulty.Medium,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _createValidator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateRecipeCommandValidator_ShouldFail_WhenTitleIsEmpty(string title)
    {
        // Arrange
        var command = new CreateRecipeCommand
        {
            Title = title,
            Description = "Mô tả chi tiết món ăn thơm ngon",
            PrepTimeMinutes = 15,
            CookTimeMinutes = 30,
            Servings = 4,
            Difficulty = RecipeDifficulty.Easy,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _createValidator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRecipeCommand.Title));
    }

    [Fact]
    public void CreateRecipeCommandValidator_ShouldFail_WhenTitleIsShorterThan3Characters()
    {
        // Arrange (D6: Tiêu đề 3 - 200 ký tự)
        var command = new CreateRecipeCommand
        {
            Title = "Ab",
            Description = "Mô tả chi tiết món ăn thơm ngon",
            PrepTimeMinutes = 15,
            CookTimeMinutes = 30,
            Servings = 4,
            Difficulty = RecipeDifficulty.Easy,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _createValidator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRecipeCommand.Title));
    }

    [Fact]
    public void CreateRecipeCommandValidator_ShouldFail_WhenTitleExceeds200Characters()
    {
        // Arrange (D6: Tiêu đề 3 - 200 ký tự)
        var command = new CreateRecipeCommand
        {
            Title = new string('A', 201),
            Description = "Mô tả chi tiết món ăn thơm ngon",
            PrepTimeMinutes = 15,
            CookTimeMinutes = 30,
            Servings = 4,
            Difficulty = RecipeDifficulty.Easy,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _createValidator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRecipeCommand.Title));
    }

    [Fact]
    public void CreateRecipeCommandValidator_ShouldFail_WhenDescriptionIsShorterThan10Characters()
    {
        // Arrange (D6: Mô tả 10 - 2000 ký tự)
        var command = new CreateRecipeCommand
        {
            Title = "Phở Bò",
            Description = "Quá ngắn",
            PrepTimeMinutes = 15,
            CookTimeMinutes = 30,
            Servings = 4,
            Difficulty = RecipeDifficulty.Easy,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _createValidator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRecipeCommand.Description));
    }

    [Fact]
    public void CreateRecipeCommandValidator_ShouldFail_WhenDescriptionExceeds2000Characters()
    {
        // Arrange (D6: Mô tả 10 - 2000 ký tự)
        var command = new CreateRecipeCommand
        {
            Title = "Phở Bò",
            Description = new string('D', 2001),
            PrepTimeMinutes = 15,
            CookTimeMinutes = 30,
            Servings = 4,
            Difficulty = RecipeDifficulty.Easy,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _createValidator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRecipeCommand.Description));
    }

    [Fact]
    public void CreateRecipeCommandValidator_ShouldPass_WhenDescriptionIs2000Characters()
    {
        // Arrange (D6: Cho phép mô tả dài tới 2000 ký tự)
        var command = new CreateRecipeCommand
        {
            Title = "Món Ăn Chi Tiết",
            Description = new string('X', 2000),
            PrepTimeMinutes = 15,
            CookTimeMinutes = 30,
            Servings = 4,
            Difficulty = RecipeDifficulty.Easy,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _createValidator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateRecipeCommandValidator_ShouldFail_WhenPrepTimeIsZeroOrNegative()
    {
        // Arrange (E4: PrepTimeMinutes bắt buộc > 0)
        var command = new CreateRecipeCommand
        {
            Title = "Gỏi Cuốn",
            Description = "Món khai vị thanh mát",
            PrepTimeMinutes = 0,
            CookTimeMinutes = 0,
            Servings = 2,
            Difficulty = RecipeDifficulty.Easy,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _createValidator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRecipeCommand.PrepTimeMinutes));
    }

    [Fact]
    public void CreateRecipeCommandValidator_ShouldAllowZeroCookTime_ForNoCookRecipes()
    {
        // Arrange (E4: CookTimeMinutes >= 0, cho phép 0 với món không nấu như salad/sinh tố)
        var command = new CreateRecipeCommand
        {
            Title = "Salad Hoa Quả Sốt Mayonnaise",
            Description = "Món tráng miệng tươi ngon",
            PrepTimeMinutes = 10,
            CookTimeMinutes = 0,
            Servings = 2,
            Difficulty = RecipeDifficulty.Easy,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _createValidator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateRecipeCommandValidator_ShouldFail_WhenCookTimeIsNegative()
    {
        // Arrange
        var command = new CreateRecipeCommand
        {
            Title = "Món Lỗi Thời Gian",
            Description = "Mô tả chi tiết món ăn thơm ngon",
            PrepTimeMinutes = 10,
            CookTimeMinutes = -5,
            Servings = 2,
            Difficulty = RecipeDifficulty.Easy,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _createValidator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRecipeCommand.CookTimeMinutes));
    }

    [Fact]
    public void CreateRecipeCommandValidator_ShouldFail_WhenCategoryIdIsEmpty()
    {
        // Arrange
        var command = new CreateRecipeCommand
        {
            Title = "Món Ăn Không Danh Mục",
            Description = "Mô tả chi tiết món ăn thơm ngon",
            PrepTimeMinutes = 15,
            CookTimeMinutes = 20,
            Servings = 2,
            Difficulty = RecipeDifficulty.Easy,
            CategoryId = Guid.Empty
        };

        // Act
        var result = _createValidator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRecipeCommand.CategoryId));
    }

    [Fact]
    public void UpdateRecipeCommandValidator_ShouldFail_WhenIdIsEmpty()
    {
        // Arrange
        var command = new UpdateRecipeCommand
        {
            Id = Guid.Empty,
            Title = "Tiêu đề cập nhật",
            Description = "Mô tả chi tiết món ăn thơm ngon",
            PrepTimeMinutes = 10,
            CookTimeMinutes = 20,
            Servings = 2,
            Difficulty = RecipeDifficulty.Easy,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _updateValidator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateRecipeCommand.Id));
    }
}
