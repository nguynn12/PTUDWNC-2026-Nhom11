using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CulinaryBlog.API.Common.Models;
using CulinaryBlog.Application.Features.Recipes.DTOs;
using CulinaryBlog.IntegrationTests.Infrastructure;
using Xunit;

namespace CulinaryBlog.IntegrationTests.Recipes;

public class RecipeDiscoveryEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public RecipeDiscoveryEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetRecipes_ReturnsPagedEnvelope_AndExcludesDraftRecipes()
    {
        // Arrange
        var client = _factory.CreateGuestClient();

        // Act: Duyệt danh sách công thức công khai
        var response = await client.GetAsync("/api/v1/recipes?page=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<List<RecipeSummaryDto>>>(JsonOptions);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Data);
        Assert.NotNull(envelope.Meta);

        // Đảm bảo chứa công thức Published ("Phở Bò Tái Lăn Hà Nội")
        Assert.Contains(envelope.Data, r => r.Slug == "pho-bo-tai-lan-ha-noi");

        // Đảm bảo công thức Draft ("Gỏi Cuốn Tôm Thịt Bí Mật") tuyệt đối không xuất hiện
        Assert.DoesNotContain(envelope.Data, r => r.Slug == "goi-cuon-tom-thit-bi-mat");
    }

    [Fact]
    public async Task GetRecipes_FilterByCategoryId_ReturnsOnlyMatchingCategory()
    {
        // Arrange
        var client = _factory.CreateGuestClient();
        var categoryId = CustomWebApplicationFactory.MainDishCategoryId;

        // Act: Lọc theo danh mục Món Chính
        var response = await client.GetAsync($"/api/v1/recipes?categoryId={categoryId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<List<RecipeSummaryDto>>>(JsonOptions);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Data);

        // Mọi công thức trả về đều phải thuộc đúng CategoryId đã lọc
        Assert.All(envelope.Data, r => Assert.Equal(categoryId, r.CategoryId));
    }

    [Fact]
    public async Task GetRecipeBySlug_PublishedRecipe_ReturnsFullDetails()
    {
        // Arrange
        var client = _factory.CreateGuestClient();

        // Act: Xem chi tiết công thức đã xuất bản
        var response = await client.GetAsync("/api/v1/recipes/pho-bo-tai-lan-ha-noi");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<RecipeDetailDto>>(JsonOptions);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Data);
        Assert.Equal("Phở Bò Tái Lăn Hà Nội", envelope.Data.Title);
        Assert.Equal("pho-bo-tai-lan-ha-noi", envelope.Data.Slug);
        Assert.Equal("Published", envelope.Data.Status);
    }

    [Fact]
    public async Task GetRecipeBySlug_DraftRecipe_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateGuestClient();

        // Act: Cố tình truy cập công thức Draft qua đường dẫn công khai
        var response = await client.GetAsync("/api/v1/recipes/goi-cuon-tom-thit-bi-mat");

        // Assert: FR-RCP-002 bảo mật không được để lộ công thức nháp -> 404 Not Found
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetRecipeBySlug_NonExistingSlug_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateGuestClient();

        // Act
        var response = await client.GetAsync("/api/v1/recipes/cong-thuc-hoan-toan-khong-ton-tai");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SearchRecipes_MatchingKeyword_ReturnsRecipeList()
    {
        // Arrange
        var client = _factory.CreateGuestClient();

        // Act: Tìm kiếm từ khóa "Phở"
        var response = await client.GetAsync("/api/v1/recipes/search?q=Phở");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<List<RecipeSummaryDto>>>(JsonOptions);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Data);
        Assert.Contains(envelope.Data, r => r.Title.Contains("Phở"));
    }

    [Fact]
    public async Task SearchRecipes_NoMatchKeyword_ReturnsEmptyList()
    {
        // Arrange
        var client = _factory.CreateGuestClient();

        // Act: Tìm kiếm từ khóa không hề tồn tại
        var response = await client.GetAsync("/api/v1/recipes/search?q=MonAnKhongHeTonTaiTrongHeThong999");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<List<RecipeSummaryDto>>>(JsonOptions);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Data);
        Assert.Empty(envelope.Data);
    }
}
