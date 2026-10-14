using CulinaryBlog.Application.Auth.Commands.GoogleLogin;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Infrastructure.Identity;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CulinaryBlog.UnitTests.Auth;

/// <summary>SRS FR-AUTH-003: đăng nhập Google bằng ID token.</summary>
public sealed class GoogleLoginTests
{
    private const string ClientId = "test-client.apps.googleusercontent.com";

    private sealed class FakeGoogleTokenValidator(GoogleUserInfo? result) : IGoogleTokenValidator
    {
        public Task<GoogleUserInfo?> ValidateAsync(string idToken, string? nonce, CancellationToken cancellationToken = default) =>
            Task.FromResult(result);
    }

    private static readonly GoogleUserInfo GoogleUser = new("google-sub-1", "an@gmail.com", true, "Nguyễn An", null);

    private static UserAccount Account(bool isActive = true) =>
        new("user-1", "an@gmail.com", "Nguyễn An", null, null, ["Author"], true, isActive, DateTimeOffset.UtcNow);

    private static GoogleLoginCommandHandler Handler(GoogleUserInfo? google, FakeIdentityService identity, FakeUnitOfWork unitOfWork) =>
        new(new FakeGoogleTokenValidator(google), identity, new FakeJwtService(), unitOfWork.Tokens, unitOfWork, new FakeClientInfo());

    [Fact]
    public async Task TokenHopLe_TraCapTokenVaLuuRefreshToken()
    {
        var unitOfWork = new FakeUnitOfWork();
        var handler = Handler(GoogleUser, new FakeIdentityService { Account = Account() }, unitOfWork);

        var response = await handler.Handle(new GoogleLoginCommand("id-token"), TestContext.Current.CancellationToken);

        Assert.Equal("access-token", response.AccessToken);
        Assert.Equal("user-1", response.User.Id);
        Assert.Single(unitOfWork.Tokens.Tokens);
    }

    [Fact]
    public async Task TokenSai_Tra401AuthGoogleTokenInvalid()
    {
        var unitOfWork = new FakeUnitOfWork();
        var handler = Handler(null, new FakeIdentityService { Account = Account() }, unitOfWork);

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.Handle(new GoogleLoginCommand("sai"), TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.AuthGoogleTokenInvalid, ex.ErrorCode);
        Assert.Empty(unitOfWork.Tokens.Tokens);
    }

    [Fact]
    public async Task EmailDaTonTaiNhungGoogleChuaXacMinh_Tra409()
    {
        var identity = new FakeIdentityService { GoogleResult = new GoogleLoginResult(GoogleLoginStatus.EmailNotVerified) };
        var handler = Handler(GoogleUser with { EmailVerified = false }, identity, new FakeUnitOfWork());

        var ex = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(new GoogleLoginCommand("id-token"), TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.AuthEmailExists, ex.ErrorCode);
    }

    [Fact]
    public async Task TaiKhoanBiVoHieuHoa_Tra403()
    {
        var handler = Handler(GoogleUser, new FakeIdentityService { Account = Account(isActive: false) }, new FakeUnitOfWork());

        var ex = await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(new GoogleLoginCommand("id-token"), TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.AuthAccountDisabled, ex.ErrorCode);
    }

    [Theory]
    [InlineData("Nguyễn An", "an@gmail.com", "Nguyễn An")]
    [InlineData(null, "an.nguyen@gmail.com", "an.nguyen")]
    [InlineData("A", "b@gmail.com", "b@gmail.com")]
    public void TenHienThi_Tu2Den100KyTu(string? name, string email, string expected)
    {
        Assert.Equal(expected, IdentityService.BuildDisplayName(new GoogleUserInfo("sub", email, true, name, null)));
    }

    [Fact]
    public void TenHienThiQuaDai_CatCon100KyTu()
    {
        Assert.Equal(100, IdentityService.BuildDisplayName(new GoogleUserInfo("sub", "a@gmail.com", true, new string('x', 150), null)).Length);
    }

    // ── GoogleTokenValidator: thư viện Google (GoogleJsonWebSignature) được thay bằng delegate giả lập ──

    private static GoogleJsonWebSignature.Payload Payload(string? nonce = null, string? email = "an@gmail.com") => new()
    {
        Subject = "google-sub-1",
        Email = email,
        EmailVerified = true,
        Name = "Nguyễn An",
        Picture = "https://lh3.googleusercontent.com/a/photo.jpg",
        Nonce = nonce,
    };

    private static GoogleTokenValidator Validator(GoogleTokenValidator.VerifyIdToken verify, string clientId = ClientId) =>
        new(Options.Create(new GoogleAuthSettings { ClientId = clientId }), NullLogger<GoogleTokenValidator>.Instance, verify);

    [Fact]
    public async Task Validator_TokenHopLe_DocDungThongTinVaKiemTraAudience()
    {
        GoogleJsonWebSignature.ValidationSettings? used = null;
        var validator = Validator((_, settings) =>
        {
            used = settings;
            return Task.FromResult(Payload(nonce: "n-1"));
        });

        var info = await validator.ValidateAsync("id-token", "n-1", TestContext.Current.CancellationToken);

        Assert.NotNull(info);
        Assert.Equal("google-sub-1", info.Subject);
        Assert.Equal("an@gmail.com", info.Email);
        Assert.True(info.EmailVerified);
        Assert.Equal("Nguyễn An", info.Name);
        Assert.Equal("https://lh3.googleusercontent.com/a/photo.jpg", info.PictureUrl);
        Assert.NotNull(used);
        Assert.Equal(new[] { ClientId }, used.Audience);
    }

    [Fact]
    public async Task Validator_ThuVienGoogleTuChoi_TraNull()
    {
        var validator = Validator((_, _) => throw new InvalidJwtException("JWT has expired."));

        Assert.Null(await validator.ValidateAsync("id-token", null, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Validator_NonceKhongKhop_TuChoi()
    {
        var validator = Validator((_, _) => Task.FromResult(Payload(nonce: "n-1")));

        Assert.Null(await validator.ValidateAsync("id-token", "n-2", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Validator_ClientGuiNonceNhungTokenKhongCo_TuChoi()
    {
        var validator = Validator((_, _) => Task.FromResult(Payload(nonce: null)));

        Assert.Null(await validator.ValidateAsync("id-token", "n-1", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Validator_ThieuEmail_TuChoi()
    {
        var validator = Validator((_, _) => Task.FromResult(Payload(email: null)));

        Assert.Null(await validator.ValidateAsync("id-token", null, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Validator_ChuaCauHinhClientId_KhongGoiGoogle()
    {
        var called = false;
        var validator = Validator((_, _) =>
        {
            called = true;
            return Task.FromResult(Payload());
        }, clientId: "");

        Assert.Null(await validator.ValidateAsync("id-token", null, TestContext.Current.CancellationToken));
        Assert.False(called);
    }
}
