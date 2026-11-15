using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using StackExchange.Redis;
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

            // Cùng nguồn cấu hình với Infrastructure.DependencyInjection; không dùng địa chỉ cố định.
            var redisSw = Stopwatch.StartNew();
            var (redisEndpoint, isRedisHealthy) = await CheckRedisAsync(configuration, cancellationToken);
            checks["redis"] = new
            {
                status = isRedisHealthy ? "Healthy" : "Degraded",
                type = "Redis Cache",
                responseTimeMs = redisSw.ElapsedMilliseconds,
                endpoint = redisEndpoint
            };

            var minioSw = Stopwatch.StartNew();
            var minioEndpoint = configuration["Minio:Endpoint"] ?? "http://localhost:9000";
            var validMinioEndpoint = Uri.TryCreate(minioEndpoint, UriKind.Absolute, out var minioUri)
                && (minioUri.Scheme == Uri.UriSchemeHttp || minioUri.Scheme == Uri.UriSchemeHttps);
            var isMinioHealthy = validMinioEndpoint
                && await CanConnectAsync(minioUri!.Host, minioUri.Port, cancellationToken);
            checks["storage"] = new
            {
                status = isMinioHealthy ? "Healthy" : "Degraded",
                type = "MinIO Object Storage",
                responseTimeMs = minioSw.ElapsedMilliseconds,
                endpoint = validMinioEndpoint ? minioUri!.GetLeftPart(UriPartial.Authority) : "Invalid configuration"
            };

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

                var (_, redisReady) = await CheckRedisAsync(configuration, cancellationToken);
                if (!redisReady)
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
        .WithSummary("Readiness Probe kiểm tra PostgreSQL và Redis theo cấu hình ứng dụng")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status503ServiceUnavailable);

        return group;
    }
    private static async Task<(string Endpoint, bool Healthy)> CheckRedisAsync(
        IConfiguration configuration, CancellationToken cancellationToken)
    {
        try
        {
            var connectionString = configuration.GetConnectionString("Redis")
                ?? configuration["Redis:ConnectionString"]
                ?? "localhost:6379";
            var options = ConfigurationOptions.Parse(connectionString);
            var addresses = new List<string>();
            var healthy = false;
            foreach (var endpoint in options.EndPoints)
            {
                var (host, port) = endpoint switch
                {
                    DnsEndPoint dns => (dns.Host, dns.Port),
                    IPEndPoint ip => (ip.Address.ToString(), ip.Port),
                    _ => ("", 0)
                };
                if (string.IsNullOrWhiteSpace(host)) continue;
                if (port == 0) port = options.Ssl ? 6380 : 6379;
                addresses.Add($"{host}:{port}");
                healthy |= await CanConnectAsync(host, port, cancellationToken);
            }
            return (string.Join(",", addresses), healthy);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch { return ("Invalid configuration", false); }
    }

    private static async Task<bool> CanConnectAsync(string host, int port, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(1));
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(host, port, timeout.Token);
            return client.Connected;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch { return false; }
    }

}
