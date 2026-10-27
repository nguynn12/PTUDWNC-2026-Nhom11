
using CulinaryBlog.Application.Common.Interfaces;

namespace CulinaryBlog.Application.Auth.Shared;

/// <summary>Thu hồi mọi refresh token chưa bị thu hồi của một user (đăng xuất khỏi mọi phiên).</summary>
public static class RefreshTokenRevoker
{
    /// <summary>
    /// Đánh dấu revoke mọi token chưa thu hồi của user. KHÔNG lưu DB — caller gọi
    /// <see cref="IUnitOfWork.SaveChangesAsync"/> để gộp chung một lần ghi.
    /// </summary>
    /// <returns>Số token đã thu hồi.</returns>
    public static async Task<int> RevokeAllAsync(
        IRefreshTokenRepository repository,
        string userId,
        string reason,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var tokens = await repository.GetNotRevokedByUserIdAsync(userId, cancellationToken);
        foreach (var token in tokens)
        {
            token.Revoke(reason);
        }

        return tokens.Count;
    }
}
