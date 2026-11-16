namespace CulinaryBlog.UnitTests.Recipes;

using System;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes.Queries.GetRecipes;
using CulinaryBlog.Domain.Enums;
using FluentValidation.TestHelper;
using Xunit;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Unit Tests kiểm tra tính toàn vẹn và các quy tắc của GetRecipesQuery và GetRecipesQueryValidator theo FR-SRCH-002/003/004 và API-11/12/14.
/// </summary>
public class GetRecipesQueryTests
{
    private readonly GetRecipesQueryValidator _validator = new();

    [Fact]
    public void GetRecipesQuery_ShouldInitializeWithDefaultValues()
    {
        // Act
        var query = new GetRecipesQuery();

        // Assert
        Assert.Equal(1, query.Page);
        Assert.Equal(12, query.PageSize);
        Assert.Null(query.CategoryId);
        Assert.Null(query.Difficulty);
        Assert.Null(query.MaxPrepTime);
        Assert.Null(query.MaxCookTime);
        Assert.Null(query.MaxTotalTime);
        Assert.Null(query.MinCalories);
        Assert.Null(query.MaxCalories);
        Assert.Null(query.SortBy);
        Assert.Null(query.SortOrder);
        Assert.Null(query.Mine);
        Assert.Null(query.Status);
        Assert.Null(query.AuthorId);
    }

    [Fact]
    public void GetRecipesQuery_ShouldAcceptAllFilterAndSortingParameters()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        // Act
        var query = new GetRecipesQuery(
            Page: 2,
            PageSize: 20,
            CategoryId: categoryId,
            Difficulty: RecipeDifficulty.Medium,
            MaxPrepTime: 15,
            MaxCookTime: 45,
            MaxTotalTime: 60,
            MinCalories: 200m,
            MaxCalories: 600m,
            SortBy: "totalTime",
            SortOrder: "asc",
            Mine: true,
            Status: RecipeStatus.Draft,
            AuthorId: "author-123");

