using Microsoft.AspNetCore.Identity;

namespace CulinaryBlog.Domain.Entities;

/// <summary>
/// Tài khoản người dùng của hệ thống — mở rộng IdentityUser (ASP.NET Core Identity).
/// Đối chiếu SRS.md mục 7.7 (mô hình dữ liệu) và mục 3.1 FR-AUTH-001..010.
/// Khoá chính Id (string, độ dài 450) do Identity sinh tự động — không đổi tuỳ ý.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>Tên hiển thị công khai, 2-100 ký tự — FR-AUTH-006/007.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>URL ảnh đại diện, tối đa 500 ký tự — nullable, phải là URL hợp lệ (validate ở Application layer).</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>Tiểu sử ngắn, tối đa 1000 ký tự — nullable.</summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Cờ kích hoạt tài khoản — Admin vô hiệu hoá chủ động qua FR-AUTH-010.
    /// KHÁC với Identity Lockout (LockoutEnd/AccessFailedCount — tạm khoá tự động do đăng
    /// nhập sai nhiều lần): IsActive = false là hành động CHỦ ĐỘNG của Admin, không tự hết hạn.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Navigation — 1 user có nhiều refresh token (nhiều phiên/thiết bị).</summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    /// <summary>Dùng cho đăng ký thông thường qua email/mật khẩu — FR-AUTH-001.</summary>
    public static ApplicationUser Create(string email, string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        return new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email.Trim(),
            // UserName nội bộ = email (Identity tự normalize) — theo FR-AUTH-001
            UserName = email.Trim(),
            DisplayName = displayName.Trim(),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>Dùng khi đăng nhập lần đầu qua Google OAuth — FR-AUTH-003.</summary>
    public static ApplicationUser CreateFromGoogle(string email, string? displayName, string? avatarUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email.Trim(),
            UserName = email.Trim(),
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? email.Trim() : displayName.Trim(),
            AvatarUrl = avatarUrl,
            EmailConfirmed = true, // Google đã xác minh email thay chúng ta
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>FR-AUTH-010 — chỉ Admin gọi được, không tự vô hiệu hoá chính mình (kiểm tra ở Handler).</summary>
    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    /// <summary>FR-AUTH-007 — Email/UserName KHÔNG đổi qua đây.</summary>
    public void UpdateProfile(string? displayName, string? avatarUrl, string? bio)
    {
        if (!string.IsNullOrWhiteSpace(displayName)) DisplayName = displayName.Trim();
        if (avatarUrl is not null) AvatarUrl = avatarUrl.Trim();
        if (bio is not null) Bio = bio.Trim();
    }
}
