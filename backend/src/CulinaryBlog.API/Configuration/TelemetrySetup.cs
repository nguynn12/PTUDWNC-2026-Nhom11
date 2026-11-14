using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CulinaryBlog.API.Configuration;

/// <summary>
/// OPS-07: OpenTelemetry (trace; metric chỉ thu trong tiến trình) cho request vào API và HTTP gọi ra ngoài (Google, MinIO...).
/// Chỉ xuất dữ liệu khi cấu hình <c>OpenTelemetry:OtlpEndpoint</c> (vd. Seq nhận OTLP tại
/// <c>http://seq:80/ingest/otlp</c>); bỏ trống thì vẫn thu thập trong tiến trình nhưng không gửi đi đâu.
/// </summary>
public static class TelemetrySetup
{
    public const string ServiceName = "CulinaryBlog.API";

    public static IServiceCollection AddCulinaryTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var otlp = configuration["OpenTelemetry:OtlpEndpoint"];
        var enabled = !string.IsNullOrWhiteSpace(otlp) && Uri.TryCreate(otlp, UriKind.Absolute, out _);

        var builder = services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(ServiceName))
            .WithTracing(t =>
            {
                t.AddAspNetCoreInstrumentation(o =>
                    // Không theo dõi health check để khỏi làm nhiễu trace.
                    o.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health"))
                 .AddHttpClientInstrumentation();
                if (enabled)
                {
                    t.AddOtlpExporter(o =>
                    {
                        o.Endpoint = new Uri(otlp!.TrimEnd('/') + "/v1/traces");
                        o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                    });
                }
            })
            // Metric thu trong tiến trình; Seq không nhận OTLP metric nên không xuất ra Seq (tránh lỗi 404 liên tục).
            .WithMetrics(m => m.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation());

        _ = builder;
        return services;
    }
}
