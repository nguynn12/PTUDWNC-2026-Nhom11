namespace CulinaryBlog.UnitTests.Features.RecipeContent;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.RecipeSteps.DTOs;
using CulinaryBlog.Application.Features.RecipeSteps.Services;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class RecipeStepServiceTests
{
    private CulinaryBlogDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<CulinaryBlogDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_Steps_{Guid.NewGuid()}")
            .Options;

        return new CulinaryBlogDbContext(options);
    }

    [Fact]
    public async Task GetStepsAsync_WhenRecipeExists_ShouldReturnOrderedSteps()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe { Id = recipeId, Title = "Bánh Mì", Description = "Bánh mì pate", AuthorId = "user1", Status = RecipeStatus.Published });
        context.RecipeSteps.AddRange(
            new RecipeStep { Id = Guid.NewGuid(), RecipeId = recipeId, StepNumber = 3, Description = "Nướng bánh mì" },
            new RecipeStep { Id = Guid.NewGuid(), RecipeId = recipeId, StepNumber = 1, Description = "Nhào bột" },
            new RecipeStep { Id = Guid.NewGuid(), RecipeId = recipeId, StepNumber = 2, Description = "Ủ bột" }
        );
        await context.SaveChangesAsync();

        var service = new RecipeStepService(context);

        // Act
        var result = await service.GetStepsAsync(recipeId);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(1, result[0].StepNumber);
        Assert.Equal("Nhào bột", result[0].Description);
        Assert.Equal(2, result[1].StepNumber);
        Assert.Equal(3, result[2].StepNumber);
    }

    [Fact]
    public async Task GetStepsAsync_WhenDraftAndNotOwner_ShouldThrowForbiddenException()
    {
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe { Id = recipeId, Title = "Bánh Cuốn", Description = "Bánh cuốn nóng", AuthorId = "user1", Status = RecipeStatus.Draft });
        await context.SaveChangesAsync();

        var service = new RecipeStepService(context);

        await Assert.ThrowsAsync<ForbiddenException>(() => service.GetStepsAsync(recipeId, currentUserId: "user2", isAdmin: false));
    }

    [Fact]
    public async Task AddStepAsync_WhenClientProvidesStepNumber_ShouldIgnoreAndAssignServerMaxPlusOne()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe { Id = recipeId, Title = "Gà Rang Muối", Description = "Món gà", AuthorId = "user1" });
        var step1 = new RecipeStep { Id = Guid.NewGuid(), RecipeId = recipeId, StepNumber = 1, Description = "Sơ chế gà" };
        var step2 = new RecipeStep { Id = Guid.NewGuid(), RecipeId = recipeId, StepNumber = 2, Description = "Rang gà chín" };
        context.RecipeSteps.AddRange(step1, step2);
        await context.SaveChangesAsync();

        var service = new RecipeStepService(context);

        // Client cố tình gửi StepNumber = 2
        var request = new CreateRecipeStepRequest
        {
            StepNumber = 2,
            Title = "Ướp gia vị",
            Description = "Ướp gà với sả ớt"
        };

        // Act
        var result = await service.AddStepAsync(recipeId, request, currentUserId: "user1", isAdmin: false);

        // Assert - API-17: Server tự sinh liên tục max + 1 = 3, bỏ qua số client
        Assert.Equal(3, result.StepNumber);
        Assert.Equal("Ướp gia vị", result.Title);
    }
    [Fact]
    public async Task AddStepAsync_WhenNoStepNumber_ShouldAssignMaxPlusOne()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe { Id = recipeId, Title = "Bánh Mì", Description = "Bánh mì", AuthorId = "user1" });
        context.RecipeSteps.Add(new RecipeStep { Id = Guid.NewGuid(), RecipeId = recipeId, StepNumber = 1, Description = "Bước 1" });
        await context.SaveChangesAsync();

        var service = new RecipeStepService(context);
        var request = new CreateRecipeStepRequest
        {
            Title = "Bước 2",
            Description = "Nội dung bước 2",
            DurationMinutes = 15
        };

        // Act
        var result = await service.AddStepAsync(recipeId, request, currentUserId: "user1", isAdmin: false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.StepNumber);
        Assert.Equal("Bước 2", result.Title);
        Assert.Equal(15, result.DurationMinutes);
    }


    [Fact]
    public async Task ReorderStepsAsync_WhenValid_ShouldRenumber1ToN()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe { Id = recipeId, Title = "Bò Kho", Description = "Bò kho bánh mì", AuthorId = "user1" });
        var s1 = new RecipeStep { Id = Guid.NewGuid(), RecipeId = recipeId, StepNumber = 1, Description = "Bước A" };
        var s2 = new RecipeStep { Id = Guid.NewGuid(), RecipeId = recipeId, StepNumber = 2, Description = "Bước B" };
        var s3 = new RecipeStep { Id = Guid.NewGuid(), RecipeId = recipeId, StepNumber = 3, Description = "Bước C" };
        context.RecipeSteps.AddRange(s1, s2, s3);
        await context.SaveChangesAsync();

        var service = new RecipeStepService(context);

        // Đảo ngược thứ tự: s3, s1, s2
        var request = new ReorderRecipeStepsRequest
        {
            StepIds = new List<Guid> { s3.Id, s1.Id, s2.Id }
        };

        // Act
        var result = await service.ReorderStepsAsync(recipeId, request, currentUserId: "user1", isAdmin: false);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(s3.Id, result[0].Id);
        Assert.Equal(1, result[0].StepNumber);

        Assert.Equal(s1.Id, result[1].Id);
        Assert.Equal(2, result[1].StepNumber);

        Assert.Equal(s2.Id, result[2].Id);
        Assert.Equal(3, result[2].StepNumber);
    }

    [Fact]
    public async Task DeleteStepAsync_WhenDeleted_ShouldRenumberRemainingSteps1ToN()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe { Id = recipeId, Title = "Lẩu Thái", Description = "Lẩu Thái hải sản", AuthorId = "user1" });
        var s1 = new RecipeStep { Id = Guid.NewGuid(), RecipeId = recipeId, StepNumber = 1, Description = "Nấu nước dùng" };
        var s2 = new RecipeStep { Id = Guid.NewGuid(), RecipeId = recipeId, StepNumber = 2, Description = "Thả hải sản" };
        var s3 = new RecipeStep { Id = Guid.NewGuid(), RecipeId = recipeId, StepNumber = 3, Description = "Thêm rau thơm" };
        context.RecipeSteps.AddRange(s1, s2, s3);
        await context.SaveChangesAsync();

        var service = new RecipeStepService(context);

        // Xóa bước 2
        // Act
        await service.DeleteStepAsync(recipeId, s2.Id, currentUserId: "user1", isAdmin: false);

        // Assert
        var activeSteps = await context.RecipeSteps
            .Where(s => s.RecipeId == recipeId)
            .OrderBy(s => s.StepNumber)
            .ToListAsync();

        Assert.Equal(2, activeSteps.Count);
        Assert.Equal(s1.Id, activeSteps[0].Id);
        Assert.Equal(1, activeSteps[0].StepNumber);

        Assert.Equal(s3.Id, activeSteps[1].Id);
        Assert.Equal(2, activeSteps[1].StepNumber); // Đã tự động renumber từ 3 thành 2
    }

    [Fact]
    public async Task AddStepAsync_WhenConcurrencyTokenMismatch_ShouldThrowConflictException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        var recipe = new Recipe { Id = recipeId, Title = "Bún Bò", Description = "Bún bò Huế", AuthorId = "user1" };
        context.Recipes.Add(recipe);
        await context.SaveChangesAsync();

        context.Entry(recipe).Property<uint>("xmin").CurrentValue = 100u;
        await context.SaveChangesAsync();

        var service = new RecipeStepService(context);
        var request = new CreateRecipeStepRequest
        {
            Title = "Bước 1",
            Description = "Nấu nước dùng"
        };

        // Act & Assert - API-03: Mismatch concurrency token (client sends 99 != db 100)
        await Assert.ThrowsAsync<ConflictException>(() =>
            service.AddStepAsync(recipeId, request, currentUserId: "user1", isAdmin: false, concurrencyToken: 99u));
    }
}
