using System.IdentityModel.Tokens.Jwt;
using CulinaryBlog.Infrastructure.Identity;
using Microsoft.Extensions.Options;
using Xunit;

namespace CulinaryBlog.UnitTests.Auth;

/// <summary>SRS NFR-SEC: access token sống 15 phút và <c>expiresIn</c> phải khớp thời hạn thật của token.</summary>
public sealed class JwtServiceTests
{
    private static JwtService CreateService(int? expiryMinutes = null)
    {
        var settings = expiryMinutes is null
            ? new JwtSettings { Secret = new string('k', 64), Issuer = "test-issuer", Audience = "test-audience" }
            : new JwtSettings { Secret = new string('k', 64), Issuer = "test-issuer", Audience = "test-audience", ExpiryMinutes = expiryMinutes.Value };

        return new JwtService(Options.Create(settings));
    }

    [Fact]
    public void ExpiryMinutes_MacDinhLa15Phut()
    {
        Assert.Equal(15, new JwtSettings().ExpiryMinutes);
        Assert.Equal(900, CreateService().AccessTokenLifetimeSeconds);
    }

    [Fact]
    public void GenerateAccessToken_HetHanDungThoiGianCauHinh()
    {
        var service = CreateService(15);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(
            service.GenerateAccessToken("user-1", "author@culinaryblog.local", ["Author"], emailConfirmed: true));

        var expectedExpiry = DateTime.UtcNow.AddSeconds(service.AccessTokenLifetimeSeconds);
        Assert.InRange(token.ValidTo, expectedExpiry.AddMinutes(-1), expectedExpiry.AddMinutes(1));
    }
}
