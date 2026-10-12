namespace CulinaryBlog.UnitTests.Recipes;

using System;
using System.Collections.Generic;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Categories.DTOs;
using CulinaryBlog.Application.Features.Recipes.DTOs;
using CulinaryBlog.Application.Features.Recipes.Queries.GetRecipeBySlug;
using Xunit;

/// <summary>
/// Unit Tests kiểm tra tính toàn vẹn và các quy tắc của GetRecipeBySlugQuery và RecipeDetailDto theo FR-RCP-002.
/// </summary>
public class GetRecipeBySlugQueryTests
{
    [Fact]
    public void GetRecipeBySlugQuery_ShouldInitializeWithProvidedSlug()
    {
        // Arrange
        const string expectedSlug = "pho-bo-ha-noi-truyen-thong";

        // Act
        var query = new GetRecipeBySlugQuery(expectedSlug);

        // Assert
        Assert.Equal(expectedSlug, query.Slug);
    }

    [Fact]
    public void RecipeDetailDto_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var publishedDate = DateTimeOffset.UtcNow;

        var dto = new RecipeDetailDto
        {
            Id = recipeId,
            Title = "Phở bò Hà Nội",
            Slug = "pho-bo-ha-noi",
            Description = "Món phở truyền thống thơm ngon đậm đà",
            PrepTimeMinutes = 30,
            CookTimeMinutes = 120,
            Servings = 4,
            Difficulty = "Medium",
            Status = "Published",
            PublishedAt = publishedDate,
            CreatedAt = publishedDate.AddDays(-1),
            Category = new CategoryDto
            {
                Id = categoryId,
                Name = "Món Nước",
                Slug = "mon-nuoc"
            },
            Author = new RecipeAuthorDto
            {
                Id = "author-123",
                DisplayName = "Đầu bếp Quân",
                UserName = "quan.chef"
            },
            Nutrition = new RecipeNutritionDto
            {
                Calories = 450m,
                Protein = 25m,
                Carbohydrates = 50m,
                Fat = 12m,
                Fiber = 3m,
                Sodium = 800m,
                Source = "Manual"
            },
            Ingredients = new List<RecipeIngredientDto>
            {
                new() { Id = Guid.NewGuid(), Name = "Bánh phở", Quantity = 500m, Unit = "g", OrderIndex = 1 },
                new() { Id = Guid.NewGuid(), Name = "Thịt bò", Quantity = 300m, Unit = "g", OrderIndex = 2 }
            },
            Steps = new List<RecipeStepDto>
            {
                new() { Id = Guid.NewGuid(), StepNumber = 1, Title = "Sơ chế", Description = "Rửa sạch thịt bò và luộc sơ" },
                new() { Id = Guid.NewGuid(), StepNumber = 2, Title = "Nấu nước dùng", Description = "Hầm xương bò trong 2 tiếng" }
            },
            Images = new List<RecipeImageDto>
            {
                new() { Id = Guid.NewGuid(), OriginalUrl = "/images/pho-1.jpg", IsPrimary = true, OrderIndex = 1 }
            }
        };

        // Assert
        Assert.Equal(recipeId, dto.Id);
        Assert.Equal("Phở bò Hà Nội", dto.Title);
        Assert.Equal("pho-bo-ha-noi", dto.Slug);
        Assert.Equal(150, dto.PrepTimeMinutes + dto.CookTimeMinutes);
        Assert.NotNull(dto.Category);
        Assert.Equal(categoryId, dto.Category.Id);
        Assert.NotNull(dto.Author);
        Assert.Equal("author-123", dto.Author.Id);
        Assert.NotNull(dto.Nutrition);
        Assert.Equal(450m, dto.Nutrition.Calories);
        Assert.Equal(2, dto.Ingredients.Count);
        Assert.Equal(2, dto.Steps.Count);
        Assert.Single(dto.Images);
        Assert.True(dto.Images[0].IsPrimary);
    }

    [Fact]
    public void NotFoundException_ShouldHaveCorrectErrorCode_ForRecipeNotFound()
    {
        // Arrange
        const string slug = "non-existent-recipe";
        const string expectedErrorCode = "RECIPE_NOT_FOUND";

        // Act
        var exception = new NotFoundException($"Không tìm thấy công thức nấu ăn với đường dẫn '{slug}'.", expectedErrorCode);

        // Assert
        Assert.Equal(expectedErrorCode, exception.ErrorCode);
        Assert.Contains(slug, exception.Message);
    }
}
