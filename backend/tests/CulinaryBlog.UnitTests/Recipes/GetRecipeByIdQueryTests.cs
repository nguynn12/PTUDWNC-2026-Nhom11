namespace CulinaryBlog.UnitTests.Recipes;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes.Queries.GetRecipeById;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class GetRecipeByIdQueryTests
{
    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public string? UserId { get; set; }
        public bool IsAdmin { get; set; }
    }

    private sealed class TestUserQueryService : IUserQueryService
    {
        public Task<IReadOnlyDictionary<string, AuthorSummary>> GetAuthorSummariesAsync(
            IEnumerable<string> authorIds,
            CancellationToken cancellationToken = default)
        {
            var dict = new Dictionary<string, AuthorSummary>();
            foreach (var id in authorIds)
            {
                dict[id] = new AuthorSummary(id, $"Author_{id}", null);
            }
            return Task.FromResult<IReadOnlyDictionary<string, AuthorSummary>>(dict);
        }
    }

    private CulinaryBlogDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<CulinaryBlogDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_GetRecipeById_{Guid.NewGuid()}")
            .Options;

        return new CulinaryBlogDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedException_WhenUserNotAuthenticated()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var currentUserService = new TestCurrentUserService { UserId = null, IsAdmin = false };
        var handler = new GetRecipeByIdQueryHandler(
            context,
            new TestUserQueryService(),
            currentUserService,
            NullLogger<GetRecipeByIdQueryHandler>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            handler.Handle(new GetRecipeByIdQuery(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenRecipeDoesNotExist()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var currentUserService = new TestCurrentUserService { UserId = "author-1", IsAdmin = false };
        var handler = new GetRecipeByIdQueryHandler(
            context,
            new TestUserQueryService(),
            currentUserService,
            NullLogger<GetRecipeByIdQueryHandler>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetRecipeByIdQuery(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenRecipeIsSoftDeleted()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var categoryId = Guid.NewGuid();
        context.Categories.Add(new Category { Id = categoryId, Name = "Cat", Slug = "cat" });

        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe
        {
            Id = recipeId,
            Title = "Công thức đã xóa",
            Slug = "cong-thuc-da-xoa",
            Description = "Mô tả công thức",
            AuthorId = "author-1",
            Status = RecipeStatus.Draft,
            IsDeleted = true,
            CategoryId = categoryId
        });
        await context.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = "author-1", IsAdmin = false };
        var handler = new GetRecipeByIdQueryHandler(
            context,
            new TestUserQueryService(),
            currentUserService,
            NullLogger<GetRecipeByIdQueryHandler>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetRecipeByIdQuery(recipeId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenUserIsNotAuthorAndNotAdmin()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var categoryId = Guid.NewGuid();
        context.Categories.Add(new Category { Id = categoryId, Name = "Cat", Slug = "cat" });

        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe
        {
            Id = recipeId,
            Title = "Công thức của người khác",
            Slug = "cong-thuc-cua-nguoi-khac",
            Description = "Mô tả công thức",
            AuthorId = "author-1",
            Status = RecipeStatus.Draft,
            IsDeleted = false,
            CategoryId = categoryId
        });
        await context.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = "author-stranger", IsAdmin = false };
        var handler = new GetRecipeByIdQueryHandler(
            context,
            new TestUserQueryService(),
            currentUserService,
            NullLogger<GetRecipeByIdQueryHandler>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new GetRecipeByIdQuery(recipeId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldReturnRecipeDetail_WhenUserIsAuthorEvenIfDraft()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        context.Categories.Add(new Category
        {
            Id = categoryId,
            Name = "Món chính",
            Slug = "mon-chinh",
            Description = "Danh mục món chính"
        });

        context.Recipes.Add(new Recipe
        {
            Id = recipeId,
            Title = "Bún Bò Huế Cay Nồng",
            Slug = "bun-bo-hue-cay-nong",
            Description = "Công thức nấu bún bò chuẩn vị Huế",
            PrepTimeMinutes = 30,
            CookTimeMinutes = 60,
            Servings = 4,
            Difficulty = RecipeDifficulty.Hard,
            AuthorId = "author-1",
            Status = RecipeStatus.Draft, // Bản nháp
            IsDeleted = false,
            CategoryId = categoryId
        });
        await context.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = "author-1", IsAdmin = false };
        var handler = new GetRecipeByIdQueryHandler(
            context,
            new TestUserQueryService(),
            currentUserService,
            NullLogger<GetRecipeByIdQueryHandler>.Instance);

        // Act
        var result = await handler.Handle(new GetRecipeByIdQuery(recipeId), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(recipeId, result.Id);
        Assert.Equal("Bún Bò Huế Cay Nồng", result.Title);
        Assert.Equal("Draft", result.Status);
        Assert.Equal("Hard", result.Difficulty);
    }

    [Fact]
    public async Task Handle_ShouldReturnRecipeDetail_WhenUserIsAdminEvenIfDraft()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var categoryId = Guid.NewGuid();
        context.Categories.Add(new Category { Id = categoryId, Name = "Cat Admin", Slug = "cat-admin" });

        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe
        {
            Id = recipeId,
            Title = "Công thức nháp của Author",
            Slug = "cong-thuc-nhap-cua-author",
            Description = "Mô tả",
            AuthorId = "author-1",
            Status = RecipeStatus.Draft,
            IsDeleted = false,
            CategoryId = categoryId
        });
        await context.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = "admin-user", IsAdmin = true };
        var handler = new GetRecipeByIdQueryHandler(
            context,
            new TestUserQueryService(),
            currentUserService,
            NullLogger<GetRecipeByIdQueryHandler>.Instance);

        // Act
        var result = await handler.Handle(new GetRecipeByIdQuery(recipeId), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(recipeId, result.Id);
        Assert.Equal("Draft", result.Status);
    }
}
