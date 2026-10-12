namespace CulinaryBlog.UnitTests.Recipes;

using System;
using System.Threading;
using System.Threading.Tasks;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Recipes.Commands.ArchiveRecipe;
using CulinaryBlog.Application.Features.Recipes.Commands.DeleteRecipe;
using CulinaryBlog.Application.Features.Recipes.Commands.PublishRecipe;
using CulinaryBlog.Application.Features.Recipes.Commands.PurgeRecipe;
using CulinaryBlog.Application.Features.Recipes.Commands.RestoreRecipe;
using CulinaryBlog.Application.Features.Recipes.Commands.UnarchiveRecipe;
using CulinaryBlog.Application.Features.Recipes.Commands.UnpublishRecipe;
using CulinaryBlog.Application.Features.Recipes.Queries.GetTrashedRecipes;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

/// <summary>
/// Unit Tests kiểm tra Authorization, Optimistic Concurrency Token, và Rule Validation
/// cho toàn bộ luồng vòng đời Recipe (Publish, Unpublish, Archive, Unarchive, Delete, Trash, Restore, Purge)
/// của thành viên Tạ Nhật Nguyên (MSSV: 2312704).
/// </summary>
public class RecipeLifecycleAuthorizationAndConcurrencyTests
{
    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public string? UserId { get; set; }
        public bool IsAdmin { get; set; }
    }

    private CulinaryBlogDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<CulinaryBlogDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_RecipeLifecycle_{Guid.NewGuid()}")
            .Options;

        return new CulinaryBlogDbContext(options);
    }

    [Fact]
    public async Task PublishRecipe_ShouldThrowUnauthorizedException_WhenUserNotAuthenticated()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe { Id = recipeId, Title = "Bún Chả", AuthorId = "author-1" });
        await context.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { UserId = null, IsAdmin = false };
        var repo = new EfRepository<Recipe>(context);
        var uow = new UnitOfWork(context);
        var handler = new PublishRecipeCommandHandler(repo, uow, context, currentUser);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            handler.Handle(new PublishRecipeCommand(recipeId), CancellationToken.None));

        Assert.Equal("AUTH_UNAUTHORIZED", ex.ErrorCode);
    }

    [Fact]
    public async Task PublishRecipe_ShouldThrowForbiddenException_WhenUserNotOwnerAndNotAdmin()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe { Id = recipeId, Title = "Bún Chả", AuthorId = "author-1" });
        await context.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { UserId = "other-user", IsAdmin = false };
        var repo = new EfRepository<Recipe>(context);
        var uow = new UnitOfWork(context);
        var handler = new PublishRecipeCommandHandler(repo, uow, context, currentUser);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new PublishRecipeCommand(recipeId), CancellationToken.None));

        Assert.Equal("RECIPE_FORBIDDEN", ex.ErrorCode);
    }

    [Fact]
    public async Task PublishRecipe_ShouldThrowBusinessRuleException_WhenRecipeHasNoIngredientsOrSteps()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe
        {
            Id = recipeId,
            Title = "Bún Chả Chưa Đủ Thành Phần",
            Slug = "bun-cha-chua-du-thanh-phan",
            AuthorId = "author-1",
            Status = RecipeStatus.Draft
        });
        await context.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { UserId = "author-1", IsAdmin = false };
        var repo = new EfRepository<Recipe>(context);
        var uow = new UnitOfWork(context);
        var handler = new PublishRecipeCommandHandler(repo, uow, context, currentUser);

        // Act & Assert (C5: Cần ít nhất 1 nguyên liệu và 1 bước thực hiện)
        var ex = await Assert.ThrowsAsync<BusinessRuleValidationException>(() =>
            handler.Handle(new PublishRecipeCommand(recipeId), CancellationToken.None));

        Assert.Equal("RECIPE_PUBLISH_INCOMPLETE", ex.ErrorCode);
    }

    [Fact]
    public async Task PublishRecipe_ShouldSucceed_WhenOwnerAndHasIngredientsAndSteps()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe
        {
            Id = recipeId,
            Title = "Phở Gà Đầy Đủ",
            Slug = "pho-ga-day-du",
            AuthorId = "author-1",
            Status = RecipeStatus.Draft
        });
        context.RecipeIngredients.Add(new RecipeIngredient
        {
            Id = Guid.NewGuid(),
            RecipeId = recipeId,
            Name = "Thịt gà",
            Quantity = 500,
            Unit = "g"
        });
        context.RecipeSteps.Add(new RecipeStep
        {
            Id = Guid.NewGuid(),
            RecipeId = recipeId,
            StepNumber = 1,
            Title = "Luộc gà",
            Description = "Luộc gà chín tới vớt ra ngâm nước đá"
        });
        await context.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { UserId = "author-1", IsAdmin = false };
        var repo = new EfRepository<Recipe>(context);
        var uow = new UnitOfWork(context);
        var handler = new PublishRecipeCommandHandler(repo, uow, context, currentUser);

        // Act
        var result = await handler.Handle(new PublishRecipeCommand(recipeId), CancellationToken.None);

        // Assert
        Assert.Equal(RecipeStatus.Published, result.Status);
        Assert.NotNull(result.PublishedAt);

        var updatedEntity = await context.Recipes.FindAsync(recipeId);
        Assert.NotNull(updatedEntity);
        Assert.Equal(RecipeStatus.Published, updatedEntity.Status);
        Assert.NotNull(updatedEntity.PublishedAt);
    }

    [Fact]
    public async Task UnpublishRecipe_ShouldThrowUnauthorizedException_WhenUserNotAuthenticated()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe { Id = recipeId, Title = "Cơm Chiên", AuthorId = "author-1", Status = RecipeStatus.Published });
        await context.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { UserId = null, IsAdmin = false };
        var repo = new EfRepository<Recipe>(context);
        var uow = new UnitOfWork(context);
        var handler = new UnpublishRecipeCommandHandler(repo, uow, context, currentUser);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            handler.Handle(new UnpublishRecipeCommand(recipeId), CancellationToken.None));

        Assert.Equal("AUTH_UNAUTHORIZED", ex.ErrorCode);
    }

    [Fact]
    public async Task UnpublishRecipe_ShouldThrowForbiddenException_WhenUserNotOwnerAndNotAdmin()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe { Id = recipeId, Title = "Cơm Chiên", AuthorId = "author-1", Status = RecipeStatus.Published });
        await context.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { UserId = "hacker", IsAdmin = false };
        var repo = new EfRepository<Recipe>(context);
        var uow = new UnitOfWork(context);
        var handler = new UnpublishRecipeCommandHandler(repo, uow, context, currentUser);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new UnpublishRecipeCommand(recipeId), CancellationToken.None));

        Assert.Equal("RECIPE_FORBIDDEN", ex.ErrorCode);
    }

    [Fact]
    public async Task UnpublishRecipe_ShouldSucceed_WhenOwnerAndCurrentlyPublished()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe
        {
            Id = recipeId,
            Title = "Cơm Chiên Dương Châu",
            AuthorId = "author-1",
            Status = RecipeStatus.Published
        });
        await context.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { UserId = "author-1", IsAdmin = false };
        var repo = new EfRepository<Recipe>(context);
        var uow = new UnitOfWork(context);
        var handler = new UnpublishRecipeCommandHandler(repo, uow, context, currentUser);

        // Act
        var result = await handler.Handle(new UnpublishRecipeCommand(recipeId), CancellationToken.None);

        // Assert
        Assert.Equal(RecipeStatus.Draft, result.Status);

        var updatedEntity = await context.Recipes.FindAsync(recipeId);
        Assert.NotNull(updatedEntity);
        Assert.Equal(RecipeStatus.Draft, updatedEntity.Status);
    }

    [Fact]
    public async Task ArchiveRecipe_ShouldSucceed_WhenOwnerAndCurrentlyPublished()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe
        {
            Id = recipeId,
            Title = "Canh Chua",
            AuthorId = "author-1",
            Status = RecipeStatus.Published
        });
        await context.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { UserId = "author-1", IsAdmin = false };
        var repo = new EfRepository<Recipe>(context);
        var uow = new UnitOfWork(context);
        var handler = new ArchiveRecipeCommandHandler(repo, uow, context, currentUser);

        // Act
        var result = await handler.Handle(new ArchiveRecipeCommand(recipeId), CancellationToken.None);

        // Assert
        Assert.Equal(RecipeStatus.Archived, result.Status);
    }

    [Fact]
    public async Task UnarchiveRecipe_ShouldSucceed_WhenOwnerAndCurrentlyArchived()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe
        {
            Id = recipeId,
            Title = "Món Cũ Lưu Trữ",
            AuthorId = "author-1",
            Status = RecipeStatus.Archived
        });
        await context.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { UserId = "author-1", IsAdmin = false };
        var repo = new EfRepository<Recipe>(context);
        var uow = new UnitOfWork(context);
        var handler = new UnarchiveRecipeCommandHandler(repo, uow, context, currentUser);

        // Act
        var result = await handler.Handle(new UnarchiveRecipeCommand(recipeId), CancellationToken.None);

        // Assert
        Assert.Equal(RecipeStatus.Draft, result.Status);
    }

    [Fact]
    public async Task DeleteRecipe_ShouldSoftDelete_WhenAuthorizedOwner()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe
        {
            Id = recipeId,
            Title = "Món Cần Xóa",
            AuthorId = "author-1",
            Status = RecipeStatus.Draft
        });
        await context.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { UserId = "author-1", IsAdmin = false };
        var repo = new EfRepository<Recipe>(context);
        var uow = new UnitOfWork(context);
        var handler = new DeleteRecipeCommandHandler(repo, uow, context, currentUser);

        // Act
        await handler.Handle(new DeleteRecipeCommand(recipeId), CancellationToken.None);

        // Assert
        var deletedEntity = await context.Recipes.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == recipeId);
        Assert.NotNull(deletedEntity);
        Assert.True(deletedEntity.IsDeleted);
        Assert.NotNull(deletedEntity.DeletedAt);
    }

    [Fact]
    public async Task AdminTrash_ShouldThrowUnauthorizedException_WhenUserNotAuthenticated()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var currentUser = new TestCurrentUserService { UserId = null, IsAdmin = false };
        var handler = new GetTrashedRecipesQueryHandler(context, currentUser);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            handler.Handle(new GetTrashedRecipesQuery(), CancellationToken.None));

        Assert.Equal("AUTH_UNAUTHORIZED", ex.ErrorCode);
    }

    [Fact]
    public async Task AdminTrash_ShouldThrowForbiddenException_WhenUserNotAdmin()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var currentUser = new TestCurrentUserService { UserId = "regular-user", IsAdmin = false };
        var handler = new GetTrashedRecipesQueryHandler(context, currentUser);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new GetTrashedRecipesQuery(), CancellationToken.None));

        Assert.Equal("RECIPE_FORBIDDEN", ex.ErrorCode);
    }

    [Fact]
    public async Task AdminRestore_ShouldRestoreRecipe_WhenAdmin()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe
        {
            Id = recipeId,
            Title = "Món Đã Bị Xóa Mềm",
            AuthorId = "author-1",
            IsDeleted = true,
            DeletedAt = DateTimeOffset.UtcNow.AddDays(-2)
        });
        await context.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { UserId = "admin-user", IsAdmin = true };
        var repo = new EfRepository<Recipe>(context);
        var uow = new UnitOfWork(context);
        var handler = new RestoreRecipeCommandHandler(repo, uow, context, currentUser);

        // Act
        await handler.Handle(new RestoreRecipeCommand(recipeId), CancellationToken.None);

        // Assert
        var restored = await context.Recipes.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == recipeId);
        Assert.NotNull(restored);
        Assert.False(restored.IsDeleted);
        Assert.Null(restored.DeletedAt);
    }

    [Fact]
    public async Task AdminPurge_ShouldPermanentlyDeleteRecipe_WhenAdmin()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe
        {
            Id = recipeId,
            Title = "Món Chuẩn Bị Xóa Vĩnh Viễn",
            AuthorId = "author-1",
            IsDeleted = true,
            DeletedAt = DateTimeOffset.UtcNow.AddDays(-15)
        });
        await context.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { UserId = "admin-user", IsAdmin = true };
        var repo = new EfRepository<Recipe>(context);
        var uow = new UnitOfWork(context);
        var handler = new PurgeRecipeCommandHandler(repo, uow, context, currentUser);

        // Act
        await handler.Handle(new PurgeRecipeCommand(recipeId), CancellationToken.None);

        // Assert
        var purged = await context.Recipes.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == recipeId);
        Assert.Null(purged);
    }
}
