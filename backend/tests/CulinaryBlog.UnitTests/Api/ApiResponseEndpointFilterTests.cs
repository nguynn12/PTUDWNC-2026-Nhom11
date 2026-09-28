using System.Text.Json;
using CulinaryBlog.API.Responses;
using CulinaryBlog.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;

namespace CulinaryBlog.UnitTests.Api;

/// <summary>Kiểm tra envelope <c>{data, meta}</c> theo RESOLVED-CONFLICTS C2.</summary>
public sealed class ApiResponseEndpointFilterTests
{
    public sealed record SampleDto(int Id, string Name);

    [Fact]
    public async Task InvokeAsync_TraVeDto_BocThanhData()
    {
        var dto = new SampleDto(1, "Phở bò");

        var result = await InvokeFilterAsync(dto);

        var envelope = Assert.IsType<ApiResponse<object>>(result);
        Assert.Same(dto, envelope.Data);
    }

    [Fact]
    public async Task InvokeAsync_TraVePagedResult_BocThanhDataVaMeta()
    {
        var paged = new PagedResult<SampleDto>(
            [new SampleDto(3, "Bún chả"), new SampleDto(4, "Cơm tấm")],
            page: 2,
            pageSize: 2,
            totalCount: 5);

        var result = await InvokeFilterAsync(paged);

        var envelope = Assert.IsType<PagedApiResponse<object?>>(result);
        Assert.Equal(2, envelope.Data.Count);
        Assert.Equal(new PaginationMeta(2, 2, 5, 3, HasNextPage: true, HasPreviousPage: true), envelope.Meta);
    }

    [Fact]
    public async Task InvokeAsync_TraVeIResult_GiuNguyen()
    {
        var noContent = TypedResults.NoContent();

        var result = await InvokeFilterAsync(noContent);

        Assert.Same(noContent, result);
    }

    [Fact]
    public async Task InvokeAsync_TraVeNull_GiuNguyenNull()
    {
        var result = await InvokeFilterAsync(null);

        Assert.Null(result);
    }

    [Fact]
    public void PagedApiResponse_SerializeJson_DungTenFieldTheoC2()
    {
        var paged = new PagedResult<SampleDto>([new SampleDto(1, "Gỏi cuốn")], page: 1, pageSize: 10, totalCount: 1);
        var envelope = ApiResponseEndpointFilter.Wrap(paged);

        using var json = JsonDocument.Parse(JsonSerializer.Serialize(envelope, JsonSerializerOptions.Web));
        var root = json.RootElement;

        Assert.Equal("Gỏi cuốn", root.GetProperty("data")[0].GetProperty("name").GetString());
        var meta = root.GetProperty("meta");
        Assert.Equal(1, meta.GetProperty("page").GetInt32());
        Assert.Equal(10, meta.GetProperty("pageSize").GetInt32());
        Assert.Equal(1, meta.GetProperty("total").GetInt32());
        Assert.Equal(1, meta.GetProperty("totalPages").GetInt32());
        Assert.False(meta.GetProperty("hasNextPage").GetBoolean());
        Assert.False(meta.GetProperty("hasPreviousPage").GetBoolean());
    }

    [Fact]
    public void ApiResults_Created_Tra201KemLocationVaEnvelope()
    {
        var dto = new SampleDto(7, "Canh chua");

        Created<ApiResponse<SampleDto>> result = ApiResults.Created("/api/v1/recipes/7", dto);

        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.Equal("/api/v1/recipes/7", result.Location);
        Assert.NotNull(result.Value);
        Assert.Same(dto, result.Value.Data);
    }

    private static async Task<object?> InvokeFilterAsync(object? endpointResult)
    {
        var filter = new ApiResponseEndpointFilter();
        var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext());

        return await filter.InvokeAsync(context, _ => ValueTask.FromResult(endpointResult));
    }
}
