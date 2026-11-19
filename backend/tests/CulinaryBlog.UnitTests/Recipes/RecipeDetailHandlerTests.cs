using CulinaryBlog.Application.Common.Constants;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes.DTOs;
using CulinaryBlog.Application.Features.Recipes.Queries.GetRecipeBySlug;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.ValueObjects;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static CulinaryBlog.UnitTests.Support.HandlerTestSupport;

namespace CulinaryBlog.UnitTests.Recipes;

public sealed class RecipeDetailHandlerTests
{
    private const string Owner = "author-owner";

    [Fact]
    public async Task PublishedDetail_MapsChildrenInDisplayOrder_AndCachesBySlugAndId()
    {
        await using var db = CreateContext();
        var category = new Category { Name = "Món nước", Slug = "mon-nuoc" };
        var recipe = MakeRecipe(RecipeStatus.Published);
        recipe.CategoryId = category.Id;
        recipe.Nutrition = new RecipeNutrition
        {
            Calories = 0m, Protein = 7.25m, Fat = 3m, Carbohydrates = 2m, Fiber = 1m, Sodium = 125m
        };
        recipe.Ingredients =
        [
            new() { Name = "Gừng", OrderIndex = 2, Quantity = 0.5m, Unit = "g", Notes = "Thái lát" },
            new() { Name = "Gạo", OrderIndex = 1 },
            new() { Name = "Nguyên liệu đã xóa", IsDeleted = true }
        ];
        recipe.Steps =
        [
            new() { StepNumber = 2, Title = "Nấu", Description = "Nấu trên lửa nhỏ", DurationMinutes = 20 },
            new() { StepNumber = 1, Title = "Sơ chế" },
            new() { StepNumber = 3, Title = "Bước đã xóa", IsDeleted = true }
        ];
        recipe.Images =
        [
            new() { OrderIndex = 2, OriginalUrl = "https://img.test/second.jpg" },
            new()
            {
                OrderIndex = 1, OriginalUrl = "https://img.test/original.jpg", MediumUrl = "https://img.test/medium.webp",
                ThumbnailUrl = "https://img.test/thumb.webp", IsPrimary = true, AltText = "Tô cháo"
            },
            new() { OriginalUrl = "https://img.test/deleted.jpg", IsDeleted = true }
        ];
        db.AddRange(category, recipe);
        db.Entry(recipe).Property<uint>("xmin").CurrentValue = 42u;
        await db.SaveChangesAsync();

        var authors = new Authors(new AuthorSummary(Owner, "Tác giả", "https://img.test/author.jpg"));
        var cache = new Cache();
        var handler = MakeHandler(db, authors, cache);
        var result = await handler.Handle(new GetRecipeBySlugQuery("  CHAO-GA  "), CancellationToken.None);

        var detail = Assert.IsType<RecipeDetailDto>(result.Recipe);
        Assert.Equal(42u, detail.ConcurrencyToken);
        Assert.Equal(category.Id, detail.Category?.Id);
        Assert.Equal("Tác giả", detail.Author?.DisplayName);
        Assert.Equal(0m, detail.Nutrition?.Calories);
        Assert.Equal(7.25m, detail.Nutrition?.Protein);
        Assert.Equal("Manual", detail.Nutrition?.Source);
        Assert.Equal(new[] { "Gạo", "Gừng" }, detail.Ingredients.Select(item => item.Name));
        Assert.Equal(0.5m, detail.Ingredients[1].Quantity);
        Assert.Equal(new[] { "Sơ chế", "Nấu" }, detail.Steps.Select(step => step.Title));
        Assert.Equal(20, detail.Steps[1].DurationMinutes);
        Assert.Equal(2, detail.Images.Count);
        Assert.True(detail.Images[0].IsPrimary);
        Assert.Equal("https://img.test/medium.webp", detail.Images[0].MediumUrl);
        Assert.Equal(new[] { Owner }, Assert.Single(authors.Lookups));
        Assert.Contains((CacheConstants.RecipeSlugPrefix + recipe.Slug, CacheConstants.RecipeDetailDuration), cache.Writes);
        Assert.Contains((CacheConstants.RecipeDetailPrefix + recipe.Id, CacheConstants.RecipeDetailDuration), cache.Writes);

        var cached = await handler.Handle(new GetRecipeBySlugQuery(recipe.Slug), CancellationToken.None);
        Assert.Same(detail, cached.Recipe);
        Assert.Single(authors.Lookups);
    }

    [Theory]
    [InlineData(RecipeStatus.Draft, null, false, false)]
    [InlineData(RecipeStatus.Draft, Owner, false, true)]
    [InlineData(RecipeStatus.Archived, "other-author", false, false)]
    [InlineData(RecipeStatus.Archived, "admin", true, true)]
    public async Task PrivateDetail_IsVisibleOnlyToOwnerOrAdmin_AndNeverStoredInSharedCache(
        RecipeStatus status, string? userId, bool admin, bool visible)
    {
        await using var db = CreateContext();
        var category = new Category { Name = "Món nước", Slug = "mon-nuoc" };
        var recipe = MakeRecipe(status);
        recipe.CategoryId = category.Id;
        db.AddRange(category, recipe);
        await db.SaveChangesAsync();
        var cache = new Cache();
        var authors = new Authors();

        var result = await MakeHandler(db, authors, cache, new User(userId, admin))
            .Handle(new GetRecipeBySlugQuery(recipe.Slug), CancellationToken.None);

        if (visible)
        {
            Assert.Equal(recipe.Id, result.Recipe?.Id);
            Assert.Equal(status, result.Recipe?.Status);
            Assert.Null(result.Recipe?.Nutrition);
            Assert.Null(result.Recipe?.Author);
            Assert.Single(authors.Lookups);
        }
        else
        {
            Assert.Null(result.Recipe);
            Assert.False(result.IsRedirect);
            Assert.Empty(authors.Lookups);
        }
        Assert.Empty(cache.Writes);
    }

    [Theory]
    [InlineData(RecipeStatus.Published, false, true)]
    [InlineData(RecipeStatus.Draft, false, false)]
    [InlineData(RecipeStatus.Published, true, false)]
    public async Task HistoricSlug_RedirectsOnlyWhenTargetIsPublicAndNotDeleted(
        RecipeStatus status, bool isDeleted, bool redirects)
    {
        await using var db = CreateContext();
        var recipe = MakeRecipe(status);
        recipe.IsDeleted = isDeleted;
        db.AddRange(recipe, new RecipeSlugHistory { RecipeId = recipe.Id, OldSlug = "old-chao" });
        await db.SaveChangesAsync();
        var authors = new Authors();

        var result = await MakeHandler(db, authors).Handle(new GetRecipeBySlugQuery("old-chao"), CancellationToken.None);

        Assert.Equal(redirects ? recipe.Slug : null, result.RedirectSlug);
        Assert.Null(result.Recipe);
        Assert.Empty(authors.Lookups);
    }

    private static GetRecipeBySlugQueryHandler MakeHandler(
        CulinaryBlogDbContext db, Authors authors, Cache? cache = null, User? user = null) =>
        new(db, authors, NullLogger<GetRecipeBySlugQueryHandler>.Instance, cache, user);

    private static Recipe MakeRecipe(RecipeStatus status) =>
        new() { Title = "Cháo gà", Slug = "chao-ga", Status = status, AuthorId = Owner };
}
