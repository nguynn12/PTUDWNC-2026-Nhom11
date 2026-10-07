namespace CulinaryBlog.UnitTests.Features.RecipeContent;

using CulinaryBlog.Application.Common.Models;
using Xunit;

/// <summary>
/// Unit Tests kiểm tra định dạng Response Envelope chuẩn hóa theo RESOLVED-CONFLICTS.md (C2).
/// Định dạng bắt buộc: { "data": T, "meta": object? }
/// </summary>
public class ApiResponseEnvelopeTests
{
    [Fact]
    public void Success_DetailOrMutation_ShouldHaveData_AndNullMeta()
    {
        // Arrange
        var payload = new { Title = "Phở bò Nam Định", CookTime = 60 };

        // Act
        var response = ApiResponse<object>.Success(payload);

        // Assert
        Assert.NotNull(response.Data);
        Assert.Equal(payload, response.Data);
        Assert.Null(response.Meta);
    }

    [Fact]
    public void Success_ListWithMeta_ShouldContainDataAndMeta()
    {
        // Arrange
        var list = new[] { "Thịt bò", "Hành hoa", "Gừng tươi" };
        var meta = new { Page = 1, PageSize = 10, TotalCount = 3 };

        // Act
        var response = ApiResponse<string[]>.Success(list, meta);

        // Assert
        Assert.Equal(3, response.Data.Length);
        Assert.NotNull(response.Meta);
    }
}
