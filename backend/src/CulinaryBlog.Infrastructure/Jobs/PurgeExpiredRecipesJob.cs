namespace CulinaryBlog.Infrastructure.Jobs;

using CulinaryBlog.Application.Common.Interfaces;
using Hangfire;
using Microsoft.Extensions.Logging;

/// <summary>
/// Hangfire Background Job tự động quét và xóa vĩnh viễn (Purge) các công thức đã xóa mềm quá 30 ngày (FR-JOB-004).
/// Chạy định kỳ vào 03:00 sáng hàng ngày (cron '0 3 * * *').
/// Tái sử dụng IRecipePurgeService dùng chung với thao tác thủ công của Quản trị viên.
/// </summary>
public class PurgeExpiredRecipesJob(
    IRecipePurgeService recipePurgeService,
    ILogger<PurgeExpiredRecipesJob> logger)
{
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [60, 300, 1800])]
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[PurgeExpiredRecipesJob] Bắt đầu quét các công thức đã nằm trong thùng rác quá 30 ngày...");

        try
        {
            var purgedCount = await recipePurgeService.PurgeExpiredRecipesAsync(30, cancellationToken);
            logger.LogInformation("[PurgeExpiredRecipesJob] Hoàn thành. Đã xóa vĩnh viễn {Count} công thức quá hạn 30 ngày", purgedCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[PurgeExpiredRecipesJob] Gặp lỗi nghiêm trọng trong tiến trình dọn rác tự động");
            throw;
        }
    }
}

