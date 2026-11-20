using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Recipes.Queries.GetTrashedRecipes;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using Xunit;
using static CulinaryBlog.UnitTests.Support.HandlerTestSupport;

namespace CulinaryBlog.UnitTests.Recipes;

public sealed class RecipeTrashQueryHandlerTests
{
    [Fact]
    public async Task AdminTrash_PaginatesOnlySoftDeletedRecipesByNewestDeletion()
    {
        await using var db = CreateContext();
        var now = DateTimeOffset.UtcNow;
        var newest = new Recipe
        {
            Title = "Cháo gà", Slug = "chao-ga", Status = RecipeStatus.Published,
            AuthorId = "author", IsDeleted = true, DeletedAt = now.AddDays(-5).AddHours(-2)
        };
        var expired = new Recipe
        {
            Title = "Cháo cá", Slug = "chao-ca", IsDeleted = true, DeletedAt = now.AddDays(-40)
        };
        db.Recipes.AddRange(expired, newest, new Recipe { Title = "Đã khôi phục", Slug = "chao-phuc-hoi" });
        await db.SaveChangesAsync();

        var handler = new GetTrashedRecipesQueryHandler(db, new User("admin", true));
        var firstPage = await handler.Handle(new GetTrashedRecipesQuery(1, 1), CancellationToken.None);
        var secondPage = await handler.Handle(new GetTrashedRecipesQuery(2, 1), CancellationToken.None);
        var defaultPage = await handler.Handle(new GetTrashedRecipesQuery(0, 0), CancellationToken.None);

        Assert.Equal(2, firstPage.TotalCount);
        var first = Assert.Single(firstPage.Items);
        Assert.Equal(newest.Id, first.Id);
        Assert.Equal(newest.Status, first.Status);
        Assert.Equal(newest.AuthorId, first.AuthorId);
        Assert.Equal(25, first.DaysRemaining);
        var last = Assert.Single(secondPage.Items);
        Assert.Equal(expired.Id, last.Id);
        Assert.Equal(0, last.DaysRemaining);
        Assert.Equal(1, defaultPage.PageNumber);
        Assert.Equal(10, defaultPage.PageSize);
        Assert.Equal(new[] { newest.Id, expired.Id }, defaultPage.Items.Select(item => item.Id));
    }

    [Fact]
    public async Task Trash_RejectsAnonymousAndOrdinaryAuthors()
    {
        await using var db = CreateContext();
        var query = new GetTrashedRecipesQuery();
        var unauthorized = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            new GetTrashedRecipesQueryHandler(db, new User(null)).Handle(query, CancellationToken.None));
        var forbidden = await Assert.ThrowsAsync<ForbiddenException>(() =>
            new GetTrashedRecipesQueryHandler(db, new User("author")).Handle(query, CancellationToken.None));

        Assert.Equal("AUTH_UNAUTHORIZED", unauthorized.ErrorCode);
        Assert.Equal("RECIPE_FORBIDDEN", forbidden.ErrorCode);
    }
}
