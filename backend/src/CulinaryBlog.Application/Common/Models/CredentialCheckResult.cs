namespace CulinaryBlog.Application.Common.Models;

/// <summary>Kết quả kiểm tra email/mật khẩu khi đăng nhập (SRS FR-AUTH-002).</summary>
public enum CredentialCheckStatus
{
    /// <summary>Đúng email/mật khẩu, tài khoản đang hoạt động.</summary>
    Success,

    /// <summary>Email không tồn tại hoặc sai mật khẩu (không phân biệt — tránh dò tài khoản).</summary>
    InvalidCredentials,

    /// <summary>Tài khoản đang bị khoá tạm do nhập sai quá số lần cho phép → 423.</summary>
    LockedOut,

    /// <summary>Mật khẩu đúng nhưng tài khoản đã bị Admin vô hiệu hoá (FR-AUTH-010) → 403.</summary>
    Disabled,
}

public sealed record CredentialCheckResult(
    CredentialCheckStatus Status,
    UserAccount? User = null,
    DateTimeOffset? LockoutEnd = null);
