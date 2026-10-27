namespace CulinaryBlog.UnitTests.Features.RecipeContent;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.RecipeIngredients.DTOs;
using CulinaryBlog.Application.Features.RecipeIngredients.Validators;
using Xunit;

/// <summary>
/// Unit Tests kiểm tra tính toàn vẹn của RecipeIngredientValidator.
/// Tuân thủ SRS v1.2.0 (FR-RCP-009) và RESOLVED-CONFLICTS.md (E1, E2).
/// </summary>
public class RecipeIngredientValidatorTests
{
    [Fact]
    public void ValidateCreate_ValidInputWithQuantityAndUnit_ShouldPass()
    {
        // Arrange
        var request = new CreateRecipeIngredientRequest
        {
            Name = "Thịt bò thăn",
            Quantity = 300.5m,
            Unit = "gram",
            Notes = "Thái mỏng",
            OrderIndex = 1
        };

        // Act & Assert (không ném ngoại lệ)
        RecipeIngredientValidator.ValidateCreate(request);
    }

    [Fact]
    public void ValidateCreate_AsNeededWithNullQuantityAndUnit_ShouldPass()
    {
        // Arrange: Kiểm tra trường hợp "vừa đủ" theo quyết định E2
        var request = new CreateRecipeIngredientRequest
        {
            Name = "Muối tiêu",
            Quantity = null,
            Unit = null,
            Notes = "Gia vị nêm nếm"
        };

        // Act & Assert
        RecipeIngredientValidator.ValidateCreate(request);
    }

    [Fact]
    public void ValidateCreate_QuantityProvidedWithoutUnit_ShouldThrowValidationException()
    {
        // Arrange: Có quantity nhưng thiếu unit theo E2
        var request = new CreateRecipeIngredientRequest
        {
            Name = "Đường cát",
            Quantity = 50m,
            Unit = null
        };

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => RecipeIngredientValidator.ValidateCreate(request));
        Assert.True(ex.Errors.ContainsKey("Unit"));
    }

    [Fact]
    public void ValidateCreate_UnitProvidedWithoutQuantity_ShouldThrowValidationException()
    {
        // Arrange: Có unit nhưng thiếu quantity theo E2
        var request = new CreateRecipeIngredientRequest
        {
            Name = "Nước cốt dừa",
            Quantity = null,
            Unit = "ml"
        };

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => RecipeIngredientValidator.ValidateCreate(request));
        Assert.True(ex.Errors.ContainsKey("Quantity"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(-0.001)]
    public void ValidateCreate_ZeroOrNegativeQuantity_ShouldThrowValidationException(decimal invalidQuantity)
    {
        // Arrange
        var request = new CreateRecipeIngredientRequest
        {
            Name = "Bột năng",
            Quantity = invalidQuantity,
            Unit = "thìa canh"
        };

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => RecipeIngredientValidator.ValidateCreate(request));
        Assert.True(ex.Errors.ContainsKey("Quantity"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ValidateCreate_EmptyOrWhitespaceName_ShouldThrowValidationException(string? emptyName)
    {
        // Arrange
        var request = new CreateRecipeIngredientRequest
        {
            Name = emptyName!,
            Quantity = 100m,
            Unit = "g"
        };

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => RecipeIngredientValidator.ValidateCreate(request));
        Assert.True(ex.Errors.ContainsKey("Name"));
    }

    [Fact]
    public void ValidateCreate_NameExceeding200Chars_ShouldThrowValidationException()
    {
        // Arrange
        var request = new CreateRecipeIngredientRequest
        {
            Name = new string('A', 201),
            Quantity = 100m,
            Unit = "g"
        };

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => RecipeIngredientValidator.ValidateCreate(request));
        Assert.True(ex.Errors.ContainsKey("Name"));
    }

    [Fact]
    public void ValidateUpdate_NegativeOrderIndex_ShouldThrowValidationException()
    {
        // Arrange
        var request = new UpdateRecipeIngredientRequest
        {
            Name = "Hành tây",
            Quantity = 1m,
            Unit = "củ",
            OrderIndex = -1
        };

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => RecipeIngredientValidator.ValidateUpdate(request));
        Assert.True(ex.Errors.ContainsKey("OrderIndex"));
    }
}
