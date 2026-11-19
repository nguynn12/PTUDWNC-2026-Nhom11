using System.Threading.Channels;
using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Infrastructure.Services;

public sealed record QueuedEmailMessage(string To, string Subject, string Body);

/// <summary>Hàng đợi email trong bộ nhớ (singleton). Request chỉ cần đẩy vào, không chờ SMTP.</summary>
public sealed class EmailQueue
{
    private readonly Channel<QueuedEmailMessage> _channel = Channel.CreateUnbounded<QueuedEmailMessage>();

    public ChannelReader<QueuedEmailMessage> Reader => _channel.Reader;

    public void Enqueue(QueuedEmailMessage message) => _channel.Writer.TryWrite(message);
}

/// <summary>
/// <see cref="IEmailService"/> mà handler/AccountEmailSender dùng: chỉ xếp email vào hàng đợi (OPS-08),
/// nên SMTP chậm hay lỗi không làm hỏng request đăng ký / quên mật khẩu.
/// </summary>
public sealed class QueuedEmailService(EmailQueue queue) : IEmailService
{
    public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        queue.Enqueue(new QueuedEmailMessage(to, subject, body));
        return Task.CompletedTask;
    }
}

/// <summary>Lấy email từ hàng đợi và gửi bằng transport thật (SMTP/Mock), thử lại theo backoff khi lỗi.</summary>
public sealed class EmailDispatchWorker(
    EmailQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<EmailDispatchWorker> logger) : BackgroundService
{
    public const string TransportKey = "email-transport";

    /// <summary>Thời gian chờ trước lần thử thứ 2, 3, ... (tổng 4 lần gửi). Cho phép ghi đè trong test.</summary>
    public IReadOnlyList<TimeSpan> RetryDelays { get; init; } =
        [TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30)];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var message in queue.Reader.ReadAllAsync(stoppingToken))
            {
                await SendWithRetryAsync(message, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Dừng ứng dụng.
        }
    }

    public async Task<bool> SendWithRetryAsync(QueuedEmailMessage message, CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var transport = scope.ServiceProvider.GetRequiredKeyedService<IEmailService>(TransportKey);
                await transport.SendEmailAsync(message.To, message.Subject, message.Body, cancellationToken);
                return true;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (attempt >= RetryDelays.Count)
                {
                    logger.LogError(ex, "Gửi email thất bại sau {Attempts} lần thử (Subject: {Subject})",
                        attempt + 1, message.Subject);
                    return false;
                }

                logger.LogWarning(ex, "Gửi email lỗi (lần {Attempt}), thử lại sau {Delay}",
                    attempt + 1, RetryDelays[attempt]);
                await Task.Delay(RetryDelays[attempt], cancellationToken);
            }
        }
    }
}
