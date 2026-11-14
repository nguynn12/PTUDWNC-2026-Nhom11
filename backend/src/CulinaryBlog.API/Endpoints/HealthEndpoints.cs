using System.Diagnostics;
using System.Net.Sockets;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CulinaryBlog.API.Endpoints;

/// <summary>
/// Định nghĩa các Minimal API Endpoints cho phân hệ Giám sát sức khỏe Hệ thống (FR-OBS-001).
/// </summary>
public static class HealthEndpoints
{
    public static RouteGroupBuilder MapHealthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/health")
            .WithTags("Health");

        // FR-OBS-001: Tổng hợp tình trạng sức khỏe các phụ thuộc (PostgreSQL, Redis, MinIO)
        group.MapGet("", async (
            CulinaryBlogDbContext dbContext,
            IConfiguration configuration,
            CancellationToken cancellationToken) =>
        {
            var totalSw = Stopwatch.StartNew();
            var checks = new Dictionary<string, object>();
            var isDbHealthy = false;

            // 1. Kiểm tra PostgreSQL Database
            var dbSw = Stopwatch.StartNew();
            try
            {
                isDbHealthy = await dbContext.Database.CanConnectAsync(cancellationToken);
                dbSw.Stop();
                checks["database"] = new
                {
                    status = isDbHealthy ? "Healthy" : "Unhealthy",
                    type = "PostgreSQL",
                    responseTimeMs = dbSw.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                dbSw.Stop();
                checks["database"] = new
                {
                    status = "Unhealthy",
                    type = "PostgreSQL",
                    responseTimeMs = dbSw.ElapsedMilliseconds,
                    error = ex.Message
                };
            }

            // 2. Kiểm tra Redis Cache (TCP Socket check cổng 6379 hoặc connection string)
            var redisSw = Stopwatch.StartNew();
            var (redisHost, redisPort) = ResolveRedisEndpoint(configuration);
            var isRedisHealthy = false;
            try
            {
                using var tcpClient = new TcpClient();
                var connectTask = tcpClient.ConnectAsync(redisHost, redisPort);
                var completed = await Task.WhenAny(connectTask, Task.Delay(1000, cancellationToken));
                if (completed == connectTask && tcpClient.Connected)
                {
                    isRedisHealthy = true;
                }
                redisSw.Stop();
                checks["redis"] = new
                {
                    status = isRedisHealthy ? "Healthy" : "Degraded",
                    type = "Redis Cache",
                    responseTimeMs = redisSw.ElapsedMilliseconds,
                    endpoint = $"{redisHost}:{redisPort}"
                };
            }
            catch
            {
                redisSw.Stop();
                checks["redis"] = new
                {
                    status = "Degraded",
                    type = "Redis Cache",
                    responseTimeMs = redisSw.ElapsedMilliseconds,
                    endpoint = $"{redisHost}:{redisPort}"
                };
            }

            // 3. Kiểm tra MinIO Storage (TCP Socket check cổng 9000)
            var minioSw = Stopwatch.StartNew();
            var (minioHost, minioPort) = ResolveMinioEndpoint(configuration);
            var isMinioHealthy = false;
            try
            {
                using var minioClient = new TcpClient();
                var connectTask = minioClient.ConnectAsync(minioHost, minioPort);
                var completed = await Task.WhenAny(connectTask, Task.Delay(1000, cancellationToken));
                if (completed == connectTask && minioClient.Connected)
                {
                    isMinioHealthy = true;
                }
                minioSw.Stop();
                checks["storage"] = new
                {
                    status = isMinioHealthy ? "Healthy" : "Degraded",
                    type = "MinIO Object Storage",
                    responseTimeMs = minioSw.ElapsedMilliseconds,
                    endpoint = $"{minioHost}:{minioPort}"
                };
            }
            catch
            {
                minioSw.Stop();
                checks["storage"] = new
                {
                    status = "Degraded",
                    type = "MinIO Object Storage",
                    responseTimeMs = minioSw.ElapsedMilliseconds,
                    endpoint = $"{minioHost}:{minioPort}"
                };
            }

            totalSw.Stop();

            var overallStatus = isDbHealthy
                ? (isRedisHealthy && isMinioHealthy ? "Healthy" : "Degraded")
                : "Unhealthy";

            var response = new
            {
                status = overallStatus,
                totalResponseTimeMs = totalSw.ElapsedMilliseconds,
                timestamp = DateTimeOffset.UtcNow,
                checks
            };

            return isDbHealthy
                ? Results.Ok(response)
                : Results.Json(response, statusCode: StatusCodes.Status503ServiceUnavailable);
        })
        .WithName("GetHealthCheck")
        .WithSummary("Tổng hợp tình trạng sức khỏe các phụ thuộc hệ thống (FR-OBS-001)")
        .WithDescription("Kiểm tra đồng thời kết nối cơ sở dữ liệu PostgreSQL, Redis Cache và MinIO Storage.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status503ServiceUnavailable);

        // FR-OBS-001: Liveness Probe (chỉ kiểm tra process backend .NET còn sống)
        group.MapGet("/live", () => Results.Ok(new
        {
            status = "Healthy",
            service = "CulinaryBlog.API",
            timestamp = DateTimeOffset.UtcNow
        }))
        .WithName("GetLivenessProbe")
        .WithSummary("Liveness Probe kiểm tra tiến trình ứng dụng còn sống (FR-OBS-001)")
        .WithDescription("Trả về HTTP 200 ngay lập tức để container orchestrator xác nhận tiến trình không bị treo.")
        .Produces(StatusCodes.Status200OK);

        group.MapGet("/ready", async (
            CulinaryBlogDbContext dbContext,
            IConfiguration configuration,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var databaseReady = await dbContext.Database.CanConnectAsync(cancellationToken);
                if (!databaseReady)
                    return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);

                var (redisHost, redisPort) = ResolveRedisEndpoint(configuration);
                using var redis = new TcpClient();
                var connection = redis.ConnectAsync(redisHost, redisPort);
                var completed = await Task.WhenAny(connection, Task.Delay(1000, cancellationToken));
                if (completed != connection || !redis.Connected)
                    return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);

                return Results.Ok(new
                {
                    status = "Ready",
                    timestamp = DateTimeOffset.UtcNow,
                    checks = new { database = "Healthy", redis = "Healthy" }
                });
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        })
        .WithName("GetReadinessProbe")
        .WithSummary("Readiness Probe kiểm tra kết nối cơ sở dữ liệu")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status503ServiceUnavailable);

        return group;
    }

    /// <summary>
    /// Redis lấy từ cùng cấu hình mà ứng dụng dùng để kết nối (<c>ConnectionStrings:Redis</c>, dạng
    /// "host:port,option=..."), sau đó mới tới <c>Redis:Host</c>/<c>Redis:Port</c>. Trong Docker đó là tên dịch vụ (redis).
    /// </summary>
    public static (string Host, int Port) ResolveRedisEndpoint(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis") ?? configuration["Redis:ConnectionString"];
        var first = connectionString?.Split(',')[0].Trim();
        if (!string.IsNullOrWhiteSpace(first) && !first.Contains('='))
        {
            var separator = first.LastIndexOf(':');
            if (separator > 0 && int.TryParse(first[(separator + 1)..], out var port))
                return (first[..separator], port);
            return (first, 6379);
        }

        var host = configuration["Redis:Host"];
        var hasPort = int.TryParse(configuration["Redis:Port"], out var configuredPort);
        return (string.IsNullOrWhiteSpace(host) ? "localhost" : host, hasPort ? configuredPort : 6379);
    }

    /// <summary>MinIO lấy từ <c>Minio:Endpoint</c> (URL), không hard-code localhost:9000.</summary>
    public static (string Host, int Port) ResolveMinioEndpoint(IConfiguration configuration)
    {
        if (Uri.TryCreate(configuration["Minio:Endpoint"], UriKind.Absolute, out var uri))
            return (uri.Host, uri.Port);
        return ("localhost", 9000);
    }
}
