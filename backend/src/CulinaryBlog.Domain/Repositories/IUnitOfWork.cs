namespace CulinaryBlog.Domain.Repositories;

/// <summary>
/// Unit of Work: gom các repository dùng chung một phiên làm việc với database và ghi mọi thay
/// đổi trong một lần <see cref="SaveChangesAsync"/> (SRS 3.3: mutation đi qua UnitOfWork để bảo
/// đảm nhất quán transaction). Thành viên khác bổ sung repository của module mình vào đây
/// (ví dụ <c>ICategoryRepository Categories</c>, <c>IRecipeRepository Recipes</c>).
/// </summary>
public interface IUnitOfWork
{
    IRefreshTokenRepository RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Mở transaction cho thao tác gồm nhiều lần ghi (ví dụ đăng ký: tạo user + lưu token).</summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Huỷ transaction đang mở; không làm gì nếu chưa mở transaction.</summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
