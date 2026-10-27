using System.Net;
using System.Text.Json;
using System.Threading.RateLimiting;
using CulinaryBlog.API.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CulinaryBlog.UnitTests.Api;

/// <summary>SRS NFR-SEC: auth rate limit 10 request/phút/IP.</summary>
public sealed class AuthRateLimitingTests
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
            AuthRateLimiting.GetPartition(ContextFrom("10.0.0.1")).PartitionKey,
            AuthRateLimiting.GetPartition(ContextFrom("10.0.0.1")).PartitionKey);
        Assert.NotEqual(
            AuthRateLimiting.GetPartition(ContextFrom("10.0.0.1")).PartitionKey,
            AuthRateLimiting.GetPartition(ContextFrom("10.0.0.2")).PartitionKey);
    }

    [Fact]
    public void ChoPhep10RequestMoiPhut_Request11BiTuChoi()
    {
        var partition = AuthRateLimiting.GetPartition(ContextFrom("10.0.0.1"));
        using var limiter = partition.Factory(partition.PartitionKey);

        for (var i = 0; i < 10; i++)
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
        var httpContext = ContextFrom("10.0.0.1");
        httpContext.RequestServices = services;
        httpContext.Request.Path = "/api/v1/auth/login";
        httpContext.Response.Body = body;

        var partition = AuthRateLimiting.GetPartition(httpContext);
        using var limiter = partition.Factory(partition.PartitionKey);
        for (var i = 0; i < AuthRateLimiting.PermitLimit; i++)
        {
            limiter.AttemptAcquire().Dispose();
        }

        using var lease = limiter.AttemptAcquire();
        await AuthRateLimiting.OnRejectedAsync(
            new OnRejectedContext { HttpContext = httpContext, Lease = lease },
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCodes.Status429TooManyRequests, httpContext.Response.StatusCode);
        Assert.False(string.IsNullOrEmpty(httpContext.Response.Headers.RetryAfter));
        body.Position = 0;
        using var json = await JsonDocument.ParseAsync(body, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("RATE_LIMIT_EXCEEDED", json.RootElement.GetProperty("type").GetString());
    }
}
