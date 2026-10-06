using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CulinaryBlog.Application.Common.Helpers;

/// <summary>
/// Tiện ích hỗ trợ chuẩn hóa chuỗi tiếng Việt thành đường dẫn URL-friendly (Slug).
/// </summary>
public static partial class SlugHelper
{
    /// <summary>
    /// Tạo slug từ tiêu đề/tên tiếng Việt (bỏ dấu, chuyển chữ thường, thay khoảng trắng bằng gạch ngang).
    /// </summary>
    /// <param name="input">Chuỗi văn bản cần tạo slug.</param>
    /// <returns>Chuỗi slug chuẩn SEO.</returns>
    public static string Generate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // 1. Chuyển chữ 'đ'/'Đ' thành 'd' trước khi phân rã dấu
        var text = input.Trim().Replace("đ", "d").Replace("Đ", "d");

        // 2. Phân rã ký tự dấu Unicode (FormD)
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        // 3. Chuẩn hóa lại FormC và chuyển sang chữ thường
        var cleanText = stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();

        // 4. Thay thế các ký tự không phải chữ cái và số bằng dấu gạch ngang
        cleanText = InvalidCharsRegex().Replace(cleanText, "-");

        // 5. Rút gọn nhiều dấu gạch ngang liên tiếp thành 1 dấu duy nhất
        cleanText = MultipleHyphensRegex().Replace(cleanText, "-");

        // 6. Cắt bỏ dấu gạch ngang ở đầu và cuối chuỗi
        return cleanText.Trim('-');
    }

    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex InvalidCharsRegex();

    [GeneratedRegex(@"[\s-]+")]
    private static partial Regex MultipleHyphensRegex();
}
