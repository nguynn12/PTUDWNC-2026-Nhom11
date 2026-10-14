using Microsoft.AspNetCore.Cors.Infrastructure;

namespace CulinaryBlog.API.Configuration;

/// <summary>
/// CORS cho frontend Next.js (SRS 5.2 "CORS Headers", NFR-SEC "CORS allowlist, không wildcard production").
/// Danh sách origin đọc từ <c>Cors:AllowedOrigins</c> (appsettings hoặc biến môi trường
/// <c>Cors__AllowedOrigins__0</c>, <c>Cors__AllowedOrigins__1</c>...).
/// </summary>
public static class CorsSetup
{
    public const string PolicyName = "Frontend";
    public const string SectionName = "Cors:AllowedOrigins";

    /// <summary>SRS 5.2: GET, POST, PUT, PATCH, DELETE, OPTIONS.</summary>
    public static readonly string[] AllowedMethods = ["GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS"];

    /// <summary>SRS 5.2: Content-Type, Authorization, X-Correlation-ID; thêm If-Match cho FR-RCP-004 (xmin).</summary>
    public static readonly string[] AllowedHeaders = ["Content-Type", "Authorization", "X-Correlation-ID", "If-Match"];

    /// <summary>Header trả về mà JavaScript phía trình duyệt cần đọc được.</summary>
    public static readonly string[] ExposedHeaders =
    [
        "ETag", "Location", "Retry-After", "api-supported-versions", "X-Correlation-ID",
        "X-RateLimit-Limit", "X-RateLimit-Remaining", "X-RateLimit-Reset",
    ];

    public static readonly TimeSpan PreflightMaxAge = TimeSpan.FromMinutes(10);

    public static IServiceCollection AddFrontendCors(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var origins = ResolveOrigins(configuration, environment);
        services.AddCors(options => options.AddPolicy(PolicyName, policy => Configure(policy, origins)));
        return services;
    }

    /// <summary>Đọc và kiểm tra danh sách origin; cấu hình sai thì dừng ứng dụng ngay khi khởi động.</summary>
    public static string[] ResolveOrigins(IConfiguration configuration, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        var origins = (configuration.GetSection(SectionName).Get<string[]>() ?? [])
            .Select(origin => origin.Trim().TrimEnd('/'))
            .Where(origin => origin.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (origins.Length == 0)
        {
            throw new InvalidOperationException(
                $"Chưa cấu hình {SectionName} (vd. \"http://localhost:3000\") nên không thể bật CORS cho frontend.");
        }

        if (origins.Contains("*"))
        {
            if (!environment.IsDevelopment())
            {
                throw new InvalidOperationException(
                    "NFR-SEC: không dùng wildcard \"*\" cho CORS ngoài môi trường Development; hãy liệt kê origin cụ thể.");
            }

            return ["*"];
        }

        foreach (var origin in origins)
        {
            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                || uri.AbsolutePath != "/")
            {
                throw new InvalidOperationException(
                    $"Origin CORS không hợp lệ: \"{origin}\". Dạng đúng: scheme://host[:port], không có đường dẫn.");
            }
        }

        return origins;
    }

    public static void Configure(CorsPolicyBuilder policy, IReadOnlyCollection<string> origins)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(origins);

        if (origins.Contains("*"))
        {
            policy.AllowAnyOrigin();
        }
        else
        {
            policy.WithOrigins([.. origins]);
        }

        // Không AllowCredentials: SRS 5.2 dùng Bearer token trong header và refresh token trong body, không dùng cookie.
        policy.WithMethods(AllowedMethods)
              .WithHeaders(AllowedHeaders)
              .WithExposedHeaders(ExposedHeaders)
              .SetPreflightMaxAge(PreflightMaxAge);
    }
}
