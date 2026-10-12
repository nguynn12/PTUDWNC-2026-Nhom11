namespace CulinaryBlog.UnitTests.Categories;

using System;
using System.Text.Json;
using CulinaryBlog.API.Common.Models;
using CulinaryBlog.Application.Features.Categories.DTOs;
using Xunit;

/// <summary>
/// Unit Tests kiểm tra tính chuẩn hóa của Response Envelope theo SRS Chương 5.2/8 và Lịch sử chuẩn hóa v1.3.0:
/// - Success response dùng { "data": ..., "meta": ... }.
/// - Khi không có metadata (danh sách không phân trang, chi tiết), thuộc tính 'meta' phải được loại bỏ (không serialize null).
/// </summary>
public class ApiResponseEnvelopeTests
{
    [Fact]
    public void ApiResponse_ShouldSerializeData_AndOmitMetaWhenNull()
    {
        // Arrange
        var categoryDto = new CategoryDto
        {
            Id = Guid.NewGuid(),
            Name = "Món Tráng Miệng",
            Slug = "mon-trang-mieng",
            Description = "Các món chè, bánh ngọt",
            ImageUrl = null,
            OrderIndex = 1,
            RecipeCount = 5,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var response = new ApiResponse<CategoryDto>(categoryDto);

        // Act
        var json = JsonSerializer.Serialize(response);

        // Assert
        Assert.Contains("\"data\"", json);
        Assert.Contains("\"mon-trang-mieng\"", json);
        Assert.DoesNotContain("\"meta\"", json); // meta null phải bị omit theo JsonIgnoreCondition.WhenWritingNull
    }

    [Fact]
    public void ApiResponse_ShouldIncludeMeta_WhenProvided()
    {
        // Arrange
        var items = new[] { "Item1", "Item2" };
        var paginationMeta = new
        {
            page = 1,
            pageSize = 10,
            total = 2,
            totalPages = 1
        };

        var response = new ApiResponse<string[]>(items, paginationMeta);

        // Act
        var json = JsonSerializer.Serialize(response);

        // Assert
        Assert.Contains("\"data\"", json);
        Assert.Contains("\"meta\"", json);
        Assert.Contains("\"totalPages\":1", json);
    }
}
