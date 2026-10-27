using System.Net;
using CulinaryBlog.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace CulinaryBlog.UnitTests.Auth;

/// <summary>IP client được ghi vào <c>RefreshToken.CreatedByIp</c> (varchar 45).</summary>
public sealed class ClientInfoServiceTests
{
    private static ClientInfoService ServiceFor(IPAddress? ip)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = ip;
        return new ClientInfoService(new HttpContextAccessor { HttpContext = context });
    }

    [Fact]
    public void Ipv4MappedIpv6_DoiVeIpv4()
    {
        Assert.Equal("127.0.0.1", ServiceFor(IPAddress.Parse("::ffff:127.0.0.1")).IpAddress);
    }

    [Fact]
    public void Ipv6_GiuNguyen()
    {
        Assert.Equal("2001:db8::1", ServiceFor(IPAddress.Parse("2001:db8::1")).IpAddress);
    }

    [Fact]
    public void KhongCoHttpContext_TraUnknown()
    {
        Assert.Equal(ClientInfoService.UnknownIp, new ClientInfoService(new HttpContextAccessor()).IpAddress);
    }
}
