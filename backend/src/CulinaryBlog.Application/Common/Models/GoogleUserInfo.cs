namespace CulinaryBlog.Application.Common.Models;

/// <summary>Thông tin người dùng lấy từ Google ID token đã được xác minh (FR-AUTH-003).</summary>
/// <param name="Subject">Claim <c>sub</c> — mã định danh Google cố định của tài khoản (provider key).</param>
/// <param name="Email">Email của tài khoản Google.</param>
/// <param name="EmailVerified">Claim <c>email_verified</c> — Google đã xác minh email hay chưa.</param>
/// <param name="Name">Tên hiển thị (claim <c>name</c>), có thể null.</param>
/// <param name="PictureUrl">Ảnh đại diện (claim <c>picture</c>), có thể null.</param>
public sealed record GoogleUserInfo(
    string Subject,
    string Email,
    bool EmailVerified,
    string? Name,
    string? PictureUrl);

/// <summary>Kết quả tìm/liên kết/tạo tài khoản từ Google.</summary>
public enum GoogleLoginStatus
{
    /// <summary>Đăng nhập được (user đã liên kết, vừa liên kết hoặc vừa tạo mới).</summary>
    Success,

    /// <summary>Email đã có tài khoản nhưng Google chưa xác minh email → không tự liên kết.</summary>
    EmailNotVerified,
}

public sealed record GoogleLoginResult(GoogleLoginStatus Status, UserAccount? User = null);
