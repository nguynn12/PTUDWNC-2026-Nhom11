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

    private static readonly string[] DescriptionTemplates =
    [
        "Tổng hợp các công thức {0} thơm ngon, chuẩn vị truyền thống cho bữa cơm gia đình.",
        "Khám phá thế giới {0} phong phú, hấp dẫn và dễ dàng thực hiện ngay tại gian bếp của bạn.",
        "Hướng dẫn chi tiết cách chế biến {0} thanh đạm, bổ dưỡng và tốt cho sức khỏe cả gia đình.",
        "Bộ sưu tập {0} đặc sắc, kết hợp tinh tế giữa nguyên liệu tươi ngon và gia vị tròn vị.",
        "Tuyển chọn {0} tuyệt hảo dành cho dịp sum họp cuối tuần, tiệc tùng và chiêu đãi bạn bè.",
        "Cẩm nang nấu {0} chuẩn vị với các bí quyết chế biến đơn giản, ngon miệng và đẹp mắt.",
        "Tuyển tập công thức {0} độc đáo, kích thích vị giác và mang lại trải nghiệm ẩm thực trọn vẹn.",
        "Gợi ý thực đơn {0} nhanh gọn, thơm ngon và giàu giá trị dinh dưỡng cho mọi bữa ăn."
    ];

    /// <summary>
    /// Danh sách 20 CategoryId cố định để đồng bộ khóa ngoại liên kết với RecipeSeeder (Thành viên 3).
    /// </summary>
    public static readonly IReadOnlyList<Guid> DeterministicIds = Enumerable.Range(1, 20)
        .Select(i => Guid.Parse($"00000000-0000-0000-0000-{i:D12}"))
        .ToList();

    /// <summary>
    /// Tạo đối tượng Faker để cấu hình quy tắc sinh dữ liệu ngẫu nhiên cho Category.
    /// </summary>
    public static Faker<Category> CreateFaker()
    {
        return new Faker<Category>("vi")
            .RuleFor(c => c.Id, f =>
            {
                var idx = f.IndexFaker;
                return idx < DeterministicIds.Count
                    ? DeterministicIds[idx]
                    : f.Random.Guid();
            })
            .RuleFor(c => c.Name, f =>
            {
                var idx = f.IndexFaker;
                if (idx < CategoryBaseNames.Length)
                {
                    return CategoryBaseNames[idx];
                }
                return $"{f.Commerce.Department()} Ẩm Thực {idx + 1}";
            })
            .RuleFor(c => c.Slug, (f, c) => Slugify(c.Name))
            .RuleFor(c => c.Description, (f, c) =>
            {
                var template = f.PickRandom(DescriptionTemplates);
                return string.Format(template, c.Name.ToLowerInvariant());
            })
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
