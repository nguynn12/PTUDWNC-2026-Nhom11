using CulinaryBlog.API.Common.Models;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.API.Endpoints;

public static class SystemLogEndpoints
{
    public static void MapSystemLogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin/system-logs")
            .WithTags("Admin System Logs")
            .RequireAuthorization("Admin");

        group.MapGet("", async (
            int? page,
            int? pageSize,
            string? search,
            string? level,
            int? statusCode,
            DateTimeOffset? from,
            DateTimeOffset? to,
            CulinaryBlogDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var p = Math.Clamp(page ?? 1, 1, 10_000);
            var ps = Math.Clamp(pageSize ?? 25, 1, 100);
            var query = dbContext.SystemLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(level))
                query = query.Where(log => log.Level == level);
            if (statusCode is not null)
                query = query.Where(log => log.StatusCode == statusCode);
            if (from is not null)
                query = query.Where(log => log.CreatedAt >= from);
            if (to is not null)
                query = query.Where(log => log.CreatedAt < to.Value.AddDays(1));
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(log => log.Path.Contains(s) || (log.Message != null && log.Message.Contains(s)));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var logs = await query
                .OrderByDescending(log => log.CreatedAt)
                .Skip((p - 1) * ps)
                .Take(ps)
                .Select(log => new SystemLogDto(
                    log.Id, log.UserId, log.Method, log.Path, log.StatusCode,
                    log.Level, log.Message, log.IpAddress, log.DurationMs, log.CreatedAt))
                .ToListAsync(cancellationToken);

            return Results.Ok(new ApiResponse<IReadOnlyList<SystemLogDto>>(
                logs,
                new { page = p, pageSize = ps, totalCount, totalPages = (int)Math.Ceiling(totalCount / (double)ps) }));
        });
    }
}

public sealed class GetSystemLogsRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? Search { get; set; }
    public string? Level { get; set; }
    public int? StatusCode { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
}

public sealed record SystemLogDto(
    long Id,
    string? UserId,
    string Method,
    string Path,
    int StatusCode,
    string Level,
    string? Message,
    string? IpAddress,
    long DurationMs,
    DateTimeOffset CreatedAt);
