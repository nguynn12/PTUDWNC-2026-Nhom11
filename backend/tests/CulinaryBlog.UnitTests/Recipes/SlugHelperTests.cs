namespace CulinaryBlog.UnitTests.Recipes;

using CulinaryBlog.Application.Common.Helpers;
using Xunit;

/// <summary>
/// Unit Tests kiểm tra tính chính xác của SlugHelper khi sinh URL-friendly slug từ tiêu đề tiếng Việt.
/// </summary>
public class SlugHelperTests
{
    [Fact]
    public void GenerateSlug_ShouldConvertVietnameseDiacriticsCorrectly()
    {
        // Arrange
        const string input = "Phở Bò Tái Nạm Hà Nội Đậm Đà Hương Vị";
        const string expected = "pho-bo-tai-nam-ha-noi-dam-da-huong-vi";

        // Act
        var result = SlugHelper.GenerateSlug(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GenerateSlug_ShouldHandleSpecialCharactersAndExtraSpaces()
    {
        // Arrange
        const string input = "  Bún Chả Hà Nội (Đặc Biệt) - Siêu Ngon @ 2026!  ";
        const string expected = "bun-cha-ha-noi-dac-biet-sieu-ngon-2026";

        // Act
        var result = SlugHelper.GenerateSlug(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GenerateSlug_ShouldReturnEmptyString_WhenInputIsNullOrWhitespace()
    {
        // Act & Assert
        Assert.Equal(string.Empty, SlugHelper.GenerateSlug(""));
        Assert.Equal(string.Empty, SlugHelper.GenerateSlug("   "));
    }

    [Fact]
    public void GenerateSlug_ShouldHandleLetterDCorrectly()
    {
        // Arrange
        const string input = "Đậu phụ rán giòn và Đu đủ xanh";
        const string expected = "dau-phu-ran-gion-va-du-du-xanh";

        // Act
        var result = SlugHelper.GenerateSlug(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GenerateSlug_ShouldNotExceedMaximumLength()
    {
        // Arrange
        var longTitle = new string('a', 250);

        // Act
        var result = SlugHelper.GenerateSlug(longTitle);

        // Assert
        Assert.True(result.Length <= 200);
    }
}
