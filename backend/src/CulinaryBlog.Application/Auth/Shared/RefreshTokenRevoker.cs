using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Application.Auth.Shared;

/// <summary>Thu hồi mọi refresh token còn hiệu lực của một user (đăng xuất khỏi mọi phiên).</summary>
public static class RefreshTokenRevoker
{
    /// <summary>
    /// Đánh dấu revoke mọi token chưa bị thu hồi của user. KHÔNG gọi SaveChanges — caller tự lưu
    /// để gộp chung một lần ghi với các thay đổi khác.
    /// </summary>
    /// <returns>Số token đã thu hồi.</returns>
    public static async Task<int> RevokeAllActiveAsync(
        IApplicationDbContext context,
        string userId,
        string reason,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        var tokens = await context.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revoke(reason);
        }

        return tokens.Count;
    }
}
