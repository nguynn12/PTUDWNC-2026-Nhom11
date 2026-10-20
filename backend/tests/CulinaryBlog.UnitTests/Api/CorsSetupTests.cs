using CulinaryBlog.API.Configuration;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace CulinaryBlog.UnitTests.Api;

/// <summary>Lab 5 – CORS allowlist theo SRS 5.2 và NFR-SEC.</summary>
public sealed class CorsSetupTests
{
    private sealed class FakeEnvironment(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "CulinaryBlog.API";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private static IConfiguration Config(params string[] origins) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(origins.Select((origin, i) =>
                new KeyValuePair<string, string?>($"Cors:AllowedOrigins:{i}", origin)))
            .Build();

    private static CorsPolicy BuildPolicy(params string[] origins)
    {
        var builder = new CorsPolicyBuilder();
        CorsSetup.Configure(builder, origins);
        return builder.Build();
    }

    [Fact]
    public void DocOrigin_ChuanHoaVaBoTrung()
    {
        var origins = CorsSetup.ResolveOrigins(
            Config(" http://localhost:3000/ ", "http://LOCALHOST:3000", "https://culinaryblog.com"),
            new FakeEnvironment(Environments.Production));

        Assert.Equal(["http://localhost:3000", "https://culinaryblog.com"], origins);
    }

    [Fact]
    public void ChuaCauHinhOrigin_DungKhoiDong()
    {
        Assert.Throws<InvalidOperationException>(() =>
            CorsSetup.ResolveOrigins(Config(), new FakeEnvironment(Environments.Development)));
    }

    [Fact]
    public void Wildcard_NgoaiDevelopment_BiTuChoi()
    {
        Assert.Throws<InvalidOperationException>(() =>
            CorsSetup.ResolveOrigins(Config("*"), new FakeEnvironment(Environments.Production)));
    }

    [Fact]
    public void Wildcard_TrongDevelopment_DuocPhep()
    {
        var origins = CorsSetup.ResolveOrigins(Config("*", "http://localhost:3000"), new FakeEnvironment(Environments.Development));

        Assert.Equal(["*"], origins);
        Assert.True(BuildPolicy(origins).AllowAnyOrigin);
    }

    [Theory]
    [InlineData("localhost:3000")]
    [InlineData("ftp://localhost")]
    [InlineData("http://localhost:3000/api")]
    public void OriginSaiDinhDang_BiTuChoi(string origin)
    {
        Assert.Throws<InvalidOperationException>(() =>
            CorsSetup.ResolveOrigins(Config(origin), new FakeEnvironment(Environments.Production)));
    }

    [Fact]
    public void Policy_DungMethodHeaderTheoSrsVaKhongDungCookie()
    {
        var policy = BuildPolicy("http://localhost:3000");

        Assert.Equal(["http://localhost:3000"], policy.Origins);
        Assert.False(policy.AllowAnyOrigin);
        Assert.False(policy.SupportsCredentials);
        Assert.Equal(["GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS"], policy.Methods);
        Assert.Contains("Authorization", policy.Headers);
        Assert.Contains("X-Correlation-ID", policy.Headers);
        Assert.Contains("If-Match", policy.Headers);
        Assert.Contains("Retry-After", policy.ExposedHeaders);
        Assert.Contains("ETag", policy.ExposedHeaders);
        Assert.Equal(TimeSpan.FromMinutes(10), policy.PreflightMaxAge);
    }

    [Fact]
    public void Policy_ChapNhanOriginTrongDanhSach_TuChoiOriginLa()
    {
        var policy = BuildPolicy("http://localhost:3000");

        Assert.True(policy.IsOriginAllowed("http://localhost:3000"));
        Assert.False(policy.IsOriginAllowed("http://evil.example.com"));
    }
}
