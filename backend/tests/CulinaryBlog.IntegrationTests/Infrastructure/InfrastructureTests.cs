using System.Net;
using Xunit;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace CulinaryBlog.IntegrationTests.Infrastructure;

public class InfrastructureTests : IClassFixture<CustomWebApplicationFactory>
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HealthAndReadiness_UseConfiguredEndpoints_WithSameRedisPrecedenceAsApplication(bool primaryConnection)
    {
        using var redis = new TcpListener(IPAddress.Loopback, 0);
        using var minio = new TcpListener(IPAddress.Loopback, 0);
        redis.Start();
        minio.Start();
        var redisPort = ((IPEndPoint)redis.LocalEndpoint).Port;
        var minioPort = ((IPEndPoint)minio.LocalEndpoint).Port;
        await using var app = _factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Redis"] = primaryConnection ? $"127.0.0.1:{redisPort},password=must-not-leak" : null,
                ["Redis:ConnectionString"] = primaryConnection ? "invalid.example:1" : $"127.0.0.1:{redisPort}",
                ["Redis:Host"] = "invalid.example",
                ["Minio:Endpoint"] = $"http://127.0.0.1:{minioPort}",
                ["Minio:PublicUrl"] = "http://invalid.example:1"
            })));
        using var client = app.CreateClient();
        using var response = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("must-not-leak", body);
        using var document = JsonDocument.Parse(body);
        var checks = document.RootElement.GetProperty("checks");
        Assert.Equal($"127.0.0.1:{redisPort}", checks.GetProperty("redis").GetProperty("endpoint").GetString());
        Assert.Equal("Healthy", checks.GetProperty("redis").GetProperty("status").GetString());
        Assert.Equal($"http://127.0.0.1:{minioPort}", checks.GetProperty("storage").GetProperty("endpoint").GetString());
        Assert.Equal("Healthy", checks.GetProperty("storage").GetProperty("status").GetString());
        using var ready = await client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.OK, ready.StatusCode);
        redis.Stop();
        using var unavailable = await client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, unavailable.StatusCode);
    }

    private readonly CustomWebApplicationFactory _factory;

    public InfrastructureTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RootApiEndpoint_ReturnsSuccessStatusCode()
    {
        // Arrange
        var client = _factory.CreateGuestClient();

        // Act
        var response = await client.GetAsync("/api/v1/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthLiveEndpoint_ReturnsSuccessStatusCode()
    {
        // Arrange
        var client = _factory.CreateGuestClient();

        // Act
        var response = await client.GetAsync("/health/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
