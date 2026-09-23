namespace CulinaryBlog.Domain.Common;

/// <summary>
/// Lớp cơ sở cho tất cả các Entity nội dung trong hệ thống (theo mục 7.1 tài liệu SRS v1.2.0).
/// Cung cấp các thuộc tính chuẩn: khóa chính UUID, thời gian tạo/cập nhật,
/// cờ và thời gian Soft Delete.
/// Concurrency token được quản lý qua PostgreSQL xmin ở tầng Infrastructure.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Khóa chính dạng UUID v4, tự động sinh khi khởi tạo.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Thời điểm tạo bản ghi (UTC). timestamptz not null.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Thời điểm cập nhật bản ghi lần cuối (UTC). timestamptz null.
    /// Ban đầu khi tạo mới có giá trị null, chỉ được cập nhật khi có thay đổi.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Cờ Soft Delete. boolean not null default false.
    /// Khi true, bản ghi bị ẩn khỏi các truy vấn thông qua Global Query Filter.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Thời điểm bị soft-delete (UTC). timestamptz null.
    /// Phục vụ nghiệp vụ khôi phục trong vòng 30 ngày hoặc dọn dẹp (purge).
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }
}
