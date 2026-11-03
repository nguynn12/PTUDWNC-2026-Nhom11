using System.Text;
using CulinaryBlog.Application.Common.Interfaces;

namespace CulinaryBlog.API.Endpoints;

/// <summary>
/// Minimal API Endpoints xuất bản Sitemap XML (FR-JOB-003, NFR-SEO).
/// </summary>
public static class SitemapEndpoints
{
    public static RouteGroupBuilder MapSitemapEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("")
            .WithTags("SEO");

        // GET /sitemap.xml
        group.MapGet("/sitemap.xml", async (ISitemapService sitemapService, CancellationToken ct) =>
        {
            var baseDir = AppContext.BaseDirectory;
            var sitemapFile = Path.Combine(baseDir, "wwwroot", "sitemap.xml");

            string xmlContent;
            if (File.Exists(sitemapFile))
            {
                xmlContent = await File.ReadAllTextAsync(sitemapFile, Encoding.UTF8, ct);
            }
            else
            {
                xmlContent = await sitemapService.GenerateSitemapXmlAsync(ct);
            }

            return Results.Content(xmlContent, "application/xml", Encoding.UTF8);
        })
        .WithName("GetSitemapXml")
        .WithSummary("Lấy tập tin sitemap.xml phục vụ máy chủ tìm kiếm")
        .Produces(StatusCodes.Status200OK, contentType: "application/xml");

        return group;
    }
}

