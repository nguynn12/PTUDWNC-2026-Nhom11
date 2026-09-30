namespace CulinaryBlog.UnitTests.Categories;

using CulinaryBlog.Application.Common.Helpers;
using Xunit;

/// <summary>
/// Unit Tests kiểm tra tính chính xác của tiện ích sinh Slug tiếng Việt chuẩn SEO (SlugHelper).
/// Đáp ứng yêu cầu FR-CAT-003: slugify chuyển chữ thường, bỏ dấu tiếng Việt, thay khoảng trắng bằng gạch ngang.
/// </summary>
public class SlugHelperTests
{
    [Theory]
    [InlineData("Món Ăn Dân Dã Nam Bộ", "mon-an-dan-da-nam-bo")]
    [InlineData("Bún Chả Cá Quy Nhơn", "bun-cha-ca-quy-nhon")]
    [InlineData("Lẩu Thả Phan Thiết", "lau-tha-phan-thiet")]
    [InlineData("Bánh Xèo Giòn Rụm", "banh-xeo-gion-rum")]
    public void Generate_ShouldRemoveVietnameseDiacriticsAndLowercase(string input, string expected)
    {
        // Act
        var result = SlugHelper.Generate(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Đặc Sản Đồng Tháp", "dac-san-dong-thap")]
    [InlineData("Đồ Nướng Đà Lạt", "do-nuong-da-lat")]
    [InlineData("Điểm Tâm Sáng", "diem-tam-sang")]
    [InlineData("Hủ Tiếu Nam Vang Đậm Đà", "hu-tieu-nam-vang-dam-da")]
    public void Generate_ShouldProperlyHandleLetterD(string input, string expected)
    {
        // Act
        var result = SlugHelper.Generate(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("  Món Ăn   Nhanh & Tiện Lợi!  ", "mon-an-nhanh-tien-loi")]
    [InlineData("Cơm Tấm (Sườn - Bì - Chả)", "com-tam-suon-bi-cha")]
    [InlineData("Trà Sữa #1 --- Trân Châu", "tra-sua-1-tran-chau")]
    [InlineData("Món Ngon 2026 @ Home", "mon-ngon-2026-home")]
    public void Generate_ShouldRemoveSpecialCharactersAndCollapseHyphens(string input, string expected)
    {
        // Act
        var result = SlugHelper.Generate(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Generate_ShouldReturnEmptyString_WhenInputIsNullOrWhiteSpace(string? input)
    {
        // Act
        var result = SlugHelper.Generate(input!);

        // Assert
        Assert.Equal(string.Empty, result);
    }
}
