namespace CulinaryBlog.Domain.Entities;

/// <summary>
/// Refresh token — hiện thực Token Rotation với phát hiện reuse theo FamilyId.
/// Đối chiếu SRS.md mục 7.8 và FR-AUTH-002/004/005/010.
/// QUAN TRỌNG: raw token (32 random bytes, Base64URL) chỉ tồn tại phía client;
/// server CHỈ LƯU SHA-256 hash (TokenHash) — không bao giờ lưu raw token vào DB.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; private set; }

    /// <summary>
    /// FK tới AspNetUsers.Id (varchar(450)). Cố ý KHÔNG có navigation User — quan hệ được
    /// cấu hình ở Infrastructure (ApplicationUserConfiguration), xem RESOLVED-CONFLICTS.md mục D7.
    /// </summary>
    public string UserId { get; private set; } = string.Empty;

    /// <summary>
    /// Nhóm token cùng một "dòng" rotation, bắt đầu từ token đầu tiên của phiên đăng nhập.
    /// Khi phát hiện reuse (token đã revoked bị dùng lại), TOÀN BỘ token cùng FamilyId
    /// bị revoke — chống token theft (FR-AUTH-004).
    /// </summary>
    public Guid FamilyId { get; private set; }

    /// <summary>SHA-256 hex digest (64 ký tự) của raw refresh token — duy nhất toàn hệ thống.</summary>
    public string TokenHash { get; private set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>Hash của token MỚI đã thay thế token này — audit trail cho chuỗi rotation.</summary>
    public string? ReplacedByTokenHash { get; private set; }

    /// <summary>"rotated" | "logout" | "reuse-detected" | "admin-deactivated".</summary>
    public string? RevocationReason { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public string? CreatedByIp { get; private set; }

    public string? RevokedByIp { get; private set; }

    // ── Cờ suy ra — KHÔNG map thành cột DB (SRS 7.8: "IsRevoked là giá trị suy ra") ──
    public bool IsRevoked => RevokedAt is not null;
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken() { } // EF Core

    /// <summary>Bắt đầu một family MỚI (lần login đầu tiên của phiên) — FamilyId = Id của chính token này.</summary>
    public static RefreshToken CreateNewFamily(
        string userId, string tokenHash, int expiryDays, string? createdByIp = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        var id = Guid.NewGuid();
        return new RefreshToken
        {
            Id = id,
            UserId = userId,
            FamilyId = id,
            TokenHash = tokenHash,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(expiryDays),
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByIp = createdByIp,
        };
    }

    /// <summary>Token kế tiếp trong CÙNG family — dùng khi rotation (FR-AUTH-004).</summary>
    public static RefreshToken CreateRotated(
        string userId, string tokenHash, Guid familyId, int expiryDays, string? createdByIp = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FamilyId = familyId,
            TokenHash = tokenHash,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(expiryDays),
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByIp = createdByIp,
        };
    }

    /// <summary>Idempotent — gọi nhiều lần không lỗi (FR-AUTH-005: logout luôn trả 204).</summary>
    public void Revoke(string reason, string? revokedByIp = null, string? replacedByTokenHash = null)
    {
        if (IsRevoked) return;

        RevokedAt = DateTimeOffset.UtcNow;
        RevocationReason = reason;
        RevokedByIp = revokedByIp;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}
