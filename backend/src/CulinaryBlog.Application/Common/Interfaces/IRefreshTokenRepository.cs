using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>
/// Repository riêng cho RefreshToken (module Auth): mở rộng <see cref="IRepository{T}"/> dùng chung
/// của nhóm với các truy vấn cần cho FR-AUTH-004/005/010. Ghi xuống DB qua
/// <see cref="IUnitOfWork.SaveChangesAsync"/>.
/// </summary>
public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    /// <summary>Tìm theo SHA-256 hash (cột unique <c>TokenHash</c>).</summary>
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>Các token CHƯA bị thu hồi trong cùng family — dùng khi phát hiện reuse.</summary>
    Task<List<RefreshToken>> GetNotRevokedByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken = default);

    /// <summary>Các token CHƯA bị thu hồi của user — dùng khi khoá tài khoản/đặt lại mật khẩu.</summary>
    Task<List<RefreshToken>> GetNotRevokedByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
