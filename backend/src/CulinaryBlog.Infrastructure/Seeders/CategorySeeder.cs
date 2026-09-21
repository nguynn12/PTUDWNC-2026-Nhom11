namespace CulinaryBlog.Infrastructure.Seeders;

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Bogus;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Lớp sinh dữ liệu ngẫu nhiên cho bảng Categories bằng thư viện Bogus.
/// Phục vụ yêu cầu tối thiểu của Lab 2 (tạo dữ liệu ngẫu nhiên cho ít nhất 20 categories).
/// </summary>
public static class CategorySeeder
{
    private static readonly string[] CategoryBaseNames =
    [
        "Món khai vị",
        "Món chính",
        "Món canh",
        "Món súp",
        "Món tráng miệng",
        "Đồ uống & Trà",
        "Món ăn vặt",
        "Món chay thanh tịnh",
        "Bánh & Điểm tâm",
        "Món xào",
        "Món kho đậm đà",
        "Món nướng thơm lừng",
        "Món hấp thanh nhẹ",
        "Món lẩu gia đình",
        "Hải sản tươi sống",
        "Salad & Gỏi",
        "Món ăn sáng",
        "Món bún & phở",
        "Món cháo bồi bổ",
        "Món cuốn thanh mát",
        "Ẩm thực miền Bắc",
        "Ẩm thực miền Trung",
        "Ẩm thực miền Nam",
        "Ẩm thực đường phố",
        "Nước chấm & Sốt"
    ];

    /// <summary>
    /// Tạo đối tượng Faker để cấu hình quy tắc sinh dữ liệu ngẫu nhiên cho Category.
    /// </summary>
    public static Faker<Category> CreateFaker()
    {
        var categoryIndex = 0;

        return new Faker<Category>("vi")
            .RuleFor(c => c.Id, f => f.Random.Guid())
            .RuleFor(c => c.Name, f =>
            {
                if (categoryIndex < CategoryBaseNames.Length)
                {
                    return CategoryBaseNames[categoryIndex++];
                }
                return $"{f.Commerce.Department()} Ẩm Thực {f.IndexFaker}";
            })
            .RuleFor(c => c.Slug, (f, c) => Slugify(c.Name))
            .RuleFor(c => c.Description, f => f.Lorem.Sentence(f.Random.Number(8, 15)))
            .RuleFor(c => c.ImageUrl, f => $"https://picsum.photos/seed/{f.Random.AlphaNumeric(8)}/800/600")
            .RuleFor(c => c.OrderIndex, f => f.IndexFaker + 1)
            .RuleFor(c => c.CreatedAt, f => f.Date.Past(1).ToUniversalTime())
            .RuleFor(c => c.UpdatedAt, (DateTime?)null)
            .RuleFor(c => c.IsDeleted, false)
            .RuleFor(c => c.DeletedAt, (DateTime?)null);
    }

    /// <summary>
    /// Sinh danh sách danh mục ngẫu nhiên bằng Bogus (mặc định 20 danh mục).
    /// </summary>
    /// <param name="count">Số lượng danh mục cần sinh (mặc định 20)</param>
    public static List<Category> Generate(int count = 20)
    {
        // Khởi tạo seed cố định để dữ liệu sinh ra có tính nhất quán và lặp lại được
        Randomizer.Seed = new Random(2026);
        var faker = CreateFaker();
        return faker.Generate(count);
    }

    /// <summary>
    /// Nạp dữ liệu ngẫu nhiên vào cơ sở dữ liệu nếu bảng Categories đang trống.
    /// Phương thức an toàn (Idempotent), tránh trùng lặp dữ liệu.
    /// </summary>
    /// <param name="context">DbContext ứng dụng</param>
    /// <param name="count">Số lượng danh mục muốn nạp (ít nhất 20)</param>
    public static async Task SeedAsync(CulinaryBlogDbContext context, int count = 20)
    {
        if (!await context.Categories.AnyAsync())
        {
            var categories = Generate(count);
            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Tiện ích chuyển đổi tên tiếng Việt thành Slug chuẩn URL.
    /// </summary>
    private static string Slugify(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

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

        var clean = stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        clean = clean.Replace("đ", "d").Replace("Đ", "d");
        clean = Regex.Replace(clean, @"[^a-z0-9\s-]", "");
        clean = Regex.Replace(clean, @"\s+", "-").Trim('-');
        return clean;
    }
}
