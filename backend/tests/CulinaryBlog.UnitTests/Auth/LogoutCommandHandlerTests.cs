using System.Security.Cryptography;
using System.Text;
using CulinaryBlog.Application.Auth.Commands.Logout;
using Xunit;
using RefreshTokenEntity = CulinaryBlog.Domain.Entities.RefreshToken;

namespace CulinaryBlog.UnitTests.Auth;

/// <summary>SRS FR-AUTH-005: logout idempotent, không lỗi với token không tồn tại/đã thu hồi.</summary>
public sealed class LogoutCommandHandlerTests
{
    private static string Hash(string raw) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();

    [Fact]
    public async Task TokenHopLe_BiThuHoiVoiLyDoLogout()
    {
        var unitOfWork = new FakeUnitOfWork();
        var token = RefreshTokenEntity.CreateNewFamily("user-1", Hash("raw-1"), 7, "127.0.0.1");
        unitOfWork.Tokens.Tokens.Add(token);

        await new LogoutCommandHandler(unitOfWork).Handle(new LogoutCommand("raw-1"), TestContext.Current.CancellationToken);

        Assert.True(token.IsRevoked);
        Assert.Equal("logout", token.RevocationReason);
        Assert.Equal(1, unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task GoiHaiLan_KhongLoi()
    {
        var unitOfWork = new FakeUnitOfWork();
        unitOfWork.Tokens.Tokens.Add(RefreshTokenEntity.CreateNewFamily("user-1", Hash("raw-1"), 7, "127.0.0.1"));
        var handler = new LogoutCommandHandler(unitOfWork);

        await handler.Handle(new LogoutCommand("raw-1"), TestContext.Current.CancellationToken);
        await handler.Handle(new LogoutCommand("raw-1"), TestContext.Current.CancellationToken);

        Assert.Equal(1, unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task TokenKhongTonTai_KhongLoiVaKhongGhiDb()
    {
        var unitOfWork = new FakeUnitOfWork();

        await new LogoutCommandHandler(unitOfWork).Handle(new LogoutCommand("khong-ton-tai"), TestContext.Current.CancellationToken);

        Assert.Equal(0, unitOfWork.SaveChangesCount);
    }
}
