using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CulinaryBlog.UnitTests.Auth;

/// <summary>OPS-08: email đi qua hàng đợi, lỗi SMTP được thử lại và không làm hỏng request.</summary>
public class EmailDispatchWorkerTests
{
    private sealed class FlakyTransport(int failTimes) : IEmailService
    {
        public int Calls;
        public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        {
            Calls++;
            if (Calls <= failTimes) throw new InvalidOperationException("SMTP down");
            return Task.CompletedTask;
        }
    }

    private static (EmailDispatchWorker Worker, FlakyTransport Transport) Create(int failTimes)
    {
        var transport = new FlakyTransport(failTimes);
        var services = new ServiceCollection();
        services.AddKeyedSingleton<IEmailService>(EmailDispatchWorker.TransportKey, transport);
        var worker = new EmailDispatchWorker(
            new EmailQueue(),
            services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>(),
            NullLogger<EmailDispatchWorker>.Instance)
        {
            RetryDelays = [TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero],
        };
        return (worker, transport);
    }

    [Fact]
    public async Task SendWithRetry_LoiTamThoi_ThuLaiRoiThanhCong()
    {
        var (worker, transport) = Create(failTimes: 2);

        var ok = await worker.SendWithRetryAsync(new QueuedEmailMessage("a@b.c", "s", "b"), CancellationToken.None);

        Assert.True(ok);
        Assert.Equal(3, transport.Calls);
    }

    [Fact]
    public async Task SendWithRetry_LoiMai_DungSauBonLanVaKhongNemLoi()
    {
        var (worker, transport) = Create(failTimes: int.MaxValue);

        var ok = await worker.SendWithRetryAsync(new QueuedEmailMessage("a@b.c", "s", "b"), CancellationToken.None);

        Assert.False(ok);
        Assert.Equal(4, transport.Calls);
    }

    [Fact]
    public async Task QueuedEmailService_ChiXepHangDoi_KhongGoiTransport()
    {
        var queue = new EmailQueue();
        await new QueuedEmailService(queue).SendEmailAsync("a@b.c", "s", "b");

        Assert.True(queue.Reader.TryRead(out var message));
        Assert.Equal("a@b.c", message!.To);
    }
}
