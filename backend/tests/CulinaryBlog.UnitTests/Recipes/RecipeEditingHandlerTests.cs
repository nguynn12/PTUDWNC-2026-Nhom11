using CulinaryBlog.Application.Common.Constants;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Recipes.Commands.CreateRecipe;
using CulinaryBlog.Application.Features.Recipes.Commands.UpdateRecipe;
using CulinaryBlog.Application.Features.Recipes.DTOs;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Repositories;
using CulinaryBlog.Infrastructure.Services.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static CulinaryBlog.UnitTests.Support.HandlerTestSupport;

namespace CulinaryBlog.UnitTests.Recipes;

public sealed class RecipeEditingHandlerTests
{
    private const string Owner = "author-owner";

    [Fact]
    public async Task Create_UsesAuthenticatedAuthor_AndSkipsCurrentAndHistoricalSlugs()
    {
        await using var db = CreateContext();
        var category = new Category { Name = "Món nước", Slug = "mon-nuoc" };
        var existing = new Recipe { CategoryId = category.Id, Slug = "chao-ga", AuthorId = Owner };
        db.AddRange(category, existing, new RecipeSlugHistory { RecipeId = existing.Id, OldSlug = "chao-ga-1" });
        await db.SaveChangesAsync();

        var result = await CreateHandler(db, new User(Owner)).Handle(new CreateRecipeCommand
        {
            Title = "  Cháo gà  ", Description = "  Cháo gà nấu gừng  ",
            CategoryId = category.Id, AuthorId = "different-author", PrepTimeMinutes = 15,
            CookTimeMinutes = 30, Servings = 4, Difficulty = RecipeDifficulty.Medium,
            Nutrition = new RecipeNutritionDto { Calories = 0, Protein = 7.25m, Source = "AutoCalculated" }
        }, CancellationToken.None);

        var saved = await db.Recipes.AsNoTracking().SingleAsync(recipe => recipe.Id == result.Id);
        Assert.Equal("chao-ga-2", saved.Slug);
        Assert.Equal("Cháo gà", saved.Title);
        Assert.Equal("Cháo gà nấu gừng", saved.Description);
        Assert.Equal(Owner, result.AuthorId);
        Assert.Equal(Owner, saved.AuthorId);
        Assert.Equal(RecipeStatus.Draft, result.Status);
        Assert.Null(result.PublishedAt);
        Assert.Equal(4, saved.Servings);
        Assert.Equal(RecipeDifficulty.Medium, saved.Difficulty);
        Assert.Equal(0m, saved.Nutrition?.Calories);
        Assert.Equal(7.25m, result.Nutrition?.Protein);
        Assert.Equal(NutritionSource.Manual, saved.Nutrition?.Source);
        Assert.Equal("Manual", result.Nutrition?.Source);
    }

    [Fact]
    public async Task Create_WithoutNutrition_LeavesNutritionUnset()
    {
        await using var db = CreateContext();
        var category = new Category { Name = "Món nước", Slug = "mon-nuoc" };
        db.Add(category);
        await db.SaveChangesAsync();

        var result = await CreateHandler(db, new User(Owner)).Handle(new CreateRecipeCommand
        {
            CategoryId = category.Id, Title = "Cháo gà", Description = "Cháo gà nấu gừng", PrepTimeMinutes = 15, Servings = 4
        }, CancellationToken.None);

        Assert.Null(result.Nutrition);
        Assert.Null((await db.Recipes.AsNoTracking().SingleAsync()).Nutrition);
    }

    [Fact]
    public async Task Create_RequiresAuthorAndAnExistingCategory()
    {
        await using var db = CreateContext();
        var request = new CreateRecipeCommand { CategoryId = Guid.NewGuid(), Title = "Cháo gà" };

        var unauthorized = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            CreateHandler(db, new User(null)).Handle(request, CancellationToken.None));
        Assert.Equal("AUTH_UNAUTHORIZED", unauthorized.ErrorCode);