        // Assert
        Assert.Equal(2, query.Page);
        Assert.Equal(20, query.PageSize);
        Assert.Equal(categoryId, query.CategoryId);
        Assert.Equal(RecipeDifficulty.Medium, query.Difficulty);
        Assert.Equal(15, query.MaxPrepTime);
        Assert.Equal(45, query.MaxCookTime);
        Assert.Equal(60, query.MaxTotalTime);
        Assert.Equal(200m, query.MinCalories);
        Assert.Equal(600m, query.MaxCalories);
        Assert.Equal("totalTime", query.SortBy);
        Assert.Equal("asc", query.SortOrder);
        Assert.True(query.Mine);
        Assert.Equal(RecipeStatus.Draft, query.Status);
        Assert.Equal("author-123", query.AuthorId);
    }

    [Theory]
    [InlineData("publishedat")]
    [InlineData("newest")]
    [InlineData("createdat")]
    [InlineData("updatedat")]
    [InlineData("title")]
    [InlineData("preptime")]
    [InlineData("cooktime")]
    [InlineData("servings")]
    [InlineData("totaltime")]
    [InlineData("quickest")]
    [InlineData("calories")]
    public void Validator_ShouldPass_ForAllowedSortByValues(string sortBy)
    {
        // Arrange
        var query = new GetRecipesQuery(SortBy: sortBy);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SortBy);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenSortByIsInvalid()
    {
        // Arrange
        var query = new GetRecipesQuery(SortBy: "invalid_column");

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SortBy);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenDifficultyIsOutOfEnumRange()
    {
        // Arrange
        var query = new GetRecipesQuery(Difficulty: (RecipeDifficulty)99);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Difficulty)
            .WithErrorMessage("Mức độ khó không hợp lệ.");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenStatusIsOutOfEnumRange()
    {
        // Arrange
        var query = new GetRecipesQuery(Status: (RecipeStatus)99);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("Trạng thái công thức không hợp lệ.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validator_ShouldHaveError_WhenPageIsInvalid(int invalidPage)
    {
        // Arrange
        var query = new GetRecipesQuery(Page: invalidPage);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void Validator_ShouldHaveError_WhenPageSizeIsOutOfBounds(int invalidPageSize)
    {
        // Arrange
        var query = new GetRecipesQuery(PageSize: invalidPageSize);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenMinCaloriesGreaterThanMaxCalories()
    {
        // Arrange
        var query = new GetRecipesQuery(MinCalories: 800m, MaxCalories: 300m);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor("CaloriesRange");
    }

    [Theory]
    [InlineData(100, 10, 10)]
    [InlineData(105, 10, 11)]
    [InlineData(0, 10, 0)]
    [InlineData(5, 12, 1)]
    [InlineData(25, 12, 3)]
    public void PagedResult_ShouldCalculateTotalPagesCorrectly(int total, int pageSize, int expectedTotalPages)
    {
        // Act
        var pagedResult = new PagedResult<string>(Array.Empty<string>(), total, 1, pageSize);

        // Assert
        Assert.Equal(total, pagedResult.Total);
        Assert.Equal(pageSize, pagedResult.PageSize);
        Assert.Equal(expectedTotalPages, pagedResult.TotalPages);
    }
    [Theory]
    [InlineData("quickest", "asc", "Alpha")]
    [InlineData("quickest", "desc", "Beta")]
    [InlineData("prepTime", "asc", "Alpha")]
    [InlineData("prepTime", "desc", "Beta")]
    [InlineData("servings", "asc", "Alpha")]
    [InlineData("servings", "desc", "Beta")]
    [InlineData("cookTime", "asc", "Alpha")]
    [InlineData("cookTime", "desc", "Beta")]
    [InlineData("newest", "asc", "Alpha")]
    [InlineData("newest", "desc", "Beta")]
    [InlineData("title", "asc", "Alpha")]
    [InlineData("title", "desc", "Beta")]
    public async Task Handler_OrdersPublishedRecipes(string sortBy, string order, string first)
    {
        using var db = DiscoveryDb();
        var result = await Handler(db).Handle(new(SortBy: sortBy, SortOrder: order), TestContext.Current.CancellationToken);
        Assert.Equal(2, result.Total);
        Assert.Equal(first, result.Items[0].Title);
        Assert.All(result.Items, recipe => Assert.Equal(RecipeStatus.Published, recipe.Status));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DefaultSort_UsesCreationDate_ForPublicAndOwnedLists(bool mine)
    {
        using var db = DiscoveryDb();
        var alpha = db.Recipes.Single(recipe => recipe.Title == "Alpha");
        var beta = db.Recipes.Single(recipe => recipe.Title == "Beta");
        alpha.CreatedAt = DateTimeOffset.UtcNow;
        beta.CreatedAt = alpha.CreatedAt.AddDays(-2);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var descending = await Handler(db).Handle(new(Mine: mine, Status: RecipeStatus.Published), TestContext.Current.CancellationToken);
        Assert.Equal("Alpha", descending.Items[0].Title);
        var ascending = await Handler(db).Handle(new(Mine: mine, Status: RecipeStatus.Published, SortOrder: "asc"), TestContext.Current.CancellationToken);
        Assert.Equal("Beta", ascending.Items[0].Title);
    }

    [Fact]
    public async Task EmptyPrivateAndAuthorFilters_DoNotPoisonPublicCache()
    {
        using var db = DiscoveryDb();
        var cache = new DiscoveryCache();
        var privateResult = await Handler(db, cache).Handle(new(Mine: true, Status: RecipeStatus.Archived), TestContext.Current.CancellationToken);
        Assert.Empty(privateResult.Items);
        Assert.Equal(0, cache.Writes);
        var admin = Handler(db, cache, new DiscoveryUser("admin", true));
        Assert.Empty((await admin.Handle(new(AuthorId: "missing-author"), TestContext.Current.CancellationToken)).Items);
        Assert.Equal(0, cache.Writes);
        Assert.Equal(2, (await Handler(db, cache).Handle(new(), TestContext.Current.CancellationToken)).Total);
    }

    [Fact]
    public async Task AuthorFilter_RequiresAdmin()
    {
        using var db = DiscoveryDb();
        await Assert.ThrowsAsync<CulinaryBlog.Application.Common.Exceptions.ForbiddenException>(() =>
            Handler(db).Handle(new(AuthorId: "author"), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Mine_RequiresLoginAndSearchesOnlyOwnedRecipes()
    {
        using var db = DiscoveryDb();
        await Assert.ThrowsAsync<CulinaryBlog.Application.Common.Exceptions.UnauthorizedException>(() =>
            Handler(db, user: new DiscoveryUser(null)).Handle(new(Mine: true), TestContext.Current.CancellationToken));
        var result = await Handler(db).Handle(new(Mine: true, Q: "alpha"), TestContext.Current.CancellationToken);
        Assert.Equal("Alpha", Assert.Single(result.Items).Title);
    }

    [Theory]
    [InlineData(null, 2)]
    [InlineData("author", 3)]
    public async Task CategoryPagination_IncludesOnlyPermittedDrafts_WithoutSharingPrivateCache(string? userId, int expected)
    {
        using var db = DiscoveryDb();
        var cache = new DiscoveryCache();
        var user = new DiscoveryUser(userId);
        var handler = new CulinaryBlog.Application.Features.Categories.Queries.GetCategoryBySlug.GetCategoryBySlugQueryHandler(
            db, Microsoft.Extensions.Logging.Abstractions.NullLogger<CulinaryBlog.Application.Features.Categories.Queries.GetCategoryBySlug.GetCategoryBySlugQueryHandler>.Instance,
            cache, user, new DiscoveryAuthors());
        var category = await handler.Handle(new("com", 1, 1), TestContext.Current.CancellationToken);
        Assert.NotNull(category);
        Assert.Single(category.Recipes.Items);
        Assert.Equal(expected, category.Recipes.Total);
        Assert.Equal(2, category.RecipeCount);
        var guest = new CulinaryBlog.Application.Features.Categories.Queries.GetCategoryBySlug.GetCategoryBySlugQueryHandler(
            db, Microsoft.Extensions.Logging.Abstractions.NullLogger<CulinaryBlog.Application.Features.Categories.Queries.GetCategoryBySlug.GetCategoryBySlugQueryHandler>.Instance,
            cache, new DiscoveryUser(null), new DiscoveryAuthors());
        Assert.Equal(2, (await guest.Handle(new("com"), TestContext.Current.CancellationToken))!.Recipes.Total);
    }

    private static CulinaryBlog.Infrastructure.Persistence.CulinaryBlogDbContext DiscoveryDb()
    {
        var db = new CulinaryBlog.Infrastructure.Persistence.CulinaryBlogDbContext(
            new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<CulinaryBlog.Infrastructure.Persistence.CulinaryBlogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var categoryId = Guid.NewGuid();
        db.Categories.Add(new() { Id = categoryId, Name = "Cơm", Slug = "com" });
        db.Recipes.AddRange(
            new() { Id = Guid.NewGuid(), CategoryId = categoryId, AuthorId = "author", Title = "Alpha", Slug = "alpha", PrepTimeMinutes = 5, CookTimeMinutes = 5, Servings = 2, Status = RecipeStatus.Published, Difficulty = RecipeDifficulty.Easy, PublishedAt = DateTimeOffset.UtcNow.AddDays(-1) },
            new() { Id = Guid.NewGuid(), CategoryId = categoryId, AuthorId = "author", Title = "Beta", Slug = "beta", PrepTimeMinutes = 40, CookTimeMinutes = 50, Servings = 6, Status = RecipeStatus.Published, Difficulty = RecipeDifficulty.Medium, PublishedAt = DateTimeOffset.UtcNow },
            new() { Id = Guid.NewGuid(), CategoryId = categoryId, AuthorId = "author", Title = "Gamma", Slug = "gamma", Status = RecipeStatus.Draft });
        db.SaveChanges();
        return db;
    }

    private static GetRecipesQueryHandler Handler(CulinaryBlog.Infrastructure.Persistence.CulinaryBlogDbContext db,
        DiscoveryCache? cache = null, DiscoveryUser? user = null) => new(db, new DiscoveryAuthors(), user ?? new("author"),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<GetRecipesQueryHandler>.Instance, cache);

    private sealed record DiscoveryUser(string? UserId, bool IsAdmin = false) : CulinaryBlog.Application.Common.Interfaces.ICurrentUserService;
    private sealed class DiscoveryAuthors : CulinaryBlog.Application.Common.Interfaces.IUserQueryService
    {
        public Task<IReadOnlyDictionary<string, CulinaryBlog.Application.Common.Models.AuthorSummary>> GetAuthorSummariesAsync(IEnumerable<string> ids, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyDictionary<string, CulinaryBlog.Application.Common.Models.AuthorSummary>>(new Dictionary<string, CulinaryBlog.Application.Common.Models.AuthorSummary>());
    }
    private sealed class DiscoveryCache : CulinaryBlog.Application.Common.Interfaces.ICacheService
    {
        private readonly Dictionary<string, object> _values = [];
        public int Writes { get; private set; }
        public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) => Task.FromResult(_values.TryGetValue(key, out var value) ? (T?)value : default);
        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default) { _values[key] = value!; Writes++; return Task.CompletedTask; }
        public Task RemoveAsync(string key, CancellationToken ct = default) { _values.Remove(key); return Task.CompletedTask; }
        public Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default) { foreach (var key in _values.Keys.Where(k => k.StartsWith(prefix)).ToList()) _values.Remove(key); return Task.CompletedTask; }
        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken ct = default) => await factory();
    }}
