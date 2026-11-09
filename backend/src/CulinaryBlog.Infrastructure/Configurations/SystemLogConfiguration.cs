using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CulinaryBlog.Infrastructure.Configurations;

public sealed class SystemLogConfiguration : IEntityTypeConfiguration<SystemLog>
{
    public void Configure(EntityTypeBuilder<SystemLog> builder)
    {
        builder.ToTable("SystemLogs");
        builder.HasKey(log => log.Id);
        builder.Property(log => log.Method).HasMaxLength(16).IsRequired();
        builder.Property(log => log.Path).HasMaxLength(500).IsRequired();
        builder.Property(log => log.Level).HasMaxLength(32).IsRequired();
        builder.Property(log => log.Message).HasMaxLength(2000);
        builder.Property(log => log.IpAddress).HasMaxLength(64);
        builder.HasIndex(log => log.CreatedAt);
        builder.HasIndex(log => new { log.StatusCode, log.CreatedAt });
        builder.HasIndex(log => log.UserId);
    }
}
