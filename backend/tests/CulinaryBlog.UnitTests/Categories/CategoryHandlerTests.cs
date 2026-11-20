using CulinaryBlog.Application.Common.Constants;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Categories.Commands.CreateCategory;
using CulinaryBlog.Application.Features.Categories.Commands.DeleteCategory;
using CulinaryBlog.Application.Features.Categories.Commands.UpdateCategory;
using CulinaryBlog.Application.Features.Categories.DTOs;
using CulinaryBlog.Application.Features.Categories.Queries.GetCategories;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Services.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static CulinaryBlog.UnitTests.Support.HandlerTestSupport;

namespace CulinaryBlog.UnitTests.Categories;

public sealed class CategoryHandlerTests
{
    [Fact]
    public async Task Create_ResolvesSlugCollision_AndInvalidatesCachedList()
    {
        await using var db = CreateContext();
        db.Categories.Add(new Category { Name = "Danh mục đang dùng", Slug = "mon-nuoc" });
        await db.SaveChangesAsync();
        var cache = new Cache();
        await cache.SetAsync(CacheConstants.CategoryAllKey, new List<CategoryDto>());
        var handler = new CreateCategoryCommandHandler(db, NullLogger<CreateCategoryCommandHandler>.Instance, Invalidator(cache));

        var result = await handler.Handle(
            new CreateCategoryCommand("  Món nước  ", "  Các món có nước dùng  ", "  https://img.test/mon-nuoc.jpg  ", 3),
            CancellationToken.None);

        var saved = await db.Categories.AsNoTracking().SingleAsync(category => category.Id == result.Id);
        Assert.Equal("Món nước", saved.Name);
        Assert.Equal("mon-nuoc-2", saved.Slug);
        Assert.Equal("Các món có nước dùng", result.Description);
        Assert.Equal("https://img.test/mon-nuoc.jpg", result.ImageUrl);
        Assert.Equal(3, result.OrderIndex);
        Assert.Equal(0, result.RecipeCount);
        Assert.Null(await cache.GetAsync<List<CategoryDto>>(CacheConstants.CategoryAllKey));
    }

