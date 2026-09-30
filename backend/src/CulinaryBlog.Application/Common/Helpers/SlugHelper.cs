namespace CulinaryBlog.Application.Common.Helpers;

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Tiện ích chuẩn hóa chuỗi và sinh slug URL-friendly từ tiêu đề tiếng Việt.
/// </summary>
public static partial class SlugHelper
{
    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex RemoveInvalidCharsRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultipleSpacesRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex MultipleHyphensRegex();

    /// <summary>
    /// Chuyển đổi một tiêu đề (bao gồm tiếng Việt có dấu) thành slug URL-friendly.
    /// Ví dụ: "Phở Bò Hà Nội Đặc Biệt 2026!" => "pho-bo-ha-noi-dac-biet-2026"
    /// </summary>
    public static string GenerateSlug(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return string.Empty;
        }

        var normalized = title.Trim().ToLowerInvariant();

        // Xử lý ký tự 'đ' trong tiếng Việt
        normalized = normalized.Replace("đ", "d");

        // Loại bỏ dấu thanh tiếng Việt qua Unicode Normalization FormD
        var decomposed = normalized.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var ch in decomposed)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(ch);
            }
        }

        var cleanString = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

        // Loại bỏ các ký tự đặc biệt, chỉ giữ lại chữ cái, số, khoảng trắng và gạch ngang
        cleanString = RemoveInvalidCharsRegex().Replace(cleanString, "");

        // Thay thế khoảng trắng bằng dấu gạch ngang
        cleanString = MultipleSpacesRegex().Replace(cleanString, "-");

        // Loại bỏ các dấu gạch ngang trùng lặp liên tiếp
        cleanString = MultipleHyphensRegex().Replace(cleanString, "-").Trim('-');

        // Giới hạn độ dài slug tối đa 200 ký tự (theo đặc tả cột Slug tối đa 220 ký tự, dự trù hậu tố -1, -2...)
        if (cleanString.Length > 200)
        {
            cleanString = cleanString[..200].TrimEnd('-');
        }

        return cleanString;
    }
}
