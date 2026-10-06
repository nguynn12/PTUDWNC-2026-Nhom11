namespace CulinaryBlog.UnitTests.Features.RecipeContent;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.RecipeIngredients.DTOs;
using CulinaryBlog.Application.Features.RecipeIngredients.Services;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class RecipeIngredientServiceTests
{
    private CulinaryBlogDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<CulinaryBlogDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_Ingredients_{Guid.NewGuid()}")
            .Options;

        return new CulinaryBlogDbContext(options);
    }

    [Fact]
    public async Task GetIngredientsAsync_WhenRecipeExists_ShouldReturnOrderedIngredients()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe { Id = recipeId, Title = "Phở Bò", Description = "Món phở truyền thống", AuthorId = "user1" });
        context.RecipeIngredients.AddRange(
            new RecipeIngredient { Id = Guid.NewGuid(), RecipeId = recipeId, Name = "Hành tây", OrderIndex = 2 },
            new RecipeIngredient { Id = Guid.NewGuid(), RecipeId = recipeId, Name = "Bánh phở", OrderIndex = 0 },
            new RecipeIngredient { Id = Guid.NewGuid(), RecipeId = recipeId, Name = "Thịt bò", OrderIndex = 1 }
        );
        await context.SaveChangesAsync();

        var service = new RecipeIngredientService(context);

        // Act
        var result = await service.GetIngredientsAsync(recipeId);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Bánh phở", result[0].Name);
        Assert.Equal("Thịt bò", result[1].Name);
        Assert.Equal("Hành tây", result[2].Name);
    }

    [Fact]
    public async Task GetIngredientByIdAsync_WhenFound_ShouldReturnDto()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();
        context.Recipes.Add(new Recipe { Id = recipeId, Title = "Cơm Tấm", Description = "Món cơm tấm", AuthorId = "user1" });
        context.RecipeIngredients.Add(new RecipeIngredient
        {
            Id = ingredientId,
            RecipeId = recipeId,
            Name = "Sườn nướng",
            Quantity = 200,
            Unit = "g",
            OrderIndex = 0
        });
        await context.SaveChangesAsync();

        var service = new RecipeIngredientService(context);

        // Act
        var result = await service.GetIngredientByIdAsync(recipeId, ingredientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ingredientId, result.Id);
        Assert.Equal("Sườn nướng", result.Name);
        Assert.Equal(200m, result.Quantity);
    }

    [Fact]
    public async Task AddIngredientAsync_WhenValidAndOwner_ShouldAddSuccessfully()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe { Id = recipeId, Title = "Bún Bò", Description = "Bún bò Huế", AuthorId = "author123" });
        await context.SaveChangesAsync();

        var service = new RecipeIngredientService(context);
        var request = new CreateRecipeIngredientRequest
        {
            Name = "Bắp bò hoa",
            Quantity = 300,
            Unit = "g",
            Notes = "Thái lát mỏng"
        };

        // Act
        var result = await service.AddIngredientAsync(recipeId, request, currentUserId: "author123", isAdmin: false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Bắp bò hoa", result.Name);
        Assert.Equal(300m, result.Quantity);
        Assert.Equal(0, result.OrderIndex);

        var saved = await context.RecipeIngredients.FirstOrDefaultAsync(i => i.Id == result.Id);
        Assert.NotNull(saved);
        Assert.Equal("Bắp bò hoa", saved.Name);
    }

    [Fact]
    public async Task AddIngredientAsync_WhenNotOwnerAndNotAdmin_ShouldThrowForbidden()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe { Id = recipeId, Title = "Bún Bò", Description = "Bún bò Huế", AuthorId = "owner_id" });
        await context.SaveChangesAsync();

        var service = new RecipeIngredientService(context);
        var request = new CreateRecipeIngredientRequest { Name = "Gia vị", Quantity = 10, Unit = "g" };

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.AddIngredientAsync(recipeId, request, currentUserId: "hacker_id", isAdmin: false));
    }

    [Fact]
    public async Task DeleteIngredientAsync_ShouldSoftDelete()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();
        context.Recipes.Add(new Recipe { Id = recipeId, Title = "Canh Chua", Description = "Canh chua cá lóc", AuthorId = "user1" });
        context.RecipeIngredients.Add(new RecipeIngredient
        {
            Id = ingredientId,
            RecipeId = recipeId,
            Name = "Cá lóc",
            OrderIndex = 0
        });
        await context.SaveChangesAsync();

        var service = new RecipeIngredientService(context);

        // Act
        await service.DeleteIngredientAsync(recipeId, ingredientId, currentUserId: "user1", isAdmin: false);

        // Assert
        var item = await context.RecipeIngredients.IgnoreQueryFilters().FirstOrDefaultAsync(i => i.Id == ingredientId);
        Assert.NotNull(item);
        Assert.True(item.IsDeleted);
        Assert.NotNull(item.DeletedAt);
    }
}
