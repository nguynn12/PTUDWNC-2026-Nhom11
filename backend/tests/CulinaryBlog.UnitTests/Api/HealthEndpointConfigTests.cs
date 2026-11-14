using CulinaryBlog.API.Endpoints;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CulinaryBlog.UnitTests.Api;

/// <summary>OPS-03: health check đọc đúng cấu hình đang dùng, không hard-code localhost.</summary>
public class HealthEndpointConfigTests
{
    private static IConfiguration Config(params (string Key, string Value)[] items) =>
        new ConfigurationBuilder().AddInMemoryCollection(items.ToDictionary(i => i.Key, i => (string?)i.Value)).Build();

    [Fact]
    public void Redis_TuConnectionString_LayTenDichVuCompose() =>
        Assert.Equal(("redis", 6379), HealthEndpoints.ResolveRedisEndpoint(Config(("ConnectionStrings:Redis", "redis:6379,abortConnect=false"))));

    [Fact]
    public void Redis_ConnectionStringKhongCoPort_DungMacDinh6379() =>
        Assert.Equal(("cache.internal", 6379), HealthEndpoints.ResolveRedisEndpoint(Config(("ConnectionStrings:Redis", "cache.internal"))));

    [Fact]
    public void Redis_KhongCoConnectionString_DungRedisHostPort() =>
        Assert.Equal(("r1", 6400), HealthEndpoints.ResolveRedisEndpoint(Config(("Redis:Host", "r1"), ("Redis:Port", "6400"))));

    [Fact]
    public void Redis_KhongCauHinh_MacDinhLocalhost() =>
        Assert.Equal(("localhost", 6379), HealthEndpoints.ResolveRedisEndpoint(Config()));

    [Fact]
    public void Minio_TuEndpoint_LayHostVaPort() =>
        Assert.Equal(("minio", 9000), HealthEndpoints.ResolveMinioEndpoint(Config(("Minio:Endpoint", "http://minio:9000"))));
}