    [Fact]
    public async Task Create_RejectsCaseInsensitiveDuplicateName()
    {
        await using var db = CreateContext();
        db.Categories.Add(new Category { Name = "Salad", Slug = "salad" });
        await db.SaveChangesAsync();
        var handler = new CreateCategoryCommandHandler(db, NullLogger<CreateCategoryCommandHandler>.Instance);

        var error = await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CreateCategoryCommand("  SALAD  "), CancellationToken.None));

        Assert.Equal("CATEGORY_NAME_EXISTS", error.ErrorCode);
        Assert.Single(await db.Categories.ToListAsync());
    }

    [Fact]
    public async Task Create_FallsBackToCategorySlug_WhenNameHasNoSlugCharacters()
    {
        await using var db = CreateContext();
        var handler = new CreateCategoryCommandHandler(db, NullLogger<CreateCategoryCommandHandler>.Instance);

        var result = await handler.Handle(new CreateCategoryCommand("🔥 Món!"), CancellationToken.None);
        var fallback = await handler.Handle(new CreateCategoryCommand("🍲"), CancellationToken.None);

        Assert.Equal("mon", result.Slug);
        Assert.Equal("danh-muc", fallback.Slug);
        Assert.Null(fallback.Description);
        Assert.Null(fallback.ImageUrl);
    }

    [Fact]
    public async Task Update_KeepsExistingSlug_AndCountsOnlyPublishedNonDeletedRecipes()
    {
        await using var db = CreateContext();
        var category = new Category { Name = "Món nước", Slug = "mon-nuoc", Description = "Mô tả cũ" };
        db.AddRange(category, RecipeIn(category, RecipeStatus.Published), RecipeIn(category, RecipeStatus.Draft),
            RecipeIn(category, RecipeStatus.Published, true));
        await db.SaveChangesAsync();
        var cache = new Cache();
        var detailKey = CacheConstants.CategoryPrefix + category.Slug;
        await cache.SetAsync(detailKey, category.Name);
        var handler = new UpdateCategoryCommandHandler(db, NullLogger<UpdateCategoryCommandHandler>.Instance, Invalidator(cache));

        var result = await handler.Handle(
            new UpdateCategoryCommand(category.Id, "  Món hầm  ", "  ", " https://img.test/stew.jpg ", 6),
            CancellationToken.None);

        var saved = await db.Categories.AsNoTracking().SingleAsync();
        Assert.Equal("Món hầm", saved.Name);
        Assert.Equal("mon-nuoc", result.Slug);
        Assert.Null(saved.Description);
        Assert.Equal("https://img.test/stew.jpg", saved.ImageUrl);
        Assert.Equal(6, saved.OrderIndex);
        Assert.NotNull(saved.UpdatedAt);
        Assert.Equal(1, result.RecipeCount);
        Assert.Null(await cache.GetAsync<string>(detailKey));
    }

    [Fact]
    public async Task Update_RejectsMissingCategoryAndNameOwnedByAnotherCategory()
    {
        await using var db = CreateContext();
        var category = new Category { Name = "Món nước", Slug = "mon-nuoc" };
        db.Categories.AddRange(category, new Category { Name = "Salad", Slug = "salad" });
        await db.SaveChangesAsync();
        var handler = new UpdateCategoryCommandHandler(db, NullLogger<UpdateCategoryCommandHandler>.Instance);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new UpdateCategoryCommand(Guid.NewGuid(), "Món hầm"), CancellationToken.None));
        var duplicate = await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new UpdateCategoryCommand(category.Id, "salad"), CancellationToken.None));

        Assert.Equal("CATEGORY_NAME_EXISTS", duplicate.ErrorCode);
        Assert.Equal("Món nước", (await db.Categories.AsNoTracking().SingleAsync(item => item.Id == category.Id)).Name);
    }

    [Theory]
    [InlineData(RecipeStatus.Draft)]
    [InlineData(RecipeStatus.Published)]
    [InlineData(RecipeStatus.Archived)]
    public async Task Delete_RejectsCategoryWithAnyNonDeletedRecipe(RecipeStatus status)
    {
        await using var db = CreateContext();
        var category = new Category { Name = "Món nước", Slug = "mon-nuoc" };
        db.AddRange(category, RecipeIn(category, status));
        await db.SaveChangesAsync();
        var handler = new DeleteCategoryCommandHandler(db, NullLogger<DeleteCategoryCommandHandler>.Instance);

        var error = await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None));

        Assert.Equal("CATEGORY_DELETE_HAS_RECIPES", error.ErrorCode);
        Assert.False((await db.Categories.AsNoTracking().SingleAsync()).IsDeleted);
    }

    [Fact]
    public async Task Delete_SoftDeletesWhenOnlyRecipeWasDeleted_AndInvalidatesCategory()
    {
        await using var db = CreateContext();
        var category = new Category { Name = "Món nước", Slug = "mon-nuoc" };
        var recipe = RecipeIn(category, RecipeStatus.Published, true);
        db.AddRange(category, recipe);
        await db.SaveChangesAsync();
        var cache = new Cache();
        await cache.SetAsync(CacheConstants.CategoryAllKey, new List<CategoryDto>());
        var handler = new DeleteCategoryCommandHandler(db, NullLogger<DeleteCategoryCommandHandler>.Instance, Invalidator(cache));

        Assert.True(await handler.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None));

        Assert.Empty(await db.Categories.ToListAsync());
        var deleted = await db.Categories.IgnoreQueryFilters().AsNoTracking().SingleAsync();
        Assert.True(deleted.IsDeleted);
        Assert.NotNull(deleted.DeletedAt);
        Assert.True(await db.Recipes.IgnoreQueryFilters().AnyAsync(item => item.Id == recipe.Id));
        Assert.Null(await cache.GetAsync<List<CategoryDto>>(CacheConstants.CategoryAllKey));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None));
    }

    [Fact]
    public async Task List_UsesDisplayOrderAndCachedPublishedCounts()
    {
        await using var db = CreateContext();
        var soup = new Category { Name = "Cháo", Slug = "chao", OrderIndex = 1 };
        var rice = new Category { Name = "Cơm", Slug = "com", OrderIndex = 1 };
        var dessert = new Category { Name = "Bánh", Slug = "banh", OrderIndex = 2 };
        db.AddRange(soup, rice, dessert, new Category { Name = "Ẩn", Slug = "an", IsDeleted = true },
            RecipeIn(soup, RecipeStatus.Published), RecipeIn(soup, RecipeStatus.Published, true),
            RecipeIn(soup, RecipeStatus.Draft), RecipeIn(soup, RecipeStatus.Archived));
        await db.SaveChangesAsync();
        var cache = new Cache();
        var handler = new GetCategoriesQueryHandler(db, NullLogger<GetCategoriesQueryHandler>.Instance, cache);

        var result = await handler.Handle(new GetCategoriesQuery(), CancellationToken.None);
        Assert.Equal(new[] { "chao", "com", "banh" }, result.Select(item => item.Slug));
        Assert.Equal(new[] { 1, 0, 0 }, result.Select(item => item.RecipeCount));
        Assert.Equal((CacheConstants.CategoryAllKey, CacheConstants.CategoryDuration), Assert.Single(cache.Writes));

        db.Recipes.Add(RecipeIn(soup, RecipeStatus.Published));
        await db.SaveChangesAsync();
        Assert.Equal(1, (await handler.Handle(new GetCategoriesQuery(), CancellationToken.None))[0].RecipeCount);
        await cache.RemoveAsync(CacheConstants.CategoryAllKey);
        Assert.Equal(2, (await handler.Handle(new GetCategoriesQuery(), CancellationToken.None))[0].RecipeCount);
    }

    [Fact]
    public async Task List_WithoutCache_ReturnsEmptyListForAnEmptyDatabase()
    {
        await using var db = CreateContext();
        var handler = new GetCategoriesQueryHandler(db, NullLogger<GetCategoriesQueryHandler>.Instance);

        Assert.Empty(await handler.Handle(new GetCategoriesQuery(), CancellationToken.None));
    }

    private static CacheInvalidator Invalidator(Cache cache) => new(cache, NullLogger<CacheInvalidator>.Instance);

    private static Recipe RecipeIn(Category category, RecipeStatus status, bool isDeleted = false) =>
        new() { CategoryId = category.Id, Status = status, IsDeleted = isDeleted, Slug = Guid.NewGuid().ToString("N") };
}
