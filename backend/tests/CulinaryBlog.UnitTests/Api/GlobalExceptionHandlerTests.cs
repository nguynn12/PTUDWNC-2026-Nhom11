using System.Text.Json;
using CulinaryBlog.API.ErrorHandling;
using CulinaryBlog.Application.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CulinaryBlog.UnitTests.Api;

/// <summary>
/// Kiểm tra body RFC 7807 thực tế được ghi ra response (định dạng SRS 5.2:
/// <c>type, title, status, detail, errors</c> + <c>traceId</c>).
/// </summary>
public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_ValidationFailed_Ghi422VaDanhSachLoi()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["email"] = ["Email không đúng định dạng."],
        };

        var (status, body) = await HandleAsync(new ValidationFailedException(errors));

        Assert.Equal(StatusCodes.Status422UnprocessableEntity, status);
        Assert.Equal("VALIDATION_ERROR", body.GetProperty("type").GetString());
        Assert.Equal(422, body.GetProperty("status").GetInt32());
        Assert.Equal("/api/v1/recipes", body.GetProperty("instance").GetString());
        Assert.Equal(
            "Email không đúng định dạng.",
            body.GetProperty("errors").GetProperty("email")[0].GetString());
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("traceId").GetString()));
    }

    [Fact]
    public async Task TryHandleAsync_NotFound_Ghi404KhongCoErrors()
    {
        var (status, body) = await HandleAsync(
            new NotFoundException(ErrorCodes.CategoryNotFound, "Không tìm thấy danh mục."));

        Assert.Equal(StatusCodes.Status404NotFound, status);
        Assert.Equal("CATEGORY_NOT_FOUND", body.GetProperty("type").GetString());
        Assert.Equal("Không tìm thấy danh mục.", body.GetProperty("detail").GetString());
        Assert.False(body.TryGetProperty("errors", out _));
    }

    [Fact]
    public async Task TryHandleAsync_LoiKhongXacDinh_Ghi500VaAnThongDiepGoc()
    {
        var (status, body) = await HandleAsync(new InvalidOperationException("Password=bi-mat"));

        Assert.Equal(StatusCodes.Status500InternalServerError, status);
        Assert.Equal("INTERNAL_SERVER_ERROR", body.GetProperty("type").GetString());
        Assert.DoesNotContain("bi-mat", body.GetRawText(), StringComparison.Ordinal);
    }

    private static async Task<(int Status, JsonElement Body)> HandleAsync(Exception exception)
    {
        using var services = new ServiceCollection()
            .AddLogging()
            .AddProblemDetails()
            .BuildServiceProvider();

        using var responseBody = new MemoryStream();
        var httpContext = new DefaultHttpContext { RequestServices = services };
        httpContext.Request.Method = HttpMethods.Post;
        httpContext.Request.Path = "/api/v1/recipes";
        httpContext.Response.Body = responseBody;

        var handler = new GlobalExceptionHandler(
            services.GetRequiredService<IProblemDetailsService>(),
            NullLogger<GlobalExceptionHandler>.Instance);

        var handled = await handler.TryHandleAsync(httpContext, exception, TestContext.Current.CancellationToken);

        Assert.True(handled);
        responseBody.Position = 0;
        using var json = JsonDocument.Parse(responseBody);
        return (httpContext.Response.StatusCode, json.RootElement.Clone());
    }
}
