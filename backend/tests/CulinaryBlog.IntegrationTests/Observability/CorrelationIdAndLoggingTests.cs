namespace CulinaryBlog.IntegrationTests.Observability;

using System.Net;
using CulinaryBlog.IntegrationTests.Infrastructure;
using Xunit;

public class CorrelationIdAndLoggingTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CorrelationIdAndLoggingTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CorrelationId_WhenClientDoesNotSendHeader_ServerGeneratesAndReturnsGuidHeader()
    {
        // Arrange
        var client = _factory.CreateGuestClient();

        // Act
        var response = await client.GetAsync("/api/v1/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-ID"), "Missing X-Correlation-ID response header");
        var correlationId = response.Headers.GetValues("X-Correlation-ID").FirstOrDefault();
        Assert.False(string.IsNullOrWhiteSpace(correlationId));
        Assert.True(Guid.TryParse(correlationId, out _), "Generated CorrelationId must be a valid Guid");
    }

    [Fact]
    public async Task CorrelationId_WhenClientSendsHeader_ServerEchoesSameCorrelationId()
    {
        // Arrange
        var client = _factory.CreateGuestClient();
        var customCorrelationId = "cid-test-lab08-custom-123456";
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/");
        request.Headers.Add("X-Correlation-ID", customCorrelationId);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-ID"), "Missing X-Correlation-ID response header");
        var echoCorrelationId = response.Headers.GetValues("X-Correlation-ID").FirstOrDefault();
        Assert.Equal(customCorrelationId, echoCorrelationId);
    }
}

