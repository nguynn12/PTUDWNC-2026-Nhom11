using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Constants;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CulinaryBlog.IntegrationTests.Infrastructure;

/// <summary>
/// WebApplicationFactory tùy biến cho toàn bộ kịch bản Integration Test của nhóm 11.
/// Sử dụng InMemory Database biệt lập để test độc lập không phụ thuộc DB ngoài và nạp TestAuthHandler.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "IntegrationTestDb_" + Guid.NewGuid();
    private bool _databaseSeeded;
    private readonly object _lock = new();

    // Dữ liệu mẫu ID cố định để dễ dàng assert trong test case
    public static readonly Guid AppetizerCategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid MainDishCategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid PublishedRecipeId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid DraftRecipeId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly string AuthorUserId = "00000000-0000-0000-0000-000000000002";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=fake_test_db;Username=test;Password=test",
                ["Jwt:SecretKey"] = "SuperSecretKeyForIntegrationTestingPurposesOnly1234567890!@#$%",
                ["Jwt:Issuer"] = "CulinaryBlogTest",
                ["Jwt:Audience"] = "CulinaryBlogTest",
                ["Jwt:AccessTokenExpirationMinutes"] = "60",
                ["Jwt:RefreshTokenExpirationDays"] = "7"
            });
        });

        builder.ConfigureServices(services =>
        {
            // 1. Gỡ bỏ đăng ký DbContext PostgreSQL hiện tại
            var dbContextDescriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<CulinaryBlogDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(CulinaryBlogDbContext) ||
                d.ServiceType == typeof(IApplicationDbContext)).ToList();

            foreach (var descriptor in dbContextDescriptors)
            {
                services.Remove(descriptor);
            }

            // 2. Thay thế bằng InMemory Database với ServiceProvider riêng biệt để không xung đột provider
            var inMemoryProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<CulinaryBlogDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName)
                       .UseInternalServiceProvider(inMemoryProvider);
            });

            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<CulinaryBlogDbContext>());

            // 3. Đăng ký Test Authentication Scheme làm Scheme mặc định
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.AuthenticationScheme,
                _ => { });
        });
    }

    public void EnsureDatabaseSeeded()
    {
        if (_databaseSeeded) return;

        lock (_lock)
        {
            if (_databaseSeeded) return;

            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
            db.Database.EnsureCreated();
            SeedIntegrationTestData(db);
            _databaseSeeded = true;
        }
    }

    private static void SeedIntegrationTestData(CulinaryBlogDbContext db)
    {
        if (db.Categories.Any()) return;

        // Seed Categories
        var cat1 = new Category
        {
            Id = AppetizerCategoryId,
            Name = "Món Khai Vị",
            Slug = "mon-khai-vi",
            Description = "Các món gỏi súp thanh đạm",
            ImageUrl = "/images/appetizer.jpg",
            OrderIndex = 1
        };

        var cat2 = new Category
        {
            Id = MainDishCategoryId,
            Name = "Món Chính",
            Slug = "mon-chinh",
            Description = "Món kho nướng đậm đà",
            ImageUrl = "/images/main.jpg",
            OrderIndex = 2
        };
        db.Categories.AddRange(cat1, cat2);

        // Seed Recipes
        var recipePublished = new Recipe
        {
            Id = PublishedRecipeId,
            AuthorId = AuthorUserId,
            CategoryId = MainDishCategoryId,
            Title = "Phở Bò Tái Lăn Hà Nội",
            Slug = "pho-bo-tai-lan-ha-noi",
            Description = "Công thức nấu phở bò gia truyền thơm ngon đậm vị",
            PrepTimeMinutes = 30,
            CookTimeMinutes = 60,
            Servings = 4,
            Difficulty = RecipeDifficulty.Medium,
            Status = RecipeStatus.Published,
            PublishedAt = DateTimeOffset.UtcNow
        };

        var recipeDraft = new Recipe
        {
            Id = DraftRecipeId,
            AuthorId = AuthorUserId,
            CategoryId = AppetizerCategoryId,
            Title = "Gỏi Cuốn Tôm Thịt Bí Mật",
            Slug = "goi-cuon-tom-thit-bi-mat",
            Description = "Bản nháp công thức gỏi cuốn đang hoàn thiện",
            PrepTimeMinutes = 20,
            CookTimeMinutes = 10,
            Servings = 2,
            Difficulty = RecipeDifficulty.Easy,
            Status = RecipeStatus.Draft
        };

        db.Recipes.AddRange(recipePublished, recipeDraft);
        db.SaveChanges();
    }

    /// <summary>Tạo HttpClient với role Admin.</summary>
    public HttpClient CreateAdminClient()
    {
        EnsureDatabaseSeeded();
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", Roles.Admin);
        return client;
    }

    /// <summary>Tạo HttpClient với role Author.</summary>
    public HttpClient CreateAuthorClient()
    {
        EnsureDatabaseSeeded();
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", Roles.Author);
        return client;
    }

    /// <summary>Tạo HttpClient là khách vãng lai (Anonymous/Guest).</summary>
    public HttpClient CreateGuestClient()
    {
        EnsureDatabaseSeeded();
        return CreateClient();
    }
}
