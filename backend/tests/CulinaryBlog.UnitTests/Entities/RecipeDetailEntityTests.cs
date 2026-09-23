namespace CulinaryBlog.UnitTests.Entities;

using System;
using CulinaryBlog.Domain.Entities;
using Xunit;

/// <summary>
/// Unit Tests kiểm tra tính toàn vẹn và các quy tắc của các thực thể con trong Recipe:
/// RecipeIngredient, RecipeStep, RecipeImage (Nhiệm vụ Thành viên 4).
/// </summary>
public class RecipeDetailEntityTests
{
    [Fact]
    public void RecipeIngredient_ShouldInitializeWithValidProperties()
    {
        // Arrange
        var recipeId = Guid.NewGuid();

        // Act
        var ingredient = new RecipeIngredient
        {
            RecipeId = recipeId,
            Name = "Thịt bò Wagyu A5",
            Quantity = 250.5m,
            Unit = "gram",
            Notes = "Thái lát mỏng 2mm",
            OrderIndex = 1
        };

        // Assert
        Assert.NotEqual(Guid.Empty, ingredient.Id);
        Assert.Equal(recipeId, ingredient.RecipeId);
        Assert.Equal("Thịt bò Wagyu A5", ingredient.Name);
        Assert.Equal(250.5m, ingredient.Quantity);
        Assert.Equal("gram", ingredient.Unit);
        Assert.Equal("Thái lát mỏng 2mm", ingredient.Notes);
        Assert.Equal(1, ingredient.OrderIndex);
        Assert.False(ingredient.IsDeleted);
    }

    [Fact]
    public void RecipeIngredient_CanBeCreatedAsAsNeeded_WithNullQuantityAndUnit()
    {
        // Arrange & Act: Kiểm tra trường hợp "vừa đủ" theo quyết định E2
        var ingredient = new RecipeIngredient
        {
            RecipeId = Guid.NewGuid(),
            Name = "Muối biển tinh khiết",
            Quantity = null,
            Unit = null,
            Notes = "Nêm nếm vừa ăn",
            OrderIndex = 5
        };

        // Assert
        Assert.Null(ingredient.Quantity);
        Assert.Null(ingredient.Unit);
        Assert.Equal("Muối biển tinh khiết", ingredient.Name);
    }

    [Fact]
    public void RecipeStep_ShouldInitializeCorrectly_AndSupportParentStepRelationship()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var parentStepId = Guid.NewGuid();

        // Act
        var step = new RecipeStep
        {
            RecipeId = recipeId,
            StepNumber = 1,
            Title = "Sơ chế thịt bò",
            Description = "Dùng khăn sạch thấm khô thịt rồi thái ngang thớ",
            DurationMinutes = 15,
            ImageUrl = "https://storage.culinaryblog.com/recipes/step1.jpg",
            ParentStepId = parentStepId
        };

        // Assert
        Assert.NotEqual(Guid.Empty, step.Id);
        Assert.Equal(recipeId, step.RecipeId);
        Assert.Equal(1, step.StepNumber);
        Assert.Equal("Sơ chế thịt bò", step.Title);
        Assert.Equal("Dùng khăn sạch thấm khô thịt rồi thái ngang thớ", step.Description);
        Assert.Equal(15, step.DurationMinutes);
        Assert.Equal(parentStepId, step.ParentStepId);
        Assert.False(step.IsDeleted);
    }

    [Fact]
    public void RecipeImage_ShouldSupportPrimaryAndGalleryImages()
    {
        // Arrange
        var recipeId = Guid.NewGuid();

        // Act: Ảnh chính
        var primaryImage = new RecipeImage
        {
            RecipeId = recipeId,
            OriginalUrl = $"https://storage.culinaryblog.com/recipes/{recipeId}/primary.jpg",
            MediumUrl = $"https://storage.culinaryblog.com/recipes/{recipeId}/med.jpg",
            ThumbnailUrl = $"https://storage.culinaryblog.com/recipes/{recipeId}/thumb.jpg",
            AltText = "Ảnh đại diện món ăn",
            IsPrimary = true,
            OrderIndex = 0
        };

        // Act: Ảnh phụ gallery
        var galleryImage = new RecipeImage
        {
            RecipeId = recipeId,
            OriginalUrl = $"https://storage.culinaryblog.com/recipes/{recipeId}/step1.jpg",
            AltText = "Ảnh công đoạn 1",
            IsPrimary = false,
            OrderIndex = 1
        };

        // Assert
        Assert.True(primaryImage.IsPrimary);
        Assert.Equal(0, primaryImage.OrderIndex);
        Assert.False(galleryImage.IsPrimary);
        Assert.Equal(1, galleryImage.OrderIndex);
    }
}
