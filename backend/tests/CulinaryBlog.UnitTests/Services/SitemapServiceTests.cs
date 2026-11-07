using System.Xml.Linq;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Services.Sitemap;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CulinaryBlog.UnitTests.Services;

public class SitemapServiceTests
{
    private CulinaryBlogDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<CulinaryBlogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new CulinaryBlogDbContext(options);
    }

    [Fact]
    public async Task GenerateSitemapXmlAsync_Includes_Published_Recipes_And_Excludes_Draft_Archived_Deleted()
    {
        var db = CreateInMemoryDbContext();

        // 1. Thêm categories
        var cat1 = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Món Khai Vị",
            Slug = "mon-khai-vi",
            OrderIndex = 1,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var catDeleted = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Danh Mục Cũ",
            Slug = "danh-muc-cu",
            OrderIndex = 2,
            IsDeleted = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Categories.AddRange(cat1, catDeleted);

        // 2. Thêm recipes với các trạng thái khác nhau
        var recipePublished = new Recipe
        {
            Id = Guid.NewGuid(),
            Title = "Gỏi Cuốn Tôm Thịt",
            Slug = "goi-cuon-tom-thit",
            Status = RecipeStatus.Published,
            IsDeleted = false,
            CategoryId = cat1.Id,
            AuthorId = "author-1",
            PublishedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var recipeDraft = new Recipe
        {
            Id = Guid.NewGuid(),
            Title = "Bánh Mì Nháp",
            Slug = "banh-mi-nhap",
            Status = RecipeStatus.Draft,
            IsDeleted = false,
            CategoryId = cat1.Id,
            AuthorId = "author-1",
            CreatedAt = DateTimeOffset.UtcNow
        };
        var recipeArchived = new Recipe
        {
            Id = Guid.NewGuid(),
            Title = "Chè Ba Màu Cũ",
            Slug = "che-ba-mau-cu",
            Status = RecipeStatus.Archived,
            IsDeleted = false,
            CategoryId = cat1.Id,
            AuthorId = "author-1",
            CreatedAt = DateTimeOffset.UtcNow
        };
        var recipeDeleted = new Recipe
        {
            Id = Guid.NewGuid(),
            Title = "Món Đã Xóa",
            Slug = "mon-da-xoa",
            Status = RecipeStatus.Published,
            IsDeleted = true,
            CategoryId = cat1.Id,
            AuthorId = "author-1",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Recipes.AddRange(recipePublished, recipeDraft, recipeArchived, recipeDeleted);
        await db.SaveChangesAsync();

        var inMemoryConfig = new Dictionary<string, string?>
        {
            ["ClientApp:BaseUrl"] = "https://culinaryblog.com"
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig).Build();

        var service = new SitemapService(db, configuration, NullLogger<SitemapService>.Instance);

        // Act
        var xml = await service.GenerateSitemapXmlAsync();

        // Assert
        Assert.NotNull(xml);
        var doc = XDocument.Parse(xml);
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var locs = doc.Descendants(ns + "loc").Select(x => x.Value).ToList();

        // 1. Phải chứa trang tĩnh
        Assert.Contains("https://culinaryblog.com/", locs);
        Assert.Contains("https://culinaryblog.com/recipes", locs);
        Assert.Contains("https://culinaryblog.com/categories", locs);

        // 2. Phải chứa Category chưa xóa
        Assert.Contains("https://culinaryblog.com/categories/mon-khai-vi", locs);
        Assert.DoesNotContain("https://culinaryblog.com/categories/danh-muc-cu", locs);

        // 3. Phải CHỨA Published recipe
        Assert.Contains("https://culinaryblog.com/recipes/goi-cuon-tom-thit", locs);

        // 4. Tuyệt đối KHÔNG ĐƯỢC CHỨA Draft, Archived, Deleted theo NFR-SEO
        Assert.DoesNotContain("https://culinaryblog.com/recipes/banh-mi-nhap", locs);
        Assert.DoesNotContain("https://culinaryblog.com/recipes/che-ba-mau-cu", locs);
        Assert.DoesNotContain("https://culinaryblog.com/recipes/mon-da-xoa", locs);
    }
}

