namespace CulinaryBlog.UnitTests.Recipes;

using System;
using CulinaryBlog.Application.Features.Recipes.Queries.SearchRecipes;
using CulinaryBlog.Domain.Enums;
using FluentValidation.TestHelper;
using Xunit;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

/// <summary>
/// Unit Tests kiểm tra tính toàn vẹn của SearchRecipesQuery và các quy tắc kiểm thực của SearchRecipesQueryValidator (FR-SRCH-001/002/003/004).
/// </summary>
public class SearchRecipesQueryTests
{
    [Theory]
    [InlineData("newest", "desc", "A")]
    [InlineData("newest", "asc", "B")]
    [InlineData("quickest", "asc", "A")]
    [InlineData("quickest", "desc", "B")]
    [InlineData("prepTime", "asc", "B")]
    [InlineData("prepTime", "desc", "A")]
    [InlineData("createdAt", "desc", "A")]
    [InlineData("servings", "desc", "B")]
    public async Task Handler_SortsAcceptedFieldsBeforePaging(string sortBy, string order, string first)
    {
        using var db = new CulinaryBlogDbContext(new DbContextOptionsBuilder<CulinaryBlogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var now = DateTimeOffset.UtcNow;
        var categoryId = Guid.NewGuid();
        db.Categories.Add(new Category { Id = categoryId, Name = "Món kiểm thử", Slug = "mon-kiem-thu" });
        db.Recipes.AddRange(
            new Recipe { Title = "Mon sort A", Slug = "mon-sort-a", AuthorId = "author", CategoryId = categoryId, Status = RecipeStatus.Published,
                PublishedAt = now, CreatedAt = now, PrepTimeMinutes = 30, CookTimeMinutes = 1, Servings = 1 },
            new Recipe { Title = "Mon sort B", Slug = "mon-sort-b", AuthorId = "author", CategoryId = categoryId, Status = RecipeStatus.Published,
                PublishedAt = now.AddDays(-1), CreatedAt = now.AddDays(-1), PrepTimeMinutes = 10, CookTimeMinutes = 30, Servings = 4 });
        await db.SaveChangesAsync();
        var handler = new SearchRecipesQueryHandler(db, new NoAuthors(), NullLogger<SearchRecipesQueryHandler>.Instance);
        var result = await handler.Handle(new SearchRecipesQuery(Q: "Mon sort", PageSize: 1, SortBy: sortBy, SortOrder: order), default);
        Assert.Equal(2, result.Total);
        Assert.Equal("Mon sort " + first, Assert.Single(result.Items).Title);
    }

    private sealed class NoAuthors : IUserQueryService
    {
        public Task<IReadOnlyDictionary<string, AuthorSummary>> GetAuthorSummariesAsync(
            IEnumerable<string> userIds, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<string, AuthorSummary>>(new Dictionary<string, AuthorSummary>());
    }

    private readonly SearchRecipesQueryValidator _validator = new();

    [Fact]
    public void SearchRecipesQuery_ShouldInitializeWithDefaultValues()
    {
        // Act
        var query = new SearchRecipesQuery();

        // Assert
        Assert.Equal(string.Empty, query.Q);
        Assert.Equal(1, query.Page);
        Assert.Equal(12, query.PageSize);
        Assert.Equal("relevance", query.SortBy);
        Assert.Equal("desc", query.SortOrder);
        Assert.Null(query.CategoryId);
        Assert.Null(query.Difficulty);
        Assert.Null(query.MaxPrepTime);
        Assert.Null(query.MaxCookTime);
        Assert.Null(query.MaxTotalTime);
        Assert.Null(query.MinCalories);
        Assert.Null(query.MaxCalories);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validator_ShouldHaveError_WhenQIsEmpty(string? q)
    {
        // Arrange
        var query = new SearchRecipesQuery(Q: q!);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Q);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenQIsShorterThan2Characters()
    {
        // Arrange
        var query = new SearchRecipesQuery(Q: "a");

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Q)
            .WithErrorMessage("Từ khóa tìm kiếm phải có độ dài tối thiểu từ 2 ký tự.");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenQExceeds100Characters()
    {
        // Arrange
        var query = new SearchRecipesQuery(Q: new string('x', 101));

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Q)
            .WithErrorMessage("Từ khóa tìm kiếm không được vượt quá 100 ký tự.");
    }

    [Fact]
    public void Validator_ShouldPass_WhenQueryIsValid()
    {
        // Arrange
        var query = new SearchRecipesQuery(
            Q: "Phở bò Hà Nội",
            Page: 2,
            PageSize: 20,
            CategoryId: Guid.NewGuid(),
            Difficulty: RecipeDifficulty.Medium,
            MaxPrepTime: 30,
            MaxCookTime: 60,
            MaxTotalTime: 90,
            MinCalories: 200m,
            MaxCalories: 800m,
            SortBy: "relevance",
            SortOrder: "desc");

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("relevance")]
    [InlineData("publishedAt")]
    [InlineData("newest")]
    [InlineData("title")]
    [InlineData("prepTime")]
    [InlineData("cookTime")]
    [InlineData("servings")]
    [InlineData("totalTime")]
    [InlineData("quickest")]
    [InlineData("calories")]
    public void Validator_ShouldPass_ForAllowedSortByValues(string sortBy)
    {
        // Arrange
        var query = new SearchRecipesQuery(Q: "Món ngon", SortBy: sortBy);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SortBy);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenSortByIsNotInWhitelist()
    {
        // Arrange
        var query = new SearchRecipesQuery(Q: "Món ngon", SortBy: "invalid_column");

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SortBy);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validator_ShouldHaveError_WhenPageIsInvalid(int invalidPage)
    {
        // Arrange
        var query = new SearchRecipesQuery(Q: "Món ngon", Page: invalidPage);

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
        var query = new SearchRecipesQuery(Q: "Món ngon", PageSize: invalidPageSize);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenMinCaloriesGreaterThanMaxCalories()
    {
        // Arrange
        var query = new SearchRecipesQuery(Q: "Món ngon", MinCalories: 800m, MaxCalories: 300m);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor("CaloriesRange");
    }
}
