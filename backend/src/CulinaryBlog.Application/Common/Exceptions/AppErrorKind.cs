namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>
/// Nhóm lỗi nghiệp vụ — tầng API dựa vào đây để chọn HTTP status (SRS Phụ lục A).
/// Tầng Application KHÔNG biết tới HTTP, chỉ phân loại lỗi.
/// </summary>
public enum AppErrorKind
{
    /// <summary>Dữ liệu đầu vào không đạt rule (FluentValidation) → 422.</summary>
    Validation,

    /// <summary>Vi phạm quy tắc nghiệp vụ, ví dụ publish thiếu nguyên liệu → 422.</summary>
    BusinessRule,

    /// <summary>Không tìm thấy resource hoặc đã bị xoá mềm → 404.</summary>
    NotFound,

    /// <summary>Trùng dữ liệu unique hoặc xung đột trạng thái → 409.</summary>
    Conflict,

    /// <summary>Thông tin xác thực/token sai hoặc hết hạn → 401.</summary>
    Unauthorized,

    /// <summary>Đã xác thực nhưng không đủ quyền (role, ownership, email chưa xác nhận) → 403.</summary>
    Forbidden,

    /// <summary>Tài khoản đang bị khoá tạm thời (lockout) → 423.</summary>
    Locked,
}
