using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Infrastructure.Jobs;

/// <summary>
/// Background Job định kỳ sinh Sitemap XML (FR-JOB-003).
/// Được cấu hình chạy lúc 02:00 UTC hằng ngày ("0 2 * * *").
/// Có cơ chế retry 2 lần nếu gặp sự cố.
/// </summary>
public class GenerateSitemapJob
{
    private readonly ISitemapService _sitemapService;
    private readonly ILogger<GenerateSitemapJob> _logger;

    public GenerateSitemapJob(
        ISitemapService sitemapService,
        ILogger<GenerateSitemapJob> logger)
    {
        _sitemapService = sitemapService;
        _logger = logger;
    }

    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[FR-JOB-003] Bắt đầu thực thi GenerateSitemapJob (02:00 UTC)...");
        var urlCount = await _sitemapService.ExecuteSitemapGenerationJobAsync(cancellationToken);
        _logger.LogInformation("[FR-JOB-003] Kết thúc GenerateSitemapJob. Đã sinh {UrlCount} URL hợp lệ.", urlCount);
        return urlCount;
    }
}

