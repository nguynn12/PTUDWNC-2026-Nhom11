using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.RateLimiting;
using CulinaryBlog.API.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CulinaryBlog.UnitTests.Api;

/// <summary>
/// Unit Tests kiểm tra cấu hình GeneralRateLimiting (100 req/min/IP) và phòng chống IP Spoofing (SEC-03).
/// </summary>
public sealed class GeneralRateLimitingTests
{
    private static HttpContext ContextFrom(string ip)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(ip);
        return context;
    }

    [Fact]
    public void ClientKey_UsesRemoteIpAddress_AndIgnoresSpoofedXForwardedFor()
    {
        // Arrange (SEC-03: Không đọc trực tiếp X-Forwarded-For tránh bị giả mạo IP)
        var context = ContextFrom("192.168.1.50");
        context.Request.Headers["X-Forwarded-For"] = "10.0.0.1, 10.0.0.2";

        // Act
        var clientKey = GeneralRateLimiting.ClientKey(context);

        // Assert
        Assert.Equal("192.168.1.50", clientKey);
    }

    [Fact]
    public void CungIp_CungPartition_KhacIp_KhacPartition()
    {
        Assert.Equal(
            GeneralRateLimiting.GetPartition(ContextFrom("192.168.1.1")).PartitionKey,
            GeneralRateLimiting.GetPartition(ContextFrom("192.168.1.1")).PartitionKey);
        Assert.NotEqual(
            GeneralRateLimiting.GetPartition(ContextFrom("192.168.1.1")).PartitionKey,
            GeneralRateLimiting.GetPartition(ContextFrom("192.168.1.2")).PartitionKey);
    }

    [Fact]
    public async Task BiTuChoi_Tra429ProblemDetails_VaHeadersRateLimit()
    {
        using var services = new ServiceCollection().AddLogging().AddProblemDetails().BuildServiceProvider();
        using var body = new MemoryStream();
        var httpContext = ContextFrom("192.168.1.1");
        httpContext.RequestServices = services;
        httpContext.Request.Path = "/api/v1/recipes";
        httpContext.Response.Body = body;

        var partition = GeneralRateLimiting.GetPartition(httpContext);
        using var limiter = partition.Factory(partition.PartitionKey);
        for (var i = 0; i < GeneralRateLimiting.PermitLimit; i++)
        {
            limiter.AttemptAcquire().Dispose();
        }

        using var lease = limiter.AttemptAcquire();
        await GeneralRateLimiting.OnRejectedAsync(
            new OnRejectedContext { HttpContext = httpContext, Lease = lease },
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCodes.Status429TooManyRequests, httpContext.Response.StatusCode);
        Assert.Equal("100", httpContext.Response.Headers["X-RateLimit-Limit"]);
        Assert.Equal("0", httpContext.Response.Headers["X-RateLimit-Remaining"]);
        Assert.False(string.IsNullOrEmpty(httpContext.Response.Headers.RetryAfter));

        body.Position = 0;
        using var json = await JsonDocument.ParseAsync(body, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("RATE_LIMIT_EXCEEDED", json.RootElement.GetProperty("type").GetString());
    }
}
