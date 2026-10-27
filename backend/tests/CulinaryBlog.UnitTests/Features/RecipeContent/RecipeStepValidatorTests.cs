namespace CulinaryBlog.UnitTests.Features.RecipeContent;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.RecipeSteps.DTOs;
using CulinaryBlog.Application.Features.RecipeSteps.Validators;
using Xunit;

/// <summary>
/// Unit Tests kiểm tra tính toàn vẹn của RecipeStepValidator.
/// Tuân thủ SRS v1.2.0 (FR-RCP-010) và RESOLVED-CONFLICTS.md (E5).
/// </summary>
public class RecipeStepValidatorTests
{
    [Fact]
    public void ValidateCreate_ValidInput_ShouldPass()
    {
        // Arrange
        var request = new CreateRecipeStepRequest
        {
            Title = "Xào thịt bò",
            Description = "Bắc chảo lên bếp, cho dầu ăn và tỏi băm phi thơm rồi cho thịt bò vào xào nhanh lửa lớn.",
            DurationMinutes = 5,
            ImageUrl = "https://example.com/step.jpg",
            StepNumber = 1
        };

        // Act & Assert
        RecipeStepValidator.ValidateCreate(request);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ValidateCreate_EmptyOrWhitespaceDescription_ShouldThrowValidationException(string? emptyDescription)
    {
        // Arrange
        var request = new CreateRecipeStepRequest
        {
            Title = "Bước 1",
            Description = emptyDescription!
        };

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => RecipeStepValidator.ValidateCreate(request));
        Assert.True(ex.Errors.ContainsKey("Description"));
    }

    [Fact]
    public void ValidateCreate_DescriptionExceeding2000Chars_ShouldThrowValidationException()
    {
        // Arrange
        var request = new CreateRecipeStepRequest
        {
            Title = "Bước dài",
            Description = new string('D', 2001)
        };

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => RecipeStepValidator.ValidateCreate(request));
        Assert.True(ex.Errors.ContainsKey("Description"));
    }

    [Fact]
    public void ValidateCreate_NegativeDuration_ShouldThrowValidationException()
    {
        // Arrange
        var request = new CreateRecipeStepRequest
        {
            Title = "Thời gian sai",
            Description = "Mô tả bước",
            DurationMinutes = -1
        };

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => RecipeStepValidator.ValidateCreate(request));
        Assert.True(ex.Errors.ContainsKey("DurationMinutes"));
    }

    [Fact]
    public void ValidateCreate_ZeroDuration_ShouldPass()
    {
        // Arrange: 0 phút là hợp lệ cho bước chuẩn bị nhanh không tính giờ
        var request = new CreateRecipeStepRequest
        {
            Title = "Trộn gia vị",
            Description = "Cho các loại gia vị vào chén và khuấy đều.",
            DurationMinutes = 0
        };

        // Act & Assert
        RecipeStepValidator.ValidateCreate(request);
    }

    [Fact]
    public void ValidateCreate_InvalidStepNumber_ShouldThrowValidationException()
    {
        // Arrange
        var request = new CreateRecipeStepRequest
        {
            Description = "Mô tả",
            StepNumber = 0
        };

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => RecipeStepValidator.ValidateCreate(request));
        Assert.True(ex.Errors.ContainsKey("StepNumber"));
    }

    [Fact]
    public void ValidateReorder_EmptyStepIds_ShouldThrowBadRequestException()
    {
        // Arrange
        var request = new ReorderRecipeStepsRequest
        {
            StepIds = new List<Guid>()
        };

        // Act & Assert
        Assert.Throws<BadRequestException>(() => RecipeStepValidator.ValidateReorder(request));
    }

    [Fact]
    public void ValidateReorder_DuplicateStepIds_ShouldThrowBadRequestException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new ReorderRecipeStepsRequest
        {
            StepIds = new List<Guid> { id, id }
        };

        // Act & Assert
        Assert.Throws<BadRequestException>(() => RecipeStepValidator.ValidateReorder(request));
    }
}
