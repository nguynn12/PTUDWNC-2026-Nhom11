using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Exceptions;
using Xunit;

namespace CulinaryBlog.UnitTests.Entities;

/// <summary>Quy tắc của entity RefreshToken (SRS FR-AUTH-004/005, mục 7.8).</summary>
public sealed class RefreshTokenTests
{
    private static readonly string Hash1 = new('a', 64);
    private static readonly string Hash2 = new('b', 64);

    [Fact]
    public void CreateNewFamily_FamilyIdBangIdVaDangHoatDong()
    {
        var token = RefreshToken.CreateNewFamily("user-1", Hash1, 7, "127.0.0.1");

        Assert.Equal(token.Id, token.FamilyId);
        Assert.True(token.IsActive);
    }

    [Fact]
    public void CreateNewFamily_ThieuIp_NemArgumentException()
    {
        Assert.ThrowsAny<ArgumentException>(() => RefreshToken.CreateNewFamily("user-1", Hash1, 7, " "));
    }

    [Fact]
    public void Rotate_ThuHoiTokenCuVaTaoTokenMoiCungFamily()
    {
        var token = RefreshToken.CreateNewFamily("user-1", Hash1, 7, "127.0.0.1");

        var next = token.Rotate(Hash2, 7, "127.0.0.1");

        Assert.True(token.IsRevoked);
        Assert.Equal("rotated", token.RevocationReason);
        Assert.Equal(Hash2, token.ReplacedByTokenHash);
        Assert.Equal(token.FamilyId, next.FamilyId);
        Assert.NotEqual(token.Id, next.Id);
        Assert.True(next.IsActive);
    }

    [Fact]
    public void Rotate_TokenDaBiThuHoi_NemRefreshTokenRevokedException()
    {
        var token = RefreshToken.CreateNewFamily("user-1", Hash1, 7, "127.0.0.1");
        token.Revoke("logout");

        var ex = Assert.Throws<RefreshTokenRevokedException>(() => token.Rotate(Hash2, 7, "127.0.0.1"));
        Assert.Equal("AUTH_REFRESH_TOKEN_REVOKED", ex.ErrorCode);
    }

    [Fact]
    public void Rotate_TokenHetHan_NemRefreshTokenExpiredException()
    {
        var token = RefreshToken.CreateNewFamily("user-1", Hash1, -1, "127.0.0.1");

        var ex = Assert.Throws<RefreshTokenExpiredException>(() => token.Rotate(Hash2, 7, "127.0.0.1"));
        Assert.Equal("AUTH_REFRESH_TOKEN_EXPIRED", ex.ErrorCode);
    }

    [Fact]
    public void Revoke_GoiHaiLan_KhongDoiThoiDiemThuHoi()
    {
        var token = RefreshToken.CreateNewFamily("user-1", Hash1, 7, "127.0.0.1");
        token.Revoke("logout");
        var firstRevokedAt = token.RevokedAt;

        token.Revoke("admin-deactivated");

        Assert.Equal(firstRevokedAt, token.RevokedAt);
        Assert.Equal("logout", token.RevocationReason);
    }
}
