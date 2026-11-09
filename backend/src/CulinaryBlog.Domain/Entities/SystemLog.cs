namespace CulinaryBlog.Domain.Entities;

public sealed class SystemLog
{
    public long Id { get; set; }
    public string? UserId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string Level { get; set; } = "Information";
    public string? Message { get; set; }
    public string? IpAddress { get; set; }
    public long DurationMs { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
