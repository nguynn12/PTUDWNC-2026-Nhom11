using CulinaryBlog.Application.Auth.Commands.UpdateProfile;
using CulinaryBlog.Application.Auth.Queries.GetProfile;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using Xunit;

namespace CulinaryBlog.UnitTests.Auth;

/// <summary>SRS FR-AUTH-006 (xem hồ sơ) và FR-AUTH-007 (cập nhật hồ sơ).</summary>
public sealed class ProfileTests
{
    private sealed class FakeCurrentUser(string? userId) : ICurrentUserService
    {
        public string? UserId { get; } = userId;

        public bool IsAdmin => false;
    }

    private static UserAccount Account(bool isActive = true) =>
        new("user-1", "an@example.com", "Nguyễn An", "https://cdn.example.com/a.png", "Thích nấu ăn", ["Author"], true, isActive,
            new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task XemHoSo_TraDuFieldTheoSrs()
    {
        var handler = new GetProfileQueryHandler(new FakeCurrentUser("user-1"), new FakeIdentityService { Account = Account() });

        var dto = await handler.Handle(new GetProfileQuery(), TestContext.Current.CancellationToken);

        Assert.Equal("user-1", dto.Id);
        Assert.Equal("an@example.com", dto.Email);
        Assert.Equal("Nguyễn An", dto.DisplayName);
        Assert.Equal("https://cdn.example.com/a.png", dto.AvatarUrl);
        Assert.Equal("Thích nấu ăn", dto.Bio);
        Assert.Contains("Author", dto.Roles);
        Assert.True(dto.EmailConfirmed);
        Assert.Equal(2026, dto.CreatedAt.Year);
    }

    [Fact]
    public async Task XemHoSo_TaiKhoanBiVoHieuHoa_Tra403()
    {
        var handler = new GetProfileQueryHandler(new FakeCurrentUser("user-1"), new FakeIdentityService { Account = Account(isActive: false) });

        var ex = await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(new GetProfileQuery(), TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.AuthAccountDisabled, ex.ErrorCode);
    }

    [Fact]
    public async Task XemHoSo_UserKhongTonTai_Tra401()
    {
        var handler = new GetProfileQueryHandler(new FakeCurrentUser("user-1"), new FakeIdentityService { Account = null });

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.Handle(new GetProfileQuery(), TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.AuthTokenInvalid, ex.ErrorCode);
    }

    [Fact]
    public async Task CapNhatHoSo_ChiGuiBio_GiuNguyenFieldKhac()
    {
        var identity = new FakeIdentityService { Account = Account() };
        var handler = new UpdateProfileCommandHandler(new FakeCurrentUser("user-1"), identity);

        await handler.Handle(new UpdateProfileCommand(null, null, "Bio mới"), TestContext.Current.CancellationToken);

        Assert.Equal((null, "Bio mới", null), identity.LastProfileUpdate);
    }

    [Theory]
    [InlineData("A", null, null, "DisplayName")]
    [InlineData(null, "khong-phai-url", null, "AvatarUrl")]
    [InlineData(null, "ftp://cdn.example.com/a.png", null, "AvatarUrl")]
    public void CapNhatHoSo_DuLieuSai_KhongHopLe(string? displayName, string? avatarUrl, string? bio, string field)
    {
        var result = new UpdateProfileCommandValidator().Validate(new UpdateProfileCommand(displayName, avatarUrl, bio));

        Assert.Contains(result.Errors, e => e.PropertyName == field);
    }

    [Fact]
    public void CapNhatHoSo_BioQua1000KyTu_KhongHopLe()
    {
        var result = new UpdateProfileCommandValidator().Validate(new UpdateProfileCommand(null, null, new string('a', 1001)));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateProfileCommand.Bio));
    }

    [Fact]
    public void CapNhatHoSo_DuLieuDung_HopLe()
    {
        var result = new UpdateProfileCommandValidator().Validate(
            new UpdateProfileCommand("Nguyễn An", "https://cdn.example.com/a.png", new string('a', 1000)));

        Assert.True(result.IsValid);
    }
}
