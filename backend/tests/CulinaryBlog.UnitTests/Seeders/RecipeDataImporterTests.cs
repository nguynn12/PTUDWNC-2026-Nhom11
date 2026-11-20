using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Identity;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Seeders;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CulinaryBlog.UnitTests.Seeders;

/// <summary>Bộ nạp dữ liệu thật: công thức phải có tác giả, danh mục, nguyên liệu, bước, nguồn và nạp lặp lại không nhân đôi.</summary>
public sealed class RecipeDataImporterTests : IDisposable
{
    private const string AuthorEmail = "author@culinaryblog.local";
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "recipe-import-" + Guid.NewGuid());

    public RecipeDataImporterTests() => Directory.CreateDirectory(_folder);

    public void Dispose() => Directory.Delete(_folder, recursive: true);

    private static CulinaryBlogDbContext NewDb(bool withAuthor = true)
    {
        var db = new CulinaryBlogDbContext(new DbContextOptionsBuilder<CulinaryBlogDbContext>()
            .UseInMemoryDatabase("importer-" + Guid.NewGuid()).Options);
        if (withAuthor)
        {
            var user = ApplicationUser.Create(AuthorEmail, "Author User");
            user.NormalizedEmail = AuthorEmail.ToUpperInvariant();
            db.Users.Add(user);
            db.SaveChanges();
        }

        return db;
    }

    private static string RepoDataFolder()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "backend", "data", "recipes")))
            dir = dir.Parent;
        return Path.Combine(dir!.FullName, "backend", "data", "recipes");
    }

    private void Write(string name, string json) => File.WriteAllText(Path.Combine(_folder, name), json);

    private const string ValidJson = """
        {
          "title": "Canh thử nghiệm", "description": "Mô tả công thức thử nghiệm đủ dài.",
          "categoryName": "Món canh", "authorEmail": "author@culinaryblog.local",
          "prepTimeMinutes": 5, "cookTimeMinutes": 10, "servings": 2, "difficulty": "Medium",
          "source": { "name": "Nguồn", "url": "https://example.com/x" },
          "nutrition": { "calories": 100, "protein": 5 },
          "ingredients": [ { "name": "Muối", "quantity": 1.5, "unit": "g" }, { "name": "Tiêu" } ],
          "steps": [ { "title": "B1", "description": "Làm A", "durationMinutes": 3 }, { "title": "B2", "description": "Làm B" } ]
        }
        """;

    [Fact]
    public async Task Import_CongThucHopLe_CoTacGiaDanhMucNguyenLieuBuocVaNguon()
    {
        using var db = NewDb();
        Write("a.json", ValidJson);

        var result = await RecipeDataImporter.ImportAsync(db, _folder, null, TestContext.Current.CancellationToken);

        Assert.Equal(1, result.Imported);
        Assert.Empty(result.Errors);
        var recipe = await db.Recipes.Include(r => r.Ingredients).Include(r => r.Steps).SingleAsync(TestContext.Current.CancellationToken);
        var author = await db.Users.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(author.Id, recipe.AuthorId);
        Assert.Equal("Món canh", (await db.Categories.SingleAsync(c => c.Id == recipe.CategoryId, TestContext.Current.CancellationToken)).Name);
        Assert.Equal(RecipeStatus.Published, recipe.Status);
        Assert.NotNull(recipe.PublishedAt);
        Assert.Equal(RecipeDifficulty.Medium, recipe.Difficulty);
        Assert.DoesNotContain("https://example.com/x", recipe.Description);
        Assert.DoesNotContain("Nguồn tham khảo:", recipe.Description);
        Assert.Equal(["Muối", "Tiêu"], recipe.Ingredients.OrderBy(i => i.OrderIndex).Select(i => i.Name));
        Assert.Equal([1, 2], recipe.Steps.OrderBy(s => s.StepNumber).Select(s => s.StepNumber));
        Assert.Equal(100m, recipe.Nutrition!.Calories);
        Assert.Null(recipe.Nutrition.Fat);
    }

    [Fact]
    public async Task Import_ChayLai_KhongNhanDoi()
    {
        using var db = NewDb();
        Write("a.json", ValidJson);

        await RecipeDataImporter.ImportAsync(db, _folder, null, TestContext.Current.CancellationToken);
        var second = await RecipeDataImporter.ImportAsync(db, _folder, null, TestContext.Current.CancellationToken);

        Assert.Equal(0, second.Imported);
        Assert.Equal(1, second.Skipped);
        Assert.Equal(1, await db.Recipes.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await db.Categories.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Import_KhongCoTaiKhoanTacGia_BoQuaVaBaoLoi()
    {
        using var db = NewDb(withAuthor: false);
        Write("a.json", ValidJson);

        var result = await RecipeDataImporter.ImportAsync(db, _folder, null, TestContext.Current.CancellationToken);

        Assert.Equal(0, result.Imported);
        Assert.Single(result.Errors);
        Assert.Empty(db.Recipes);
    }

    [Fact]
    public async Task Import_ThieuNguonHoacNguyenLieu_BiTuChoi()
    {
        using var db = NewDb();
        Write("a.json", ValidJson.Replace("\"source\": { \"name\": \"Nguồn\", \"url\": \"https://example.com/x\" },", ""));
        Write("b.json", ValidJson.Replace("Canh thử nghiệm", "Canh khác").Replace(
            "\"ingredients\": [ { \"name\": \"Muối\", \"quantity\": 1.5, \"unit\": \"g\" }, { \"name\": \"Tiêu\" } ],", "\"ingredients\": [],"));

        var result = await RecipeDataImporter.ImportAsync(db, _folder, null, TestContext.Current.CancellationToken);

        Assert.Equal(0, result.Imported);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public async Task Import_TepJsonHong_KhongLamDungCaLo()
    {
        using var db = NewDb();
        Write("a.json", "{ khong phai json");
        Write("b.json", ValidJson);

        var result = await RecipeDataImporter.ImportAsync(db, _folder, null, TestContext.Current.CancellationToken);

        Assert.Equal(1, result.Imported);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task Import_CacTepDuLieuThatTrongRepo_HopLeVaCoDayDuQuanHe()
    {
        using var db = NewDb();
        // Mỗi tệp dùng tác giả riêng: tài khoản phải có sẵn như trong bộ seed người dùng.
        var emails = Directory.EnumerateFiles(RepoDataFolder(), "*.json")
            .Select(f => System.Text.Json.JsonDocument.Parse(File.ReadAllText(f)).RootElement.GetProperty("authorEmail").GetString()!)
            .Distinct(StringComparer.OrdinalIgnoreCase).Where(e => !e.Equals(AuthorEmail, StringComparison.OrdinalIgnoreCase));
        foreach (var email in emails)
        {
            var user = ApplicationUser.Create(email, email);
            user.NormalizedEmail = email.ToUpperInvariant();
            db.Users.Add(user);
        }
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await RecipeDataImporter.ImportAsync(db, RepoDataFolder(), null, TestContext.Current.CancellationToken);

        Assert.Empty(result.Errors);
        Assert.True(result.Imported >= 20);
        Assert.True(await db.Recipes.Select(r => r.AuthorId).Distinct().CountAsync(TestContext.Current.CancellationToken) >= 3, "cần nhiều tác giả khác nhau");
        var recipes = await db.Recipes.Include(r => r.Ingredients).Include(r => r.Steps).ToListAsync(TestContext.Current.CancellationToken);
        var userIds = await db.Users.Select(u => u.Id).ToListAsync(TestContext.Current.CancellationToken);
        var categoryIds = await db.Categories.Select(c => c.Id).ToListAsync(TestContext.Current.CancellationToken);
        Assert.All(recipes, r =>
        {
            Assert.Contains(r.AuthorId, userIds);
            Assert.Contains(r.CategoryId, categoryIds);
            Assert.NotEmpty(r.Ingredients);
            Assert.NotEmpty(r.Steps);
            Assert.DoesNotContain("Nguồn tham khảo:", r.Description);
        });
    }
}
