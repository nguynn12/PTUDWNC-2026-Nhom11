namespace CulinaryBlog.UnitTests.Recipes;

using System;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes.Queries.GetRecipes;
using CulinaryBlog.Domain.Enums;
using FluentValidation.TestHelper;
using Xunit;

/// <summary>
/// Unit Tests kiểm tra tính toàn vẹn và các quy tắc của GetRecipesQuery và GetRecipesQueryValidator theo FR-SRCH-002/003/004 và API-11/12/14.
/// </summary>
public class GetRecipesQueryTests
{
    private readonly GetRecipesQueryValidator _validator = new();

    [Fact]
    public void GetRecipesQuery_ShouldInitializeWithDefaultValues()
    {
        // Act
        var query = new GetRecipesQuery();

        // Assert
        Assert.Equal(1, query.Page);
        Assert.Equal(12, query.PageSize);
        Assert.Null(query.CategoryId);
        Assert.Null(query.Difficulty);
        Assert.Null(query.MaxPrepTime);
        Assert.Null(query.MaxCookTime);
        Assert.Null(query.MaxTotalTime);
        Assert.Null(query.MinCalories);
        Assert.Null(query.MaxCalories);
        Assert.Null(query.SortBy);
        Assert.Null(query.SortOrder);
        Assert.Null(query.Mine);
        Assert.Null(query.Status);
        Assert.Null(query.AuthorId);
    }

    [Fact]
    public void GetRecipesQuery_ShouldAcceptAllFilterAndSortingParameters()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        // Act
        var query = new GetRecipesQuery(
            Page: 2,
            PageSize: 20,
            CategoryId: categoryId,
            Difficulty: RecipeDifficulty.Medium,
            MaxPrepTime: 15,
            MaxCookTime: 45,
            MaxTotalTime: 60,
            MinCalories: 200m,
            MaxCalories: 600m,
            SortBy: "totalTime",
            SortOrder: "asc",
            Mine: true,
            Status: RecipeStatus.Draft,
            AuthorId: "author-123");

        // Assert
        Assert.Equal(2, query.Page);
        Assert.Equal(20, query.PageSize);
        Assert.Equal(categoryId, query.CategoryId);
        Assert.Equal(RecipeDifficulty.Medium, query.Difficulty);
        Assert.Equal(15, query.MaxPrepTime);
        Assert.Equal(45, query.MaxCookTime);
        Assert.Equal(60, query.MaxTotalTime);
        Assert.Equal(200m, query.MinCalories);
        Assert.Equal(600m, query.MaxCalories);
        Assert.Equal("totalTime", query.SortBy);
        Assert.Equal("asc", query.SortOrder);
        Assert.True(query.Mine);
        Assert.Equal(RecipeStatus.Draft, query.Status);
        Assert.Equal("author-123", query.AuthorId);
    }

    [Theory]
    [InlineData("publishedat")]
    [InlineData("newest")]
    [InlineData("createdat")]
    [InlineData("updatedat")]
    [InlineData("title")]
    [InlineData("preptime")]
    [InlineData("cooktime")]
    [InlineData("servings")]
    [InlineData("totaltime")]
    [InlineData("quickest")]
    [InlineData("calories")]
    public void Validator_ShouldPass_ForAllowedSortByValues(string sortBy)
    {
        // Arrange
        var query = new GetRecipesQuery(SortBy: sortBy);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SortBy);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenSortByIsInvalid()
    {
        // Arrange
        var query = new GetRecipesQuery(SortBy: "invalid_column");

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SortBy);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenDifficultyIsOutOfEnumRange()
    {
        // Arrange
        var query = new GetRecipesQuery(Difficulty: (RecipeDifficulty)99);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Difficulty)
            .WithErrorMessage("Mức độ khó không hợp lệ.");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenStatusIsOutOfEnumRange()
    {
        // Arrange
        var query = new GetRecipesQuery(Status: (RecipeStatus)99);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("Trạng thái công thức không hợp lệ.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validator_ShouldHaveError_WhenPageIsInvalid(int invalidPage)
    {
        // Arrange
        var query = new GetRecipesQuery(Page: invalidPage);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void Validator_ShouldHaveError_WhenPageSizeIsOutOfBounds(int invalidPageSize)
    {
        // Arrange
        var query = new GetRecipesQuery(PageSize: invalidPageSize);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenMinCaloriesGreaterThanMaxCalories()
    {
        // Arrange
        var query = new GetRecipesQuery(MinCalories: 800m, MaxCalories: 300m);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor("CaloriesRange");
    }

    [Theory]
    [InlineData(100, 10, 10)]
    [InlineData(105, 10, 11)]
    [InlineData(0, 10, 0)]
    [InlineData(5, 12, 1)]
    [InlineData(25, 12, 3)]
    public void PagedResult_ShouldCalculateTotalPagesCorrectly(int total, int pageSize, int expectedTotalPages)
    {
        // Act
        var pagedResult = new PagedResult<string>(Array.Empty<string>(), total, 1, pageSize);

        // Assert
        Assert.Equal(total, pagedResult.Total);
        Assert.Equal(pageSize, pagedResult.PageSize);
        Assert.Equal(expectedTotalPages, pagedResult.TotalPages);
    }
}
