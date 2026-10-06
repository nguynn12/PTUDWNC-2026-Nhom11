using CulinaryBlog.Application.Admin.Commands.ToggleUserStatus;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using Xunit;
using RefreshTokenEntity = CulinaryBlog.Domain.Entities.RefreshToken;

namespace CulinaryBlog.UnitTests.Auth;

/// <summary>SRS FR-AUTH-010: Admin đổi trạng thái user.</summary>
public sealed class ToggleUserStatusTests
{
    private sealed class AdminUser(string userId) : ICurrentUserService
    {
        public string? UserId { get; } = userId;

        public bool IsAdmin => true;
    }

    private static UserAccount Account() =>
        new("user-1", "an@example.com", "Nguyễn An", null, null, ["Author"], true, true, DateTimeOffset.UtcNow);

    [Fact]
    public async Task VoHieuHoa_ThuHoiToanBoRefreshTokenCuaUser()
    {
        var unitOfWork = new FakeUnitOfWork();
        var session1 = RefreshTokenEntity.CreateNewFamily("user-1", new string('a', 64), 7, "127.0.0.1");
        var session2 = RefreshTokenEntity.CreateNewFamily("user-1", new string('b', 64), 7, "127.0.0.1");
        var otherUser = RefreshTokenEntity.CreateNewFamily("user-2", new string('c', 64), 7, "127.0.0.1");
        unitOfWork.Tokens.Tokens.AddRange([session1, session2, otherUser]);

        var handler = new ToggleUserStatusCommandHandler(
            new FakeIdentityService { Account = Account() }, new AdminUser("admin-1"), unitOfWork.Tokens, unitOfWork);

        await handler.Handle(new ToggleUserStatusCommand("user-1", false), TestContext.Current.CancellationToken);

        Assert.True(session1.IsRevoked);
        Assert.True(session2.IsRevoked);
        Assert.False(otherUser.IsRevoked);
        Assert.Equal(1, unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task AdminTuVoHieuHoaChinhMinh_BiTuChoi()
    {
        var handler = new ToggleUserStatusCommandHandler(
            new FakeIdentityService { Account = Account() }, new AdminUser("admin-1"), new FakeRefreshTokenRepository(), new FakeUnitOfWork());

        await Assert.ThrowsAsync<BusinessRuleValidationException>(
            () => handler.Handle(new ToggleUserStatusCommand("admin-1", false), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task UserKhongTonTai_Tra404()
    {
        var handler = new ToggleUserStatusCommandHandler(
            new FakeIdentityService { Account = null }, new AdminUser("admin-1"), new FakeRefreshTokenRepository(), new FakeUnitOfWork());

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new ToggleUserStatusCommand("khong-co", false), TestContext.Current.CancellationToken));
    }
}
