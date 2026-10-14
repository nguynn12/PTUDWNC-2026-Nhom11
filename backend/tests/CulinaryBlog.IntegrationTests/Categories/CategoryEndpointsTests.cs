using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CulinaryBlog.API.Common.Models;
using CulinaryBlog.Application.Features.Categories.Commands.CreateCategory;
using CulinaryBlog.Application.Features.Categories.Commands.UpdateCategory;
using CulinaryBlog.Application.Features.Categories.DTOs;
using CulinaryBlog.IntegrationTests.Infrastructure;
using Xunit;

namespace CulinaryBlog.IntegrationTests.Categories;

public class CategoryEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public CategoryEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetCategories_PublicAccess_ReturnsSuccessAndList()
    {
        // Arrange
        var client = _factory.CreateGuestClient();

        // Act
        var response = await client.GetAsync("/api/v1/categories");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<List<CategoryDto>>>(JsonOptions);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Data);
        Assert.NotEmpty(envelope.Data);
        Assert.Contains(envelope.Data, c => c.Slug == "mon-khai-vi");
        Assert.Contains(envelope.Data, c => c.Slug == "mon-chinh");
    }

    [Fact]
    public async Task GetCategoryBySlug_ExistingSlug_ReturnsCategoryDetail()
    {
        // Arrange
        var client = _factory.CreateGuestClient();

        // Act
        var response = await client.GetAsync("/api/v1/categories/mon-chinh");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<CategoryDetailDto>>(JsonOptions);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Data);
        Assert.Equal("Món Chính", envelope.Data.Name);
        Assert.Equal("mon-chinh", envelope.Data.Slug);
    }

    [Fact]
    public async Task GetCategoryBySlug_NonExistingSlug_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateGuestClient();

        // Act
        var response = await client.GetAsync("/api/v1/categories/danh-muc-hoan-toan-khong-ton-tai");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_GuestAnonymous_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateGuestClient();
        var request = new CreateCategoryRequest
        {
            Name = "Danh Mục Thử Nghiệm",
            Description = "Mô tả",
            ImageUrl = "/img.jpg",
            OrderIndex = 10
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/categories", request);

        // Assert: Khách vãng lai chưa xác thực phải bị chặn 401
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_AuthorRole_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateAuthorClient();
        var request = new CreateCategoryRequest
        {
            Name = "Danh Mục Của Tác Giả",
            Description = "Mô tả",
            ImageUrl = "/img.jpg",
            OrderIndex = 10
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/categories", request);

        // Assert: Tác giả không có quyền tạo danh mục, phải bị chặn 403
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_AdminRole_ValidData_ReturnsCreated()
    {
        // Arrange
        var client = _factory.CreateAdminClient();
        var uniqueName = "Đồ Uống Giải Khát " + Guid.NewGuid().ToString()[..6];
        var request = new CreateCategoryRequest
        {
            Name = uniqueName,
            Description = "Các loại nước ép và sinh tố mát lành",
            ImageUrl = "/drinks.jpg",
            OrderIndex = 5
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/categories", request);

        // Assert: Admin có quyền tạo thành công 201 Created
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>(JsonOptions);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Data);
        Assert.Equal(uniqueName, envelope.Data.Name);
        Assert.False(string.IsNullOrWhiteSpace(envelope.Data.Slug));
    }

    [Fact]
    public async Task CreateCategory_AdminRole_InvalidData_ReturnsBadRequestOrUnprocessable()
    {
        // Arrange
        var client = _factory.CreateAdminClient();
        var request = new CreateCategoryRequest
        {
            Name = "",
            Description = "Tên bị rỗng vi phạm validation",
            ImageUrl = null,
            OrderIndex = 0
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/categories", request);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.UnprocessableEntity,
            $"Expected BadRequest or UnprocessableEntity but got {response.StatusCode}");
    }

    [Fact]
    public async Task UpdateCategory_AuthorRole_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateAuthorClient();
        var request = new UpdateCategoryRequest
        {
            Name = "Tên Mới Của Author",
            Description = "Mô tả sửa",
            ImageUrl = "/new.jpg",
            OrderIndex = 1
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/categories/{CustomWebApplicationFactory.AppetizerCategoryId}", request);

        // Assert: Author không có quyền sửa danh mục -> 403 Forbidden
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCategory_AdminRole_ValidData_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateAdminClient();
        var updatedName = "Món Khai Vị Cao Cấp";
        var request = new UpdateCategoryRequest
        {
            Name = updatedName,
            Description = "Mô tả mới cập nhật",
            ImageUrl = "/new_appetizer.jpg",
            OrderIndex = 1
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/categories/{CustomWebApplicationFactory.AppetizerCategoryId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>(JsonOptions);
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Data);
        Assert.Equal(updatedName, envelope.Data.Name);
    }

    [Fact]
    public async Task DeleteCategory_AuthorRole_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateAuthorClient();

        // Act
        var response = await client.DeleteAsync($"/api/v1/categories/{CustomWebApplicationFactory.AppetizerCategoryId}");

        // Assert: Author không có quyền xóa danh mục -> 403 Forbidden
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_AdminRole_CategoryWithActiveRecipes_ReturnsConflict()
    {
        // Arrange
        var client = _factory.CreateAdminClient();

        // Act: MainDishCategoryId đang có Recipe "Phở Bò Tái Lăn Hà Nội" liên kết
        var response = await client.DeleteAsync($"/api/v1/categories/{CustomWebApplicationFactory.MainDishCategoryId}");

        // Assert: FR-CAT-005 chặn xóa khi còn recipe -> 409 Conflict
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_AdminRole_CategoryWithoutRecipes_ReturnsNoContent()
    {
        // Arrange: Admin tạo danh mục mới tinh không có bất kỳ Recipe nào liên kết
        var adminClient = _factory.CreateAdminClient();
        var categoryName = "Danh Mục Xóa Thử " + Guid.NewGuid().ToString()[..6];
        var createResponse = await adminClient.PostAsJsonAsync("/api/v1/categories",
            new CreateCategoryRequest
            {
                Name = categoryName,
                Description = "Danh mục tạm để test xóa",
                ImageUrl = null,
                OrderIndex = 99
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>(JsonOptions);
        Assert.NotNull(created?.Data);

        // Act: Xóa danh mục vừa tạo
        var deleteResponse = await adminClient.DeleteAsync($"/api/v1/categories/{created.Data.Id}");

        // Assert: Xóa thành công trả về 204 NoContent
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Kiểm chứng truy vấn lại bằng slug phải nhận 404
        var getResponse = await adminClient.GetAsync($"/api/v1/categories/{created.Data.Slug}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
