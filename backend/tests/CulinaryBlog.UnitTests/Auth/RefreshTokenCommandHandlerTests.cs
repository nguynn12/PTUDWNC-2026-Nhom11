using System.Security.Cryptography;
using System.Text;
using CulinaryBlog.Application.Auth.Commands.RefreshToken;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Domain.Exceptions;
using Xunit;
using RefreshTokenEntity = CulinaryBlog.Domain.Entities.RefreshToken;

namespace CulinaryBlog.UnitTests.Auth;

/// <summary>SRS FR-AUTH-004: rotation, phát hiện dùng lại token (reuse detection), token hết hạn, user bị khoá.</summary>
public sealed class RefreshTokenCommandHandlerTests
{
    private const string UserId = "user-1";
    private const string Ip = "127.0.0.1";

    private static string Hash(string raw) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();

    private static UserAccount Account(bool isActive = true) =>
        new(UserId, "author@culinaryblog.local", "Tác giả", null, null, ["Author"], true, isActive, DateTimeOffset.UtcNow);

    private static RefreshTokenCommandHandler CreateHandler(FakeUnitOfWork unitOfWork, UserAccount? account) =>
        new(unitOfWork, new FakeJwtService(), new FakeIdentityService { Account = account });

    [Fact]
    public async Task TokenHopLe_ThuHoiTokenCuVaTaoTokenMoiCungFamily()
    {
        var unitOfWork = new FakeUnitOfWork();
        var oldToken = RefreshTokenEntity.CreateNewFamily(UserId, Hash("raw-1"), 7, Ip);
        unitOfWork.Tokens.Tokens.Add(oldToken);

        var response = await CreateHandler(unitOfWork, Account())
            .Handle(new RefreshTokenCommand("raw-1"), TestContext.Current.CancellationToken);

        Assert.Equal("raw-refresh-token", response.RefreshToken);
        Assert.True(oldToken.IsRevoked);
        Assert.Equal("rotated", oldToken.RevocationReason);
        var newToken = Assert.Single(unitOfWork.Tokens.Tokens, token => token.Id != oldToken.Id);
        Assert.Equal(oldToken.FamilyId, newToken.FamilyId);
        Assert.True(newToken.IsActive);
    }

    [Fact]
    public async Task DungLaiTokenDaThuHoi_ThuHoiCaFamilyVaTra401()
    {
        var unitOfWork = new FakeUnitOfWork();
        var oldToken = RefreshTokenEntity.CreateNewFamily(UserId, Hash("raw-1"), 7, Ip);
        var currentToken = oldToken.Rotate(Hash("raw-2"), 7, Ip);
        unitOfWork.Tokens.Tokens.AddRange([oldToken, currentToken]);

        await Assert.ThrowsAsync<RefreshTokenRevokedException>(() => CreateHandler(unitOfWork, Account())
            .Handle(new RefreshTokenCommand("raw-1"), TestContext.Current.CancellationToken));

        Assert.True(currentToken.IsRevoked);
        Assert.Equal("reuse-detected", currentToken.RevocationReason);
    }

    [Fact]
    public async Task TokenHetHan_Tra401Expired()
    {
        var unitOfWork = new FakeUnitOfWork();
        unitOfWork.Tokens.Tokens.Add(RefreshTokenEntity.CreateNewFamily(UserId, Hash("raw-1"), -1, Ip));

        await Assert.ThrowsAsync<RefreshTokenExpiredException>(() => CreateHandler(unitOfWork, Account())
            .Handle(new RefreshTokenCommand("raw-1"), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TokenKhongTonTai_Tra401()
    {
        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => CreateHandler(new FakeUnitOfWork(), Account())
            .Handle(new RefreshTokenCommand("khong-ton-tai"), TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.AuthTokenInvalid, ex.ErrorCode);
    }

    [Fact]
    public async Task UserBiVoHieuHoa_Tra403VaThuHoiMoiToken()
    {
        var unitOfWork = new FakeUnitOfWork();
        var token = RefreshTokenEntity.CreateNewFamily(UserId, Hash("raw-1"), 7, Ip);
        unitOfWork.Tokens.Tokens.Add(token);

        var ex = await Assert.ThrowsAsync<ForbiddenException>(() => CreateHandler(unitOfWork, Account(isActive: false))
            .Handle(new RefreshTokenCommand("raw-1"), TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.AuthAccountDisabled, ex.ErrorCode);
        Assert.True(token.IsRevoked);
        Assert.Single(unitOfWork.Tokens.Tokens);
    }
}
