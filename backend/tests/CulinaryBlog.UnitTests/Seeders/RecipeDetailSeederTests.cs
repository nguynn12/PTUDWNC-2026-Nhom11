namespace CulinaryBlog.UnitTests.Seeders;

using System;
using System.Linq;
using CulinaryBlog.Infrastructure.Seeders;
using Xunit;

/// <summary>
/// Unit Tests kiểm tra các yêu cầu nghiệp vụ của Thành viên 4 trong Lab 2:
/// - Mỗi Recipe có ít nhất 10 nguyên liệu.
/// - Mỗi Recipe có ít nhất 5 bước chế biến.
/// - Thứ tự của nguyên liệu và các bước trong từng Recipe được sắp xếp chính xác.
/// - Hình ảnh minh họa có đúng 1 ảnh chính và các ảnh phụ.
/// </summary>
public class RecipeDetailSeederTests
{
    [Fact]
    public void GenerateIngredients_ShouldProduceAtLeast10Ingredients_WithAscendingOrderIndex()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var random = new Random(12345);

        // Act
        var ingredients = RecipeDetailSeeder.GenerateIngredients(recipeId, random);

        // Assert: Yêu cầu ít nhất 10 nguyên liệu
        Assert.NotNull(ingredients);
        Assert.True(ingredients.Count >= 10, $"Số lượng nguyên liệu phải >= 10 nhưng nhận được {ingredients.Count}");

        // Assert: Thuộc tính cơ bản không rỗng
        Assert.All(ingredients, i =>
        {
            Assert.Equal(recipeId, i.RecipeId);
            Assert.False(string.IsNullOrWhiteSpace(i.Name));
            Assert.True(i.OrderIndex > 0);
        });

        // Assert: Kiểm tra thứ tự OrderIndex tăng dần liên tục (1, 2, 3...)
        for (int i = 0; i < ingredients.Count; i++)
        {
            Assert.Equal(i + 1, ingredients[i].OrderIndex);
        }
    }

    [Fact]
    public void GenerateSteps_ShouldProduceAtLeast5Steps_WithAscendingStepNumbersFrom1()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var recipeTitle = "Phở Bò Tái Lăn Hà Nội";

        // Act
        var steps = RecipeDetailSeeder.GenerateSteps(recipeId, recipeTitle);

        // Assert: Yêu cầu ít nhất 5 bước chế biến
        Assert.NotNull(steps);
        Assert.True(steps.Count >= 5, $"Số lượng bước phải >= 5 nhưng nhận được {steps.Count}");

        // Assert: StepNumber tăng dần liên tục từ 1, Title và Description hợp lệ
        for (int i = 0; i < steps.Count; i++)
        {
            var expectedStepNumber = i + 1;
            var step = steps[i];

            Assert.Equal(recipeId, step.RecipeId);
            Assert.Equal(expectedStepNumber, step.StepNumber);
            Assert.False(string.IsNullOrWhiteSpace(step.Title));
            Assert.False(string.IsNullOrWhiteSpace(step.Description));
            Assert.Contains(recipeTitle, step.Description);
        }
    }

    [Fact]
    public void GenerateImages_ShouldHaveExactlyOnePrimaryImage_AndOrderedGalleryImages()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var recipeTitle = "Cá Lóc Kho Tộ";

        // Act
        var images = RecipeDetailSeeder.GenerateImages(recipeId, recipeTitle);

        // Assert
        Assert.NotNull(images);
        Assert.NotEmpty(images);

        // Có đúng 1 ảnh chính
        var primaryImage = Assert.Single(images, img => img.IsPrimary);
        Assert.Equal(0, primaryImage.OrderIndex);

        // Các ảnh phụ
        var galleryImages = images.Where(img => !img.IsPrimary).ToList();
        Assert.NotEmpty(galleryImages);
        Assert.All(galleryImages, img => Assert.True(img.OrderIndex > 0));

        // URL và RecipeId hợp lệ
        Assert.All(images, img =>
        {
            Assert.Equal(recipeId, img.RecipeId);
            Assert.False(string.IsNullOrWhiteSpace(img.OriginalUrl));
            Assert.Contains(recipeId.ToString(), img.OriginalUrl);
        });
    }

    [Fact]
    public void FullRecipeSetSimulation_ShouldSatisfyAllLab2Requirements_For100Recipes()
    {
        // Mô phỏng kiểm tra trên toàn bộ 100 recipes
        var random = new Random(202611);

        for (int r = 1; r <= 100; r++)
        {
            var recipeId = Guid.NewGuid();
            var title = $"Món ngon số {r}";

            var ingredients = RecipeDetailSeeder.GenerateIngredients(recipeId, random);
            var steps = RecipeDetailSeeder.GenerateSteps(recipeId, title);
            var images = RecipeDetailSeeder.GenerateImages(recipeId, title);

            // Kiểm tra ràng buộc của Thành viên 4 cho từng recipe
            Assert.True(ingredients.Count >= 10, $"Recipe #{r} có ít hơn 10 nguyên liệu.");
            Assert.True(steps.Count >= 5, $"Recipe #{r} có ít hơn 5 bước.");
            Assert.Single(images, x => x.IsPrimary);

            // Kiểm tra thứ tự
            var ingredientOrderIndices = ingredients.Select(i => i.OrderIndex).ToList();
            Assert.Equal(ingredientOrderIndices.OrderBy(x => x).ToList(), ingredientOrderIndices);

            var stepNumbers = steps.Select(s => s.StepNumber).ToList();
            Assert.Equal(Enumerable.Range(1, steps.Count).ToList(), stepNumbers);
        }
    }
}
