using System.Security.Claims;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace CulinaryBlog.Infrastructure.Identity;

/// <summary>
/// Xác minh Google ID token theo chuẩn OpenID Connect (FR-AUTH-003): chữ ký bằng public key công bố
/// tại discovery document của Google (tự tải và cache, tự làm mới khi Google xoay key), issuer
/// <c>accounts.google.com</c>, audience = <see cref="GoogleAuthSettings.ClientId"/>, hạn dùng và
/// nonce (nếu client gửi kèm). Dùng thư viện Microsoft.IdentityModel sẵn có của JwtBearer nên không
/// cần thêm package mới.
/// </summary>
public sealed class GoogleTokenValidator : IGoogleTokenValidator
{
    public const string DiscoveryUrl = "https://accounts.google.com/.well-known/openid-configuration";

    private static readonly string[] ValidIssuers = ["https://accounts.google.com", "accounts.google.com"];

    private readonly GoogleAuthSettings _settings;
    private readonly IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
    private readonly ILogger<GoogleTokenValidator> _logger;
    private readonly JsonWebTokenHandler _tokenHandler = new() { MapInboundClaims = false };

    public GoogleTokenValidator(
        IOptions<GoogleAuthSettings> settings,
        IConfigurationManager<OpenIdConnectConfiguration> configurationManager,
        ILogger<GoogleTokenValidator> logger)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings.Value;
        _configurationManager = configurationManager;
        _logger = logger;
    }

    /// <summary>Configuration manager đọc discovery document + JWKS của Google (cache mặc định 12 giờ).</summary>
    public static IConfigurationManager<OpenIdConnectConfiguration> CreateGoogleConfigurationManager() =>
        new ConfigurationManager<OpenIdConnectConfiguration>(
            DiscoveryUrl,
            new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever { RequireHttps = true });

    public async Task<GoogleUserInfo?> ValidateAsync(string idToken, string? nonce, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ClientId))
        {
            _logger.LogError("Chưa cấu hình {Section}:ClientId nên không thể xác minh Google ID token.", GoogleAuthSettings.SectionName);
            return null;
        }

        if (string.IsNullOrWhiteSpace(idToken))
        {
            return null;
        }

        var configuration = await _configurationManager.GetConfigurationAsync(cancellationToken);

        var result = await _tokenHandler.ValidateTokenAsync(idToken, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = ValidIssuers,
            ValidateAudience = true,
            ValidAudience = _settings.ClientId,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = configuration.SigningKeys,
            ClockSkew = TimeSpan.FromMinutes(1),
        });

        if (!result.IsValid)
        {
            if (result.Exception is SecurityTokenSignatureKeyNotFoundException)
            {
                // Google vừa xoay key: lần sau tải lại JWKS.
                _configurationManager.RequestRefresh();
            }

            _logger.LogInformation(result.Exception, "Google ID token không hợp lệ.");
            return null;
        }

        var claims = result.ClaimsIdentity;
        var subject = claims.FindFirst("sub")?.Value;
        var email = claims.FindFirst("email")?.Value;
        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        if (!string.IsNullOrEmpty(nonce) && !string.Equals(claims.FindFirst("nonce")?.Value, nonce, StringComparison.Ordinal))
        {
            _logger.LogWarning("Google ID token có nonce không khớp.");
            return null;
        }

        return new GoogleUserInfo(
            subject,
            email,
            IsTrue(claims.FindFirst("email_verified")),
            claims.FindFirst("name")?.Value,
            claims.FindFirst("picture")?.Value);
    }

    private static bool IsTrue(Claim? claim) =>
        claim is not null && bool.TryParse(claim.Value, out var value) && value;
}
