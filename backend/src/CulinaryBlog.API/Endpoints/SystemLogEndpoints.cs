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
            [AsParameters] GetSystemLogsRequest request,
            CulinaryBlogDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var page = Math.Clamp(request.Page, 1, 10_000);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);
            var query = dbContext.SystemLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Level))
                query = query.Where(log => log.Level == request.Level);
            if (request.StatusCode is not null)
                query = query.Where(log => log.StatusCode == request.StatusCode);
            if (request.From is not null)
                query = query.Where(log => log.CreatedAt >= request.From);
            if (request.To is not null)
                query = query.Where(log => log.CreatedAt < request.To.Value.AddDays(1));
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(log => log.Path.Contains(search) || (log.Message != null && log.Message.Contains(search)));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var logs = await query
                .OrderByDescending(log => log.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(log => new SystemLogDto(
                    log.Id, log.UserId, log.Method, log.Path, log.StatusCode,
                    log.Level, log.Message, log.IpAddress, log.DurationMs, log.CreatedAt))
                .ToListAsync(cancellationToken);

            return Results.Ok(new ApiResponse<IReadOnlyList<SystemLogDto>>(
                logs,
                new { page, pageSize, totalCount, totalPages = (int)Math.Ceiling(totalCount / (double)pageSize) }));
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
