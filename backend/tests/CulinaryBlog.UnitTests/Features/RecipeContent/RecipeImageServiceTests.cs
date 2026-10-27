namespace CulinaryBlog.UnitTests.Features.RecipeContent;

using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.RecipeImages.DTOs;
using CulinaryBlog.Application.Features.RecipeImages.Services;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class RecipeImageServiceTests
{
    private class FakeFileStorageService : IFileStorageService
    {
        public Task<string> UploadAsync(Stream stream, string fileName, string contentType, string? folder = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult($"https://storage.culinaryblog.com/{folder}/{fileName}");
        }

        public Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private CulinaryBlogDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<CulinaryBlogDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_Images_{Guid.NewGuid()}")
            .Options;

        return new CulinaryBlogDbContext(options);
    }

    private static MemoryStream CreateValidJpegStream()
    {
        var ms = new MemoryStream();
        ms.Write(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 });
        ms.Position = 0;
        return ms;
    }

    [Fact]
    public async Task GetImagesAsync_ShouldOrderByPrimaryFirst_ThenOrderIndex()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe { Id = recipeId, Title = "Nem Rán", Description = "Nem rán giòn", AuthorId = "user1" });
        context.RecipeImages.AddRange(
            new RecipeImage { Id = Guid.NewGuid(), RecipeId = recipeId, OriginalUrl = "url1", IsPrimary = false, OrderIndex = 0 },
            new RecipeImage { Id = Guid.NewGuid(), RecipeId = recipeId, OriginalUrl = "url2", IsPrimary = true, OrderIndex = 1 },
            new RecipeImage { Id = Guid.NewGuid(), RecipeId = recipeId, OriginalUrl = "url3", IsPrimary = false, OrderIndex = 2 }
        );
        await context.SaveChangesAsync();

        var service = new RecipeImageService(context, new FakeFileStorageService());

        // Act
        var result = await service.GetImagesAsync(recipeId);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.True(result[0].IsPrimary);
        Assert.Equal("url2", result[0].OriginalUrl);
    }

    [Fact]
    public async Task UploadImageAsync_FirstImage_ShouldDefaultToPrimary()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe { Id = recipeId, Title = "Nem Rán", Description = "Nem rán giòn", AuthorId = "user1" });
        await context.SaveChangesAsync();

        var service = new RecipeImageService(context, new FakeFileStorageService());
        using var stream = CreateValidJpegStream();

        // Act
        var result = await service.UploadImageAsync(
            recipeId,
            stream,
            "dish.jpg",
            "image/jpeg",
            stream.Length,
            altText: "Món nem rán thơm ngon",
            isPrimary: null,
            currentUserId: "user1",
            isAdmin: false);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsPrimary);
        Assert.Equal("Món nem rán thơm ngon", result.AltText);
    }

    [Fact]
    public async Task UploadImageAsync_WhenIsPrimaryTrue_ShouldUnsetPreviousPrimary()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe { Id = recipeId, Title = "Nem Rán", Description = "Nem rán", AuthorId = "user1" });
        var oldPrimary = new RecipeImage { Id = Guid.NewGuid(), RecipeId = recipeId, OriginalUrl = "old.jpg", IsPrimary = true, OrderIndex = 0 };
        context.RecipeImages.Add(oldPrimary);
        await context.SaveChangesAsync();

        var service = new RecipeImageService(context, new FakeFileStorageService());
        using var stream = CreateValidJpegStream();

        // Act
        var result = await service.UploadImageAsync(
            recipeId,
            stream,
            "new.jpg",
            "image/jpeg",
            stream.Length,
            altText: "Ảnh mới chính",
            isPrimary: true,
            currentUserId: "user1",
            isAdmin: false);

        // Assert
        Assert.True(result.IsPrimary);

        var updatedOldPrimary = await context.RecipeImages.FirstOrDefaultAsync(i => i.Id == oldPrimary.Id);
        Assert.NotNull(updatedOldPrimary);
        Assert.False(updatedOldPrimary.IsPrimary); // Đã bị unset primary
    }

    [Fact]
    public async Task UpdateImageAsync_WhenSettingPrimary_ShouldUnsetOthers()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe { Id = recipeId, Title = "Phở", Description = "Phở", AuthorId = "user1" });
        var img1 = new RecipeImage { Id = Guid.NewGuid(), RecipeId = recipeId, OriginalUrl = "1.jpg", IsPrimary = true, OrderIndex = 0 };
        var img2 = new RecipeImage { Id = Guid.NewGuid(), RecipeId = recipeId, OriginalUrl = "2.jpg", IsPrimary = false, OrderIndex = 1 };
        context.RecipeImages.AddRange(img1, img2);
        await context.SaveChangesAsync();

        var service = new RecipeImageService(context, new FakeFileStorageService());

        // Act: Đặt img2 thành primary
        var request = new UpdateRecipeImageRequest { IsPrimary = true };
        var result = await service.UpdateImageAsync(recipeId, img2.Id, request, currentUserId: "user1", isAdmin: false);

        // Assert
        Assert.True(result.IsPrimary);

        var refreshedImg1 = await context.RecipeImages.FirstOrDefaultAsync(i => i.Id == img1.Id);
        Assert.NotNull(refreshedImg1);
        Assert.False(refreshedImg1.IsPrimary);
    }

    [Fact]
    public async Task DeleteImageAsync_WhenDeletingPrimary_ShouldElectMinOrderIndexAsNewPrimary()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var recipeId = Guid.NewGuid();
        context.Recipes.Add(new Recipe { Id = recipeId, Title = "Bún Chả", Description = "Bún chả Hà Nội", AuthorId = "user1" });
        var primaryImg = new RecipeImage { Id = Guid.NewGuid(), RecipeId = recipeId, OriginalUrl = "primary.jpg", IsPrimary = true, OrderIndex = 0 };
        var secondaryImg1 = new RecipeImage { Id = Guid.NewGuid(), RecipeId = recipeId, OriginalUrl = "sec1.jpg", IsPrimary = false, OrderIndex = 2 };
        var secondaryImg2 = new RecipeImage { Id = Guid.NewGuid(), RecipeId = recipeId, OriginalUrl = "sec2.jpg", IsPrimary = false, OrderIndex = 1 };
        context.RecipeImages.AddRange(primaryImg, secondaryImg1, secondaryImg2);
        await context.SaveChangesAsync();

        var service = new RecipeImageService(context, new FakeFileStorageService());

        // Act: Xóa ảnh primary
        await service.DeleteImageAsync(recipeId, primaryImg.Id, currentUserId: "user1", isAdmin: false);

        // Assert: secondaryImg2 có OrderIndex = 1 nhỏ hơn secondaryImg1 (OrderIndex = 2) nên được bầu làm primary mới
        var refreshedSec2 = await context.RecipeImages.FirstOrDefaultAsync(i => i.Id == secondaryImg2.Id);
        Assert.NotNull(refreshedSec2);
        Assert.True(refreshedSec2.IsPrimary);

        var refreshedSec1 = await context.RecipeImages.FirstOrDefaultAsync(i => i.Id == secondaryImg1.Id);
        Assert.NotNull(refreshedSec1);
        Assert.False(refreshedSec1.IsPrimary);
    }
}
