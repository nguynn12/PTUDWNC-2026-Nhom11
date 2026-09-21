namespace CulinaryBlog.Infrastructure.Seeders;

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Bogus;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.ValueObjects;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Lớp sinh dữ liệu giả lập cho công thức nấu ăn (RecipeSeeder).
/// Sử dụng thư viện Bogus để tạo ít nhất 100 công thức nấu ăn phong phú, thực tế
/// theo yêu cầu của bài tập Lab 2 và tài liệu SRS v1.2.0.
/// </summary>
public static class RecipeSeeder
{
    /// <summary>
    /// Danh sách 20 CategoryId mẫu để phân bổ 100 công thức đồng đều qua ít nhất 20 danh mục.
    /// </summary>
    public static readonly IReadOnlyList<Guid> CategoryIds = Enumerable.Range(1, 20)
        .Select(i => Guid.Parse($"00000000-0000-0000-0000-{i:D12}"))
        .ToList();

    /// <summary>
    /// Danh sách các AuthorId mẫu.
    /// </summary>
    public static readonly IReadOnlyList<string> AuthorIds =
    [
        "usr_chef_nguyen_2312704",
        "usr_chef_gordon_ramsay",
        "usr_chef_jamie_oliver",
        "usr_chef_luke_nguyen",
        "usr_home_cook_lan"
    ];

    private static readonly string[] DishPrefixes =
    [
        "Phở", "Bún", "Cơm", "Bánh", "Canh", "Gỏi", "Lẩu", "Mì", "Chả", "Cá kho",
        "Thịt kho", "Súp", "Salad", "Bò lúc lắc", "Gà rán", "Nem rán", "Tôm rim",
        "Bò sốt vang", "Vịt quay", "Bánh xèo"
    ];

    private static readonly string[] DishModifiers =
    [
        "Bò Hà Nội", "Chả cá Nha Trang", "Tấm Sườn Bì Chả", "Mì Quảng Đà Nẵng",
        "Chua cay Nam Bộ", "Cuốn tôm thịt", "Hải sản chua cay", "Xào giòn hải sản",
        "Cá lóc đồng", "Tộ tiêu đen", "Trứng cút nước dừa", "Bí đỏ kem tươi",
        "Rong biển mè rang", "Tiêu xanh Phú Quốc", "Mắm tỏi ớt", "Hà Nội truyền thống",
        "Mặn ngọt đậm đà", "Bánh mì giòn rụm", "Bắc Kinh ngũ vị", "Miền Tây giòn tan"
    ];

    /// <summary>
    /// Sinh và nạp dữ liệu 100 recipes vào cơ sở dữ liệu nếu chưa có.
    /// </summary>
    public static async Task SeedAsync(CulinaryBlogDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Recipes.AnyAsync(cancellationToken))
        {
            return;
        }

        var faker = new Faker("vi");
        var random = new Random(2312704); // Seed cố định theo MSSV để dữ liệu tái lập được

        var recipes = new List<Recipe>();
        var usedSlugs = new HashSet<string>();

        for (int i = 0; i < 100; i++)
        {
            var prefix = DishPrefixes[i % DishPrefixes.Length];
            var modifier = DishModifiers[(i * 3 + 7) % DishModifiers.Length];
            var title = $"{prefix} {modifier} {(i >= 20 ? $"(Kiểu {i + 1})" : string.Empty)}".Trim();

            var baseSlug = GenerateSlug(title);
            var slug = baseSlug;
            int counter = 1;
            while (!usedSlugs.Add(slug))
            {
                slug = $"{baseSlug}-{counter++}";
            }

            var categoryId = CategoryIds[i % CategoryIds.Count];
            var authorId = AuthorIds[random.Next(AuthorIds.Count)];
            var status = (RecipeStatus)(i % 3); // Phân bổ Draft, Published, Archived

            var prepTime = random.Next(10, 60);
            var cookTime = (i % 10 == 0) ? 0 : random.Next(15, 120); // Một số món salad 0 phút nấu

            var createdAt = DateTimeOffset.UtcNow.AddDays(-random.Next(1, 90));
            DateTimeOffset? publishedAt = status == RecipeStatus.Published
                ? createdAt.AddHours(random.Next(1, 24))
                : null;

            var recipe = new Recipe
            {
                Title = title,
                Slug = slug,
                Description = $"Công thức nấu {title} thơm ngon, chuẩn vị với các bước hướng dẫn chi tiết, dễ làm tại nhà.",
                Instructions = $"Tổng quan các bước thực hiện món {title}: chuẩn bị nguyên liệu sạch, sơ chế kỹ lưỡng, nêm nếm gia vị vừa miệng và trang trí bắt mắt khi dọn ra đĩa.",
                PrepTimeMinutes = prepTime,
                CookTimeMinutes = cookTime,
                Servings = random.Next(2, 8),
                Difficulty = (RecipeDifficulty)random.Next(1, 4),
                Status = status,
                CategoryId = categoryId,
                AuthorId = authorId,
                PublishedAt = publishedAt,
                Nutrition = new RecipeNutrition
                {
                    Calories = Math.Round((decimal)random.Next(250, 850), 2),
                    Protein = Math.Round((decimal)random.Next(15, 60), 2),
                    Carbohydrates = Math.Round((decimal)random.Next(20, 90), 2),
                    Fat = Math.Round((decimal)random.Next(5, 40), 2),
                    Fiber = Math.Round((decimal)random.Next(2, 12), 2),
                    Sodium = Math.Round((decimal)random.Next(300, 1800), 2),
                    Source = NutritionSource.Manual
                }
            };

            recipes.Add(recipe);
        }

        await context.Recipes.AddRangeAsync(recipes, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Tạo chuỗi slug thân thiện với URL từ tiêu đề tiếng Việt.
    /// </summary>
    public static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

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
        clean = clean.Replace('đ', 'd').Replace('Đ', 'd');
        clean = Regex.Replace(clean, @"[^a-z0-9\s-]", "");
        clean = Regex.Replace(clean, @"\s+", "-").Trim('-');

        return clean.Length > 150 ? clean[..150].TrimEnd('-') : clean;
    }
}
