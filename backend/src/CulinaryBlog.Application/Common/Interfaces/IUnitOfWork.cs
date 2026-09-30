namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>
/// Gom nhiều thay đổi (thêm/sửa/xóa qua IRepository&lt;T&gt;) thành 1 giao dịch,
/// chỉ thực sự ghi xuống database khi gọi SaveChangesAsync.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
