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

    [Theory]
    [InlineData(null, 428)]
    [InlineData("123", 400)]
    [InlineData("W/\"123\"", 400)]
    [InlineData("\"0\"", 400)]
    [InlineData("\"001\"", 400)]
    [InlineData("\"4294967296\"", 400)]
    [InlineData("\"123\"", 404)]
    public async Task Lifecycle_RejectsMissingOrInvalidStrongIfMatchBeforeMutation(string? token, int status)
    {
        using var client = _factory.CreateAdminClient();
        using var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/recipes/{Guid.NewGuid()}/archive");
        if (token != null) request.Headers.TryAddWithoutValidation("If-Match", token);
        using var response = await client.SendAsync(request);
        Assert.Equal(status, (int)response.StatusCode);
    }

    [Fact]
    public async Task MineSearch_FindsOwnDraftBeforePaging_AndEmptyPrivateResultDoesNotPoisonPublicCache()
    {
        using var author = _factory.CreateAuthorClient();
        using var match = await author.GetAsync("/api/v1/recipes?mine=true&q=bi-mat&pageSize=1");
        match.EnsureSuccessStatusCode();
        using var result = JsonDocument.Parse(await match.Content.ReadAsStringAsync());
        Assert.Equal(1, result.RootElement.GetProperty("meta").GetProperty("total").GetInt32());
        Assert.Equal(CustomWebApplicationFactory.DraftRecipeId.ToString(), result.RootElement.GetProperty("data")[0].GetProperty("id").GetString());

        using var empty = await author.GetAsync("/api/v1/recipes?mine=true&q=khong-co-mon-nay&pageSize=1");
        empty.EnsureSuccessStatusCode();
        using var noMatches = JsonDocument.Parse(await empty.Content.ReadAsStringAsync());
        Assert.Empty(noMatches.RootElement.GetProperty("data").EnumerateArray());

        using var guest = _factory.CreateGuestClient();
        using var response = await guest.GetAsync("/api/v1/recipes?pageSize=1");
        response.EnsureSuccessStatusCode();
        using var publicResult = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.NotEmpty(publicResult.RootElement.GetProperty("data").EnumerateArray());
    }

    [Fact]
    public async Task RecipeLifecycle_HttpFlow_CoversCreateEditNutritionPublishArchiveDeleteRestoreAndPurge()
    {
        using var admin = _factory.CreateAdminClient();
        using var author = _factory.CreateAuthorClient();
        using var guest = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var payload = new
        {
            title = $"Cơm rau vòng đời {Guid.NewGuid():N}", description = "Công thức kiểm thử riêng cho vòng đời qua HTTP.",
            prepTimeMinutes = 10, cookTimeMinutes = 20, servings = 2, difficulty = 1,
            categoryId = CustomWebApplicationFactory.MainDishCategoryId,
            nutrition = new { calories = 300, protein = 10, carbohydrates = 40, fat = 8, fiber = 3, sodium = 100 }
        };
        using var unauthorized = await guest.PostAsJsonAsync("/api/v1/recipes", payload);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);
        using var created = await admin.PostAsJsonAsync("/api/v1/recipes", payload);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var createdJson = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var id = createdJson.RootElement.GetProperty("data").GetProperty("id").GetGuid();
        var oldSlug = createdJson.RootElement.GetProperty("data").GetProperty("slug").GetString();
        var path = $"/api/v1/recipes/{id}";
        using var forbidden = await author.GetAsync(path);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        using var detail = await admin.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);

        // InMemory không sinh xmin; PostgreSQL thật được kiểm tra riêng bằng E2E/HTTP.
        async Task<JsonElement> Mutate(HttpMethod method, string url, object? body = null, int expected = 200)
        {
            using var request = new HttpRequestMessage(method, url);
            request.Headers.TryAddWithoutValidation("If-Match", "\"1\"");
            if (body != null) request.Content = JsonContent.Create(body);
            using var response = await admin.SendAsync(request);
            Assert.Equal(expected, (int)response.StatusCode);
            if (expected != 200) return default;
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return json.RootElement.GetProperty("data").Clone();
        }
        var editedPayload = JsonSerializer.SerializeToNode(payload)!;
        editedPayload["title"] = payload.title + " mới";
        var updated = await Mutate(HttpMethod.Put, path, editedPayload);
        Assert.Equal(300, updated.GetProperty("nutrition").GetProperty("calories").GetInt32());
        using var privateOldSlug = await guest.GetAsync($"/api/v1/recipes/{oldSlug}");
        Assert.Equal(HttpStatusCode.NotFound, privateOldSlug.StatusCode);
        await Mutate(HttpMethod.Patch, path + "/publish", expected: 422);
        using var ingredient = await admin.PostAsJsonAsync(path + "/ingredients", new { name = "Gạo", quantity = 200, unit = "g", orderIndex = 1 });
        Assert.Equal(HttpStatusCode.Created, ingredient.StatusCode);
        using var step = await admin.PostAsJsonAsync(path + "/steps", new { stepNumber = 1, title = "Nấu cơm", description = "Vo gạo sạch và nấu chín mềm với lượng nước phù hợp." });
        Assert.Equal(HttpStatusCode.Created, step.StatusCode);
        Assert.Equal("Published", (await Mutate(HttpMethod.Patch, path + "/publish")).GetProperty("status").GetString());
        using var oldSlugRedirect = await guest.GetAsync($"/api/v1/recipes/{oldSlug}");
        Assert.Equal(HttpStatusCode.MovedPermanently, oldSlugRedirect.StatusCode);
        Assert.Equal("Published", (await Mutate(HttpMethod.Patch, path + "/publish")).GetProperty("status").GetString());
        Assert.Equal("Draft", (await Mutate(HttpMethod.Patch, path + "/unpublish")).GetProperty("status").GetString());
        Assert.Equal("Draft", (await Mutate(HttpMethod.Patch, path + "/unpublish")).GetProperty("status").GetString());
        Assert.Equal("Archived", (await Mutate(HttpMethod.Patch, path + "/archive")).GetProperty("status").GetString());
        Assert.Equal("Draft", (await Mutate(HttpMethod.Patch, path + "/unarchive")).GetProperty("status").GetString());
        await Mutate(HttpMethod.Delete, path, expected: 204);
        using var trash = await admin.GetAsync("/api/v1/admin/recipes/trash");
        Assert.Equal(HttpStatusCode.OK, trash.StatusCode);
        Assert.Contains(id.ToString(), await trash.Content.ReadAsStringAsync());
        Assert.Equal("Draft", (await Mutate(HttpMethod.Post, $"/api/v1/admin/recipes/{id}/restore")).GetProperty("status").GetString());
        await Mutate(HttpMethod.Delete, path, expected: 204);
        await Mutate(HttpMethod.Delete, $"/api/v1/admin/recipes/{id}/purge", expected: 204);
        using var missing = await admin.GetAsync(path);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
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

    [Fact]
    public async Task SearchRecipes_WithoutDiacritics_ReturnsMatchingRecipe()
    {
        // Arrange (FR-SRCH-001: Tìm kiếm tiếng Việt không dấu "Pho" phải khớp với "Phở")
        var client = _factory.CreateGuestClient();

        // Act
        var response = await client.GetAsync("/api/v1/recipes/search?q=pho");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<List<RecipeSummaryDto>>>(JsonOptions);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Data);
        Assert.Contains(envelope.Data, r => r.Slug == "pho-bo-tai-lan-ha-noi");
    }

    [Fact]
    public async Task SearchRecipes_WithPagination_ReturnsCorrectPageAndPageSize()
    {
        // Arrange (FR-SRCH-004: Phân trang tìm kiếm chuẩn)
        var client = _factory.CreateGuestClient();

        // Act
        var response = await client.GetAsync("/api/v1/recipes/search?q=bo&page=1&pageSize=1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var envelope = await response.Content.ReadFromJsonAsync<TestPagedEnvelope<List<RecipeSummaryDto>>>(JsonOptions);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Data);
        Assert.Single(envelope.Data);
        Assert.NotNull(envelope.Meta);
        Assert.Equal(1, envelope.Meta.Page);
        Assert.Equal(1, envelope.Meta.PageSize);
    }

    [Fact]
    public async Task SearchRecipes_QueryTooShort_ReturnsUnprocessableEntity()
    {
        // Arrange (FR-SRCH-001: Từ khóa phải từ 2 ký tự trở lên -> vi phạm FluentValidation ném 422)
        var client = _factory.CreateGuestClient();

        // Act: Gửi từ khóa chỉ có 1 ký tự "a"
        var response = await client.GetAsync("/api/v1/recipes/search?q=a");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task SearchRecipes_InvalidSortBy_ReturnsUnprocessableEntity()
    {
        // Arrange (FR-SRCH-003: Tiêu chí sắp xếp không nằm trong whitelist -> ném 422)
        var client = _factory.CreateGuestClient();

        // Act: Gửi sortBy không hợp lệ
        var response = await client.GetAsync("/api/v1/recipes/search?q=pho&sortBy=invalid_sort_column_xyz");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}

public record TestPagedEnvelope<T>(T Data, TestPaginationMeta? Meta);
public record TestPaginationMeta(int Page, int PageSize, int TotalCount, int TotalPages);
