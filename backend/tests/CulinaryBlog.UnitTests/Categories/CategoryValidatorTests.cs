namespace CulinaryBlog.UnitTests.Categories;

using System;
using CulinaryBlog.Application.Features.Categories.Commands.CreateCategory;
using CulinaryBlog.Application.Features.Categories.Commands.UpdateCategory;
using Xunit;

/// <summary>
/// Unit Tests kiểm tra các quy tắc xác thực (FluentValidation) cho CreateCategoryCommand và UpdateCategoryCommand.
/// Đáp ứng các ràng buộc trong FR-CAT-003, FR-CAT-004:
/// - Name: 2-100 ký tự, không chứa HTML.
/// - OrderIndex: >= 0.
/// </summary>
public class CategoryValidatorTests
{
    private readonly CreateCategoryCommandValidator _createValidator = new();
    private readonly UpdateCategoryCommandValidator _updateValidator = new();

    #region CreateCategoryCommandValidator Tests

    [Fact]
    public void CreateValidator_ShouldPass_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: "Món Khai Vị",
            Description: "Các món ăn nhẹ kích thích vị giác trước bữa chính",
            ImageUrl: "https://example.com/images/khai-vi.jpg",
            OrderIndex: 1);

        // Act
        var result = _createValidator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateValidator_ShouldFail_WhenNameIsEmptyOrNull(string? name)
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: name!,
            Description: "Mô tả hợp lệ",
            ImageUrl: null,
            OrderIndex: 0);

        // Act
        var result = _createValidator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCategoryCommand.Name));
    }

    [Fact]
    public void CreateValidator_ShouldFail_WhenNameIsTooShort()
    {
        // Arrange: Name chỉ có 1 ký tự (< 2)
        var command = new CreateCategoryCommand(
            Name: "A",
            Description: null,
            ImageUrl: null,
            OrderIndex: 0);

        // Act
        var result = _createValidator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCategoryCommand.Name));
    }

    [Fact]
    public void CreateValidator_ShouldFail_WhenNameExceeds100Characters()
    {
        // Arrange: Name 101 ký tự
        var command = new CreateCategoryCommand(
            Name: new string('A', 101),
            Description: null,
            ImageUrl: null,
            OrderIndex: 0);

        // Act
        var result = _createValidator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCategoryCommand.Name));
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("<b>Món Ăn Ngon</b>")]
    [InlineData("Món nướng <img src='x' onerror='alert(1)'>")]
    public void CreateValidator_ShouldFail_WhenNameContainsHtmlTags(string invalidName)
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: invalidName,
            Description: null,
            ImageUrl: null,
            OrderIndex: 0);

        // Act
        var result = _createValidator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCategoryCommand.Name));
    }

    [Fact]
    public void CreateValidator_ShouldFail_WhenOrderIndexIsNegative()
    {
        // Arrange: OrderIndex = -1 (< 0)
        var command = new CreateCategoryCommand(
            Name: "Món Chay",
            Description: null,
            ImageUrl: null,
            OrderIndex: -1);

        // Act
        var result = _createValidator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCategoryCommand.OrderIndex));
    }

    #endregion

    #region UpdateCategoryCommandValidator Tests

    [Fact]
    public void UpdateValidator_ShouldPass_WhenCommandIsValid()
    {
        // Arrange
        var command = new UpdateCategoryCommand(
            Id: Guid.NewGuid(),
            Name: "Món Canh & Súp",
            Description: "Cập nhật mô tả phong phú hơn",
            ImageUrl: "https://example.com/soup.jpg",
            OrderIndex: 3);

        // Act
        var result = _updateValidator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void UpdateValidator_ShouldFail_WhenNameIsEmptyOrNull(string? name)
    {
        // Arrange
        var command = new UpdateCategoryCommand(
            Id: Guid.NewGuid(),
            Name: name!,
            Description: null,
            ImageUrl: null,
            OrderIndex: 0);

        // Act
        var result = _updateValidator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCategoryCommand.Name));
    }

    [Fact]
    public void UpdateValidator_ShouldFail_WhenOrderIndexIsNegative()
    {
        // Arrange
        var command = new UpdateCategoryCommand(
            Id: Guid.NewGuid(),
            Name: "Món Tráng Miệng",
            Description: null,
            ImageUrl: null,
            OrderIndex: -5);

        // Act
        var result = _updateValidator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCategoryCommand.OrderIndex));
    }

    #endregion
}
