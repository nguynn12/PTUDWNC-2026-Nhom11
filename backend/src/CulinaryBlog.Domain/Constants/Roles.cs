namespace CulinaryBlog.Domain.Constants;

/// <summary>
/// Tên các Role lưu trong AspNetRoles. "Guest" KHÔNG phải role trong DB — đó là trạng thái
/// chưa xác thực (anonymous), không có bản ghi. Xem SRS.md mục 2.3 (3 tầng phân quyền:
/// Role-Based, Resource-Based, Policy-Based) và mục 7.7.
/// </summary>
public static class Roles
{
    /// <summary>Gán tự động khi đăng ký (FR-AUTH-001). Tạo/sửa/xoá recipe của chính mình.</summary>
    public const string Author = "Author";

    /// <summary>Gán thủ công qua database seeding. Toàn quyền, bypass resource ownership check.</summary>
    public const string Admin = "Admin";
}
