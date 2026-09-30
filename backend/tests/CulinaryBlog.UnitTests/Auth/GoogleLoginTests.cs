using System.Security.Cryptography;
using CulinaryBlog.Application.Auth.Commands.GoogleLogin;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Infrastructure.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
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

    // ── GoogleTokenValidator: token ký bằng khoá RSA giả lập thay cho public key của Google ──

    private static readonly RsaSecurityKey SigningKey = new(RSA.Create(2048)) { KeyId = "test-key" };

    private static GoogleTokenValidator Validator(string clientId = ClientId)
    {
        var configuration = new OpenIdConnectConfiguration();
        configuration.SigningKeys.Add(SigningKey);

        return new GoogleTokenValidator(
            Options.Create(new GoogleAuthSettings { ClientId = clientId }),
            new StaticConfigurationManager<OpenIdConnectConfiguration>(configuration),
            NullLogger<GoogleTokenValidator>.Instance);
    }

    private static string CreateIdToken(
        string audience = ClientId,
        string issuer = "https://accounts.google.com",
        DateTime? expires = null,
        string? nonce = null,
        SecurityKey? key = null)
    {
        var claims = new Dictionary<string, object>
        {
            ["sub"] = "google-sub-1",
            ["email"] = "an@gmail.com",
            ["email_verified"] = true,
            ["name"] = "Nguyễn An",
            ["picture"] = "https://lh3.googleusercontent.com/a/photo.jpg",
        };
        if (nonce is not null)
        {
            claims["nonce"] = nonce;
        }

        var expiresAt = expires ?? DateTime.UtcNow.AddMinutes(30);
        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            Claims = claims,
            IssuedAt = expiresAt.AddHours(-1),
            NotBefore = expiresAt.AddHours(-1),
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(key ?? SigningKey, SecurityAlgorithms.RsaSha256),
        });
    }

    [Fact]
    public async Task Validator_TokenHopLe_DocDungThongTin()
    {
        var info = await Validator().ValidateAsync(CreateIdToken(nonce: "n-1"), "n-1", TestContext.Current.CancellationToken);

        Assert.NotNull(info);
        Assert.Equal("google-sub-1", info.Subject);
        Assert.Equal("an@gmail.com", info.Email);
        Assert.True(info.EmailVerified);
        Assert.Equal("Nguyễn An", info.Name);
        Assert.Equal("https://lh3.googleusercontent.com/a/photo.jpg", info.PictureUrl);
    }

    [Fact]
    public async Task Validator_SaiAudience_TuChoi()
    {
        Assert.Null(await Validator().ValidateAsync(CreateIdToken(audience: "client-khac"), null, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Validator_SaiIssuer_TuChoi()
    {
        Assert.Null(await Validator().ValidateAsync(CreateIdToken(issuer: "https://evil.example.com"), null, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Validator_HetHan_TuChoi()
    {
        Assert.Null(await Validator().ValidateAsync(
            CreateIdToken(expires: DateTime.UtcNow.AddMinutes(-10)), null, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Validator_KyBangKhoaKhac_TuChoi()
    {
        var otherKey = new RsaSecurityKey(RSA.Create(2048)) { KeyId = "test-key" };

        Assert.Null(await Validator().ValidateAsync(CreateIdToken(key: otherKey), null, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Validator_NonceKhongKhop_TuChoi()
    {
        Assert.Null(await Validator().ValidateAsync(CreateIdToken(nonce: "n-1"), "n-2", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Validator_ChuaCauHinhClientId_TuChoi()
    {
        Assert.Null(await Validator(clientId: "").ValidateAsync(CreateIdToken(), null, TestContext.Current.CancellationToken));
    }
}
