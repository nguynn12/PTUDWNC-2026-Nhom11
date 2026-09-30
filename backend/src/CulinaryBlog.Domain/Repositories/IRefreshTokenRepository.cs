using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Domain.Repositories;

/// <summary>Truy vấn refresh token theo các nhu cầu của FR-AUTH-004/005/010.</summary>
public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    /// <summary>Tìm theo SHA-256 hash (cột unique <c>TokenHash</c>).</summary>
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>Các token CHƯA bị thu hồi trong cùng family — dùng khi phát hiện reuse.</summary>
    Task<IReadOnlyList<RefreshToken>> GetNotRevokedByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken = default);

    /// <summary>Các token CHƯA bị thu hồi của user — dùng khi khoá tài khoản/đặt lại mật khẩu.</summary>
    Task<IReadOnlyList<RefreshToken>> GetNotRevokedByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