        var missing = await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler(db, new User(Owner)).Handle(request, CancellationToken.None));
        Assert.Equal("CATEGORY_NOT_FOUND", missing.ErrorCode);
        Assert.Empty(await db.Recipes.ToListAsync());
    }

    [Fact]
    public async Task Update_RenamesNeverPublishedDraft_AndPersistsHistoryCategoryAndNutrition()
    {
        await using var db = CreateContext();
        var recipe = await SeedRecipeAsync(db);
        var replacement = new Category { Name = "Món ăn sáng", Slug = "mon-an-sang" };
        db.AddRange(replacement, new RecipeSlugHistory { RecipeId = recipe.Id, OldSlug = "chao-ga-gung" });
        await db.SaveChangesAsync();
        var cache = new Cache();
        var key = $"{CacheConstants.RecipeDetailPrefix}{recipe.Id}";
        await cache.SetAsync(key, "old description");

        var result = await UpdateHandler(db, new User(Owner), cache).Handle(Edit(recipe) with
        {
            CategoryId = replacement.Id,
            Title = "  Cháo gà gừng  ",
            Nutrition = new RecipeNutritionDto { Calories = 0, Protein = 7.25m, Fat = 3.5m }
        }, CancellationToken.None);

        var saved = await db.Recipes.AsNoTracking().SingleAsync();
        Assert.Equal("chao-ga-gung-1", result.Slug);
        Assert.Equal(result.Slug, saved.Slug);
        Assert.Equal(replacement.Id, saved.CategoryId);
        Assert.Equal(RecipeStatus.Draft, saved.Status);
        Assert.Equal(0m, saved.Nutrition?.Calories);
        Assert.Equal(7.25m, saved.Nutrition?.Protein);
        Assert.Equal(3.5m, result.Nutrition?.Fat);
        Assert.Equal("Manual", result.Nutrition?.Source);
        Assert.Contains(await db.RecipeSlugHistories.ToListAsync(),
            history => history.OldSlug == "chao-ga" && history.RecipeId == saved.Id);
        Assert.Null(await cache.GetAsync<string>(key));
    }

    [Theory]
    [InlineData(RecipeStatus.Draft)]
    [InlineData(RecipeStatus.Published)]
    [InlineData(RecipeStatus.Archived)]
    public async Task Update_AsAdmin_PreservesPreviouslyPublishedSlugAndClearsNutrition(RecipeStatus status)
    {
        await using var db = CreateContext();
        var recipe = await SeedRecipeAsync(db);
        recipe.Status = status;
        recipe.PublishedAt = DateTimeOffset.UtcNow.AddDays(-2);
        recipe.Nutrition = new CulinaryBlog.Domain.ValueObjects.RecipeNutrition { Protein = 4m };
        await db.SaveChangesAsync();

        var result = await UpdateHandler(db, new User("admin", true)).Handle(
            Edit(recipe) with { Title = "Cháo gà hành", Nutrition = null }, CancellationToken.None);

        var saved = await db.Recipes.AsNoTracking().SingleAsync();
        Assert.Equal("Cháo gà hành", result.Title);
        Assert.Equal("chao-ga", saved.Slug);
        Assert.Equal(status, saved.Status);
        Assert.Equal(recipe.PublishedAt, saved.PublishedAt);
        Assert.Null(result.Nutrition);
        Assert.Null(saved.Nutrition);
        Assert.Empty(await db.RecipeSlugHistories.ToListAsync());
    }

    [Fact]
    public async Task Update_RejectsMissingRecipeAndMissingCategory()
    {
        await using var db = CreateContext();
        var recipe = await SeedRecipeAsync(db);
        var handler = UpdateHandler(db, new User(Owner));

        var missingRecipe = await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(Edit(recipe) with { Id = Guid.NewGuid() }, CancellationToken.None));
        Assert.Equal("RECIPE_NOT_FOUND", missingRecipe.ErrorCode);

        var missingCategory = await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(Edit(recipe) with { CategoryId = Guid.NewGuid() }, CancellationToken.None));
        Assert.Equal("CATEGORY_NOT_FOUND", missingCategory.ErrorCode);
        Assert.Equal(recipe.CategoryId, (await db.Recipes.AsNoTracking().SingleAsync()).CategoryId);
    }

    [Fact]
    public async Task Update_RejectsAnonymousAndOtherAuthors()
    {
        await using var db = CreateContext();
        var recipe = await SeedRecipeAsync(db);
        var edit = Edit(recipe) with { Title = "Cháo gà gừng" };

        var anonymous = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            UpdateHandler(db, new User(null)).Handle(edit, CancellationToken.None));
        Assert.Equal("AUTH_UNAUTHORIZED", anonymous.ErrorCode);

        var otherAuthor = await Assert.ThrowsAsync<ForbiddenException>(() =>
            UpdateHandler(db, new User("other-author")).Handle(edit, CancellationToken.None));
        Assert.Equal("RECIPE_FORBIDDEN", otherAuthor.ErrorCode);
        Assert.Equal("Cháo gà", (await db.Recipes.AsNoTracking().SingleAsync()).Title);
        Assert.Empty(await db.RecipeSlugHistories.ToListAsync());
    }

    [Fact]
    public async Task Update_RejectsStaleVersionBeforeChangingStoredData()
    {
        await using var db = CreateContext();
        var recipe = await SeedRecipeAsync(db);
        db.Entry(recipe).Property<uint>("xmin").CurrentValue = 500u;
        await db.SaveChangesAsync();

        var error = await Assert.ThrowsAsync<ConflictException>(() =>
            UpdateHandler(db, new User(Owner)).Handle(
                Edit(recipe) with { ConcurrencyToken = 499u, Title = "Cháo gà gừng" }, CancellationToken.None));

        Assert.Equal("RECIPE_CONCURRENCY_CONFLICT", error.ErrorCode);
        Assert.Equal("Cháo gà", (await db.Recipes.AsNoTracking().SingleAsync()).Title);
        Assert.Empty(await db.RecipeSlugHistories.ToListAsync());
    }

    private static CreateRecipeCommandHandler CreateHandler(CulinaryBlogDbContext db, User user) =>
        new(new EfRepository<Recipe>(db), new UnitOfWork(db), db, user);

    private static UpdateRecipeCommandHandler UpdateHandler(CulinaryBlogDbContext db, User user, Cache? cache = null) =>
        new(new EfRepository<Recipe>(db), new EfRepository<RecipeSlugHistory>(db),
            new UnitOfWork(db), db, user, cache is null ? null : new CacheInvalidator(cache, NullLogger<CacheInvalidator>.Instance));

    private static async Task<Recipe> SeedRecipeAsync(CulinaryBlogDbContext db)
    {
        var category = new Category { Name = "Món nước", Slug = "mon-nuoc" };
        var recipe = new Recipe
        {
            CategoryId = category.Id, Title = "Cháo gà", Slug = "chao-ga", AuthorId = Owner
        };
        db.AddRange(category, recipe);
        await db.SaveChangesAsync();
        return recipe;
    }

    private static UpdateRecipeCommand Edit(Recipe recipe) => new()
    {
        Id = recipe.Id, CategoryId = recipe.CategoryId, Title = recipe.Title,
        Description = "Cháo gà nấu gừng", PrepTimeMinutes = 15, CookTimeMinutes = 30, Servings = 4
    };
}
