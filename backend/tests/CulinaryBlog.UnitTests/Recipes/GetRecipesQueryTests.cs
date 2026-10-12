namespace CulinaryBlog.UnitTests.Recipes;

using System;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes.Queries.GetRecipes;
using CulinaryBlog.Domain.Enums;
using Xunit;

/// <summary>
/// Unit Tests kiểm tra tính toàn vẹn và các quy tắc của GetRecipesQuery và PagedResult theo FR-SRCH-002/003/004.
/// </summary>
public class GetRecipesQueryTests
{
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
            SortOrder: "asc");

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
