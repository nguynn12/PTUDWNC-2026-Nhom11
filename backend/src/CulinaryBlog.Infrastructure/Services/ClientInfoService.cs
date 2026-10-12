using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CulinaryBlog.Infrastructure.Services;

/// <summary>
/// Lấy IP client từ HttpContext. Khi chạy sau reverse proxy cần bật ForwardedHeaders để
/// RemoteIpAddress là IP thật của người dùng.
/// </summary>
public sealed class ClientInfoService(IHttpContextAccessor httpContextAccessor) : IClientInfoService
{
    public const string UnknownIp = "unknown";

    public string IpAddress
    {
        get
        {
            var address = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;
            if (address is null)
            {
                return UnknownIp;
            }

            // "::ffff:127.0.0.1" → "127.0.0.1" cho dễ đọc khi server lắng nghe dual-stack.
            return (address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address).ToString();
        }
    }
}
