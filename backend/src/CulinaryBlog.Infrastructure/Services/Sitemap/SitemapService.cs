using System.Globalization;
using System.Text;
using System.Xml.Linq;
using CulinaryBlog.Application.Common.Constants;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace CulinaryBlog.Infrastructure.Services.Sitemap;

/// <summary>
/// Dịch vụ sinh Sitemap XML định kỳ với khóa phân tán RedLock (FR-JOB-003, NFR-SEO).
/// </summary>
public class SitemapService : ISitemapService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IConnectionMultiplexer? _connectionMultiplexer;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SitemapService> _logger;

    private static readonly XNamespace Ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

    public SitemapService(
        IApplicationDbContext dbContext,
        IConfiguration configuration,
        ILogger<SitemapService> logger,
        IConnectionMultiplexer? connectionMultiplexer = null)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<string> GenerateSitemapXmlAsync(CancellationToken cancellationToken = default)
    {
        var baseUrl = _configuration["ClientApp:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:3000";

        // 1. Trang tĩnh (Priority 1.0 và 0.6)
        var urls = new List<XElement>
        {
            CreateUrlElement($"{baseUrl}/", DateTimeOffset.UtcNow, "daily", "1.0"),
            CreateUrlElement($"{baseUrl}/recipes", DateTimeOffset.UtcNow, "hourly", "0.9"),
            CreateUrlElement($"{baseUrl}/categories", DateTimeOffset.UtcNow, "weekly", "0.8"),
            CreateUrlElement($"{baseUrl}/search", DateTimeOffset.UtcNow, "daily", "0.7")
        };

        // 2. Danh mục món ăn (chưa bị xóa mềm)
        var categories = await _dbContext.Categories
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.OrderIndex)
            .Select(c => new { c.Slug, LastMod = c.UpdatedAt ?? c.CreatedAt })
            .ToListAsync(cancellationToken);

        foreach (var category in categories)
        {
            urls.Add(CreateUrlElement(
                $"{baseUrl}/categories/{category.Slug}",
                category.LastMod,
                "weekly",
                "0.8"));
        }

        // 3. Công thức nấu ăn (CHỈ LẤY Published và chưa bị xóa mềm, LOẠI BỎ Draft/Archived/Deleted theo NFR-SEO)
        var recipes = await _dbContext.Recipes
            .AsNoTracking()
            .Where(r => !r.IsDeleted && r.Status == RecipeStatus.Published)
            .OrderByDescending(r => r.PublishedAt)
            .Select(r => new { r.Slug, LastMod = r.UpdatedAt ?? r.PublishedAt ?? r.CreatedAt })
            .ToListAsync(cancellationToken);

        foreach (var recipe in recipes)
        {
            urls.Add(CreateUrlElement(
                $"{baseUrl}/recipes/{recipe.Slug}",
                recipe.LastMod,
                "weekly",
                "0.9"));
        }

        var urlset = new XElement(Ns + "urlset", urls);
        var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), urlset);

        var sb = new StringBuilder();
        using (var writer = new Utf8StringWriter(sb))
        {
            doc.Save(writer);
        }

        return sb.ToString();
    }

    public async Task<int> ExecuteSitemapGenerationJobAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[FR-JOB-003] Bắt đầu tác vụ sinh Sitemap XML định kỳ.");

        // Nếu có Redis multiplexer, áp dụng khóa phân tán RedLock để chỉ duy nhất 1 instance chạy
        if (_connectionMultiplexer != null && _connectionMultiplexer.IsConnected)
        {
            var multiplexers = new List<RedLockMultiplexer> { new(_connectionMultiplexer) };
            using var redlockFactory = RedLockFactory.Create(multiplexers);

            await using var redLock = await redlockFactory.CreateLockAsync(
                resource: CacheConstants.SitemapLockKey,
                expiryTime: TimeSpan.FromMinutes(10),
                waitTime: TimeSpan.FromSeconds(5),
                retryTime: TimeSpan.FromSeconds(1),
                cancellationToken: cancellationToken);

            if (!redLock.IsAcquired)
            {
                _logger.LogWarning("[FR-JOB-003] Không thể lấy distributed lock '{Key}'. Có thể instance khác đang thực thi sitemap job. Bỏ qua.",
                    CacheConstants.SitemapLockKey);
                return 0;
            }

            return await PerformSitemapGenerationAndSaveAsync(cancellationToken);
        }
        else
        {
            _logger.LogInformation("[FR-JOB-003] Redis không khả dụng hoặc chưa kết nối. Thực thi sitemap job không dùng distributed lock.");
            return await PerformSitemapGenerationAndSaveAsync(cancellationToken);
        }
    }

    private async Task<int> PerformSitemapGenerationAndSaveAsync(CancellationToken cancellationToken)
    {
        var sitemapXml = await GenerateSitemapXmlAsync(cancellationToken);

        // Đếm số lượng <url>
        var doc = XDocument.Parse(sitemapXml);
        var urlCount = doc.Descendants(Ns + "url").Count();

        // Ghi file vào thư mục wwwroot
        var baseDir = AppContext.BaseDirectory;
        var wwwrootDir = Path.Combine(baseDir, "wwwroot");
        if (!Directory.Exists(wwwrootDir))
        {
            Directory.CreateDirectory(wwwrootDir);
        }

        var sitemapPath = Path.Combine(wwwrootDir, "sitemap.xml");
        await File.WriteAllTextAsync(sitemapPath, sitemapXml, Encoding.UTF8, cancellationToken);

        _logger.LogInformation("[FR-JOB-003] Sinh sitemap.xml thành công tại {Path} với tổng cộng {UrlCount} URLs.",
            sitemapPath, urlCount);

        // Google đã chính thức ngừng nhận ping sitemap, ghi log khai báo sẵn sàng tại robots.txt (SRS 8.1 / buoi8.md)
        var baseUrl = _configuration["ClientApp:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:3000";
        _logger.LogInformation("[FR-JOB-003] Sitemap sẵn sàng tại: {BaseUrl}/sitemap.xml (Khai báo trong robots.txt)", baseUrl);

        return urlCount;
    }

    private static XElement CreateUrlElement(string loc, DateTimeOffset lastMod, string changeFreq, string priority)
    {
        return new XElement(Ns + "url",
            new XElement(Ns + "loc", loc),
            new XElement(Ns + "lastmod", lastMod.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            new XElement(Ns + "changefreq", changeFreq),
            new XElement(Ns + "priority", priority)
        );
    }

    private sealed class Utf8StringWriter : StringWriter
    {
        public Utf8StringWriter(StringBuilder sb) : base(sb) { }
        public override Encoding Encoding => Encoding.UTF8;
    }
}

