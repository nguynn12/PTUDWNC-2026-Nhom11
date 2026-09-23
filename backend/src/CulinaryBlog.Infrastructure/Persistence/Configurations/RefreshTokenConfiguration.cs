using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CulinaryBlog.Infrastructure.Persistence.Configurations;

/// <summary>Cấu hình bảng RefreshTokens theo SRS.md mục 7.8.</summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.UserId)
            .IsRequired()
            .HasMaxLength(450); // Khớp độ dài khoá chính AspNetUsers.Id

        builder.Property(rt => rt.FamilyId)
            .IsRequired();

        builder.Property(rt => rt.TokenHash)
            .IsRequired()
            .HasMaxLength(64) // SHA-256 hex digest = 64 ký tự
            .IsFixedLength();

        builder.Property(rt => rt.ExpiresAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(rt => rt.RevokedAt)
            .HasColumnType("timestamptz");

        builder.Property(rt => rt.ReplacedByTokenHash)
            .HasMaxLength(64)
            .IsFixedLength();

        builder.Property(rt => rt.RevocationReason)
            .HasColumnName("ReasonRevoked")
            .HasMaxLength(250);

        builder.Property(rt => rt.CreatedAt)
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(rt => rt.CreatedByIp)
            .IsRequired()
            .HasMaxLength(45); // Đủ cho IPv6

        builder.Property(rt => rt.RevokedByIp)
            .HasMaxLength(45);

        // Các cờ suy ra (IsRevoked/IsExpired/IsActive) — KHÔNG map vào DB
        builder.Ignore(rt => rt.IsRevoked);
        builder.Ignore(rt => rt.IsExpired);
        builder.Ignore(rt => rt.IsActive);

        // ── Indexes ──────────────────────────────────────────────────────────────
        builder.HasIndex(rt => rt.TokenHash).IsUnique();
        builder.HasIndex(rt => rt.FamilyId);              // Revoke cả family khi phát hiện reuse
        builder.HasIndex(rt => rt.UserId);                 // Revoke toàn bộ token của user (FR-AUTH-010)
        builder.HasIndex(rt => new { rt.UserId, rt.RevokedAt }); // Lọc token còn hiệu lực của 1 user

        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
