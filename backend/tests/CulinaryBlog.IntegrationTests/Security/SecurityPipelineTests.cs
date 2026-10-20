using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CulinaryBlog.IntegrationTests.Security;

public class SecurityPipelineTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public SecurityPipelineTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Cors_AllowedFrontendOrigin_ReturnsAccessControlAllowOriginHeader()
    {
        // Arrange
        var client = _factory.CreateGuestClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/");
        request.Headers.Add("Origin", "http://localhost:3000");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"), "Missing Access-Control-Allow-Origin header");
        var allowOrigin = response.Headers.GetValues("Access-Control-Allow-Origin").FirstOrDefault();
        Assert.Equal("http://localhost:3000", allowOrigin);
    }

    [Fact]
    public async Task RateLimiting_NormalRequest_ReturnsXRateLimitLimitHeader()
    {
        // Arrange
        var client = _factory.CreateGuestClient();

        // Act
        var response = await client.GetAsync("/api/v1/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-RateLimit-Limit"), "Missing X-RateLimit-Limit header");
        var limitValue = response.Headers.GetValues("X-RateLimit-Limit").FirstOrDefault();
        Assert.Equal("100", limitValue);
    }

    [Fact]
    public async Task RateLimiting_Exceeds100RequestsPerMinute_Returns429TooManyRequestsAndProblemDetails()
    {
        // Arrange: Dùng client với IP giả lập riêng biệt để cô lập phạm vi rate limit
        var client = _factory.CreateGuestClient();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "198.51.100.77");

        // Act 1: Bắn 100 requests hợp lệ tiêu thụ hết định mức Fixed Window (100 permits / phút)
        for (var i = 0; i < 100; i++)
        {
            var res = await client.GetAsync("/api/v1/");
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        }

        // Act 2: Gửi request thứ 101 vi phạm hạn ngạch Rate Limit
        var rejectedResponse = await client.GetAsync("/api/v1/");

        // Assert: Hệ thống phải trả về mã 429 Too Many Requests
        Assert.Equal(HttpStatusCode.TooManyRequests, rejectedResponse.StatusCode);

        // Kiểm tra đầy đủ các Header chuẩn Lab 5 & RFC 7807
        Assert.True(rejectedResponse.Headers.Contains("Retry-After"), "Missing Retry-After header on 429 response");
        Assert.True(rejectedResponse.Headers.Contains("X-RateLimit-Limit"), "Missing X-RateLimit-Limit header");
        Assert.Equal("100", rejectedResponse.Headers.GetValues("X-RateLimit-Limit").FirstOrDefault());
        Assert.True(rejectedResponse.Headers.Contains("X-RateLimit-Remaining"), "Missing X-RateLimit-Remaining header");
        Assert.Equal("0", rejectedResponse.Headers.GetValues("X-RateLimit-Remaining").FirstOrDefault());

        // Kiểm tra RFC 7807 Problem Details
        var problem = await rejectedResponse.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        Assert.NotNull(problem);
        Assert.Equal(429, problem.Status);
        Assert.Equal(ErrorCodes.RateLimitExceeded, problem.Type);
    }

    [Fact]
    public async Task HealthEndpoint_BypassesRateLimiter_EvenAfterManyRequests()
    {
        // Arrange: Client gửi nhiều requests tới /health/live
        var client = _factory.CreateGuestClient();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "198.51.100.88");

        // Act & Assert: Bắn 105 requests vào endpoint probes /health/live
        for (var i = 0; i < 105; i++)
        {
            var response = await client.GetAsync("/health/live");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
