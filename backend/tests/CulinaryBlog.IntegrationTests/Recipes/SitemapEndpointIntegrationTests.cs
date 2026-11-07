using System.Net;
using CulinaryBlog.IntegrationTests.Infrastructure;
using Xunit;

namespace CulinaryBlog.IntegrationTests.Recipes;

public class SitemapEndpointIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public SitemapEndpointIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetSitemapXml_Returns_200_And_Valid_Xml_Content()
    {
        // Arrange
        var client = _factory.CreateGuestClient();

        // Act
        var response = await client.GetAsync("/sitemap.xml");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/xml", response.Content.Headers.ContentType?.MediaType);

        var xml = await response.Content.ReadAsStringAsync();
        Assert.NotNull(xml);
        Assert.Contains("<urlset", xml);
        Assert.Contains("http://www.sitemaps.org/schemas/sitemap/0.9", xml);
    }
}

