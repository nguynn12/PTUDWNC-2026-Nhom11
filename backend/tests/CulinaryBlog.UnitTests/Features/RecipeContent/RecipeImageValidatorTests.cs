namespace CulinaryBlog.UnitTests.Features.RecipeContent;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.RecipeImages.DTOs;
using CulinaryBlog.Application.Features.RecipeImages.Validators;
using Xunit;

/// <summary>
/// Unit Tests kiểm tra tính toàn vẹn của RecipeImageValidator và hợp đồng RecipeImageDto.
/// Tuân thủ SRS v1.2.0 (FR-RCP-008) và RESOLVED-CONFLICTS.md (E6, E7).
/// </summary>
public class RecipeImageValidatorTests
{
    [Fact]
    public void ValidateUpdate_ValidInput_ShouldPass()
    {
        // Arrange
        var request = new UpdateRecipeImageRequest
        {
            AltText = "Hình ảnh món phở bò thơm ngon",
            IsPrimary = true,
            OrderIndex = 2
        };

        // Act & Assert
        RecipeImageValidator.ValidateUpdate(request);
    }

    [Fact]
    public void ValidateUpdate_AltTextExceeding200Chars_ShouldThrowValidationException()
    {
        // Arrange
        var request = new UpdateRecipeImageRequest
        {
            AltText = new string('X', 201)
        };

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => RecipeImageValidator.ValidateUpdate(request));
        Assert.True(ex.Errors.ContainsKey("AltText"));
    }

    [Fact]
    public void ValidateUpdate_NegativeOrderIndex_ShouldThrowValidationException()
    {
        // Arrange
        var request = new UpdateRecipeImageRequest
        {
            OrderIndex = -1
        };

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => RecipeImageValidator.ValidateUpdate(request));
        Assert.True(ex.Errors.ContainsKey("OrderIndex"));
    }

    [Fact]
    public void RecipeImageDto_MustProvideImageIdProperty_MatchingId_PerResolvedE7()
    {
        // Arrange: Quyết định E7 bắt buộc response upload ảnh phải có trường imageId
        var id = Guid.NewGuid();

        // Act
        var dto = new RecipeImageDto
        {
            Id = id,
            RecipeId = Guid.NewGuid(),
            OriginalUrl = "https://storage.culinaryblog.com/recipes/photo.jpg",
            AltText = "Món ăn",
            IsPrimary = true,
            OrderIndex = 0
        };

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal(id, dto.ImageId);
    }
}
