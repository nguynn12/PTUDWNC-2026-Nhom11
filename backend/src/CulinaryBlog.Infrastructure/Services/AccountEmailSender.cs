using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.Infrastructure.Services;

/// <summary>
/// Dựng nội dung email xác nhận (link tới trang frontend kèm <c>userId</c> và <c>token</c> — đúng
/// body của <c>POST /api/v1/auth/email/confirm</c>, FR-AUTH-008) rồi gửi qua <see cref="IEmailService"/>.
/// </summary>
public sealed class AccountEmailSender(IEmailService emailService, IOptions<ClientAppSettings> clientApp)
    : IAccountEmailSender
{
    public const string ConfirmEmailPath = "/auth/confirm-email";

    public Task SendEmailConfirmationAsync(
        string userId,
        string email,
        string displayName,
        string token,
        CancellationToken cancellationToken = default)
    {
        var link = BuildConfirmationLink(clientApp.Value.BaseUrl, userId, token);

        var body =
            $"Xin chào {displayName},\n\n" +
            "Cảm ơn bạn đã đăng ký Culinary Blog. Vui lòng bấm vào liên kết sau để xác nhận email:\n" +
            $"{link}\n\n" +
            "Nếu bạn không đăng ký tài khoản, hãy bỏ qua email này.";

        return emailService.SendEmailAsync(email, "Xác nhận email Culinary Blog", body);
    }

    public static string BuildConfirmationLink(string baseUrl, string userId, string token) =>
        $"{baseUrl.TrimEnd('/')}{ConfirmEmailPath}" +
        $"?userId={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}";
}
