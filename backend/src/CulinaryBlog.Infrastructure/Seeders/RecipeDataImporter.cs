namespace CulinaryBlog.Infrastructure.Seeders;

using System.Text.Json;
using System.Text.Json.Serialization;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.ValueObjects;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Nạp công thức THẬT từ các tệp JSON trong <c>backend/data/recipes</c> (định dạng xem README.md cùng thư mục).
/// Mỗi công thức bắt buộc có: tác giả (tài khoản đã tồn tại, tìm theo email), danh mục, ít nhất 1 nguyên liệu,
/// 1 bước và nguồn tham khảo. Chạy lặp lại an toàn: công thức đã có (trùng slug) thì bỏ qua.
/// Luồng khởi động chỉ nạp các tệp này, không tự gọi các seeder dữ liệu ngẫu nhiên.
/// </summary>
public static class RecipeDataImporter
{
    public const string DefaultFolder = "data/recipes";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Converters = { new JsonStringEnumConverter() },
    };

    public sealed record ImportResult(int Imported, int Skipped, IReadOnlyList<string> Errors);

    public static async Task<ImportResult> ImportAsync(
        CulinaryBlogDbContext context,
        string folder,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        if (!Directory.Exists(folder))
        {
            logger?.LogInformation("Không có thư mục dữ liệu công thức: {Folder}", folder);
            return new ImportResult(0, 0, errors);
        }

        int imported = 0, skipped = 0;
        foreach (var file in Directory.EnumerateFiles(folder, "*.json").OrderBy(f => f, StringComparer.Ordinal))
        {
            var name = Path.GetFileName(file);
            try
            {
                var data = JsonSerializer.Deserialize<RecipeFile>(
                               await File.ReadAllTextAsync(file, cancellationToken), JsonOptions)
                           ?? throw new InvalidDataException("Tệp rỗng.");

                var problems = Validate(data);
                if (problems.Count > 0)
                {
                    errors.Add($"{name}: {string.Join("; ", problems)}");
                    continue;
                }

                var slug = RecipeSeeder.GenerateSlug(data.Title);
                if (await context.Recipes.IgnoreQueryFilters().AnyAsync(r => r.Slug == slug, cancellationToken))
                {
                    skipped++;
                    continue;
                }

                var email = data.AuthorEmail.Trim().ToUpperInvariant();
                var authorId = await context.Users
                    .Where(u => u.NormalizedEmail == email)
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync(cancellationToken);
                if (authorId is null)
                {
                    errors.Add($"{name}: không có tài khoản tác giả '{data.AuthorEmail}' (tạo tài khoản trước khi nạp).");
                    continue;
                }

                var category = await GetOrCreateCategoryAsync(context, data.CategoryName.Trim(), cancellationToken);
                context.Recipes.Add(BuildRecipe(data, slug, authorId, category.Id));
                await context.SaveChangesAsync(cancellationToken);
                imported++;
                logger?.LogInformation(
                    "Đã nạp công thức {Title} (tác giả {Author}, danh mục {Category})",
                    data.Title, data.AuthorEmail, category.Name);
            }
            catch (Exception ex) when (ex is JsonException or InvalidDataException or IOException)
            {
                errors.Add($"{name}: {ex.Message}");
            }
        }

        foreach (var error in errors)
        {
            logger?.LogWarning("Bỏ qua tệp công thức: {Error}", error);
        }

        return new ImportResult(imported, skipped, errors);
    }

    private static List<string> Validate(RecipeFile d)
    {
        var p = new List<string>();
        if (string.IsNullOrWhiteSpace(d.Title) || d.Title.Length is < 3 or > 200) p.Add("title phải 3–200 ký tự");
        if (string.IsNullOrWhiteSpace(d.Description) || d.Description.Length is < 10 or > 1800) p.Add("description phải 10–1800 ký tự");
        if (string.IsNullOrWhiteSpace(d.CategoryName)) p.Add("thiếu categoryName");
        if (string.IsNullOrWhiteSpace(d.AuthorEmail)) p.Add("thiếu authorEmail");
        if (d.Servings <= 0) p.Add("servings phải > 0");
        if (d.PrepTimeMinutes < 0 || d.CookTimeMinutes < 0) p.Add("thời gian không được âm");
        if (d.Ingredients.Count == 0) p.Add("cần ít nhất 1 nguyên liệu");
        if (d.Ingredients.Any(i => string.IsNullOrWhiteSpace(i.Name))) p.Add("nguyên liệu thiếu name");
        if (d.Steps.Count == 0) p.Add("cần ít nhất 1 bước");
        if (d.Steps.Any(s => string.IsNullOrWhiteSpace(s.Title) || string.IsNullOrWhiteSpace(s.Description)))
            p.Add("bước thiếu title/description");
        if (d.Source is null || string.IsNullOrWhiteSpace(d.Source.Url)) p.Add("thiếu source.url (nguồn tham khảo)");
        return p;
    }

    private static async Task<Category> GetOrCreateCategoryAsync(
        CulinaryBlogDbContext context, string name, CancellationToken cancellationToken)
    {
        var slug = RecipeSeeder.GenerateSlug(name);
        var existing = await context.Categories.FirstOrDefaultAsync(c => c.Slug == slug || c.Name == name, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var order = (await context.Categories.MaxAsync(c => (int?)c.OrderIndex, cancellationToken) ?? 0) + 1;
        var category = new Category
        {
            Name = name,
            Slug = slug,
            Description = $"Công thức {name.ToLowerInvariant()}.",
            OrderIndex = order,
        };
        context.Categories.Add(category);
        await context.SaveChangesAsync(cancellationToken);
        return category;
    }

    private static Recipe BuildRecipe(RecipeFile d, string slug, string authorId, Guid categoryId)
    {
        var recipe = new Recipe
        {
            Title = d.Title.Trim(),
            Slug = slug,
            Description = d.Description.Trim(),
            PrepTimeMinutes = d.PrepTimeMinutes,
            CookTimeMinutes = d.CookTimeMinutes,
            Servings = d.Servings,
            Difficulty = d.Difficulty,
            Status = RecipeStatus.Published,
            PublishedAt = DateTimeOffset.UtcNow,
            CategoryId = categoryId,
            AuthorId = authorId,
            Nutrition = d.Nutrition is null
                ? null
                : new RecipeNutrition
                {
                    Calories = d.Nutrition.Calories,
                    Protein = d.Nutrition.Protein,
                    Carbohydrates = d.Nutrition.Carbohydrates,
                    Fat = d.Nutrition.Fat,
                    Fiber = d.Nutrition.Fiber,
                    Sodium = d.Nutrition.Sodium,
                    Source = NutritionSource.Manual,
                },
        };

        var order = 0;
        foreach (var i in d.Ingredients)
        {
            recipe.Ingredients.Add(new RecipeIngredient
            {
                Name = i.Name.Trim(),
                Quantity = i.Quantity,
                Unit = i.Unit?.Trim(),
                Notes = i.Notes?.Trim(),
                OrderIndex = order++,
            });
        }

        var number = 1;
        foreach (var s in d.Steps)
        {
            recipe.Steps.Add(new RecipeStep
            {
                StepNumber = number++,
                Title = s.Title.Trim(),
                Description = s.Description.Trim(),
                DurationMinutes = s.DurationMinutes,
                ImageUrl = s.ImageUrl?.Trim(),
            });
        }

        order = 0;
        foreach (var img in d.Images.Where(i => !string.IsNullOrWhiteSpace(i.Url)))
        {
            recipe.Images.Add(new RecipeImage
            {
                OriginalUrl = img.Url,
                MediumUrl = img.Url,
                ThumbnailUrl = img.Url,
                AltText = img.AltText,
                IsPrimary = order == 0,
                OrderIndex = order++,
            });
        }

        return recipe;
    }

    // ── Mô hình tệp JSON ────────────────────────────────────────────────────
    private sealed class RecipeFile
    {
        public string Title { get; set; } = "";
        public string? OriginalTitle { get; set; }
        public string Description { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public string AuthorEmail { get; set; } = "";
        public int PrepTimeMinutes { get; set; }
        public int CookTimeMinutes { get; set; }
        public int Servings { get; set; }
        public RecipeDifficulty Difficulty { get; set; } = RecipeDifficulty.Easy;
        public SourceInfo? Source { get; set; }
        public NutritionInfo? Nutrition { get; set; }
        public List<IngredientInfo> Ingredients { get; set; } = [];
        public List<StepInfo> Steps { get; set; } = [];
        public List<ImageInfo> Images { get; set; } = [];
    }

    private sealed class SourceInfo
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public string? AccessedOn { get; set; }
        public string? Note { get; set; }
    }

    private sealed class NutritionInfo
    {
        public decimal? Calories { get; set; }
        public decimal? Protein { get; set; }
        public decimal? Carbohydrates { get; set; }
        public decimal? Fat { get; set; }
        public decimal? Fiber { get; set; }
        public decimal? Sodium { get; set; }
    }

    private sealed class IngredientInfo
    {
        public string Name { get; set; } = "";
        public decimal? Quantity { get; set; }
        public string? Unit { get; set; }
        public string? Notes { get; set; }
    }

    private sealed class StepInfo
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public int? DurationMinutes { get; set; }
        public string? ImageUrl { get; set; }
    }

    private sealed class ImageInfo
    {
        public string Url { get; set; } = "";
        public string? AltText { get; set; }
        public string? License { get; set; }
    }
}
