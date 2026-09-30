namespace CulinaryBlog.Infrastructure.Services;

/// <summary>Cấu hình URL của frontend (Next.js) để dựng link trong email gửi người dùng.</summary>
public class ClientAppSettings
{
    public const string SectionName = "ClientApp";

    /// <summary>URL gốc của frontend, ví dụ <c>http://localhost:3000</c>.</summary>
    public string BaseUrl { get; init; } = "http://localhost:3000";
}
