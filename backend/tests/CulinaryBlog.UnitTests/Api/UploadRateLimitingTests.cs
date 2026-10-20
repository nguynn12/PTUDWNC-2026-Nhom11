using System.Net;
using System.Text.Json;
using System.Threading.RateLimiting;
using CulinaryBlog.API.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CulinaryBlog.UnitTests.Api;

/// <summary>Lab 5 TV4: Upload rate limit 5 request/phút/IP cho endpoint POST /api/v1/recipes/{id}/images.</summary>
public sealed class UploadRateLimitingTests
{
    private static HttpContext ContextFrom(string ip)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(ip);
        return context;
    }

    [Fact]
    public void CungIp_CungPartition_KhacIp_KhacPartition()
    {
        Assert.Equal(
            UploadRateLimiting.GetPartition(ContextFrom("192.168.1.100")).PartitionKey,
            UploadRateLimiting.GetPartition(ContextFrom("192.168.1.100")).PartitionKey);
        Assert.NotEqual(
            UploadRateLimiting.GetPartition(ContextFrom("192.168.1.100")).PartitionKey,
            UploadRateLimiting.GetPartition(ContextFrom("192.168.1.200")).PartitionKey);
    }

    [Fact]
    public void ChoPhep5RequestMoiPhut_Request6BiTuChoi()
    {
        var partition = UploadRateLimiting.GetPartition(ContextFrom("192.168.1.100"));
        using var limiter = partition.Factory(partition.PartitionKey);

        for (var i = 0; i < UploadRateLimiting.PermitLimit; i++)
        {
            using var lease = limiter.AttemptAcquire();
            Assert.True(lease.IsAcquired);
        }

        using var rejected = limiter.AttemptAcquire();
        Assert.False(rejected.IsAcquired);
        Assert.True(rejected.TryGetMetadata(MetadataName.RetryAfter, out _));
    }

    [Fact]
    public async Task BiTuChoi_Tra429ProblemDetailsRateLimitExceeded()
    {
        using var services = new ServiceCollection().AddLogging().AddProblemDetails().BuildServiceProvider();
        using var body = new MemoryStream();
        var httpContext = ContextFrom("192.168.1.100");
        httpContext.RequestServices = services;
        httpContext.Request.Path = "/api/v1/recipes/a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d/images";
        httpContext.Response.Body = body;

        var partition = UploadRateLimiting.GetPartition(httpContext);
        using var limiter = partition.Factory(partition.PartitionKey);
        for (var i = 0; i < UploadRateLimiting.PermitLimit; i++)
        {
            limiter.AttemptAcquire().Dispose();
        }

        using var lease = limiter.AttemptAcquire();
        await UploadRateLimiting.OnRejectedAsync(
            new OnRejectedContext { HttpContext = httpContext, Lease = lease },
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCodes.Status429TooManyRequests, httpContext.Response.StatusCode);
        Assert.False(string.IsNullOrEmpty(httpContext.Response.Headers.RetryAfter));
        body.Position = 0;
        using var json = await JsonDocument.ParseAsync(body, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("RATE_LIMIT_EXCEEDED", json.RootElement.GetProperty("type").GetString());
        Assert.Equal(429, json.RootElement.GetProperty("status").GetInt32());
        Assert.Contains("5", json.RootElement.GetProperty("detail").GetString());
    }
}
