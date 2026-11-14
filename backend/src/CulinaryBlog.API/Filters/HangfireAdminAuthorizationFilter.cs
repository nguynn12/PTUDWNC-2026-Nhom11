using CulinaryBlog.Domain.Constants;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CulinaryBlog.API.Filters;

/// <summary>
/// SEC-04: dashboard Hangfire chỉ cho Admin đã đăng nhập. Ngoại lệ DUY NHẤT: môi trường Development và kết nối từ
/// loopback (127.0.0.1/::1) thì được vào không cần đăng nhập để tiện debug; ở Production/Staging ngoại lệ này không bao giờ bật.
/// </summary>
public class HangfireAdminAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly bool _allowAnonymousInDev;

    public HangfireAdminAuthorizationFilter(bool allowAnonymousInDev = true)
    {
        _allowAnonymousInDev = allowAnonymousInDev;
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        if (_allowAnonymousInDev && httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            var ip = httpContext.Connection.RemoteIpAddress;
            if (ip == null || System.Net.IPAddress.IsLoopback(ip))
            {
                return true;
            }
        }

        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole(Roles.Admin);
    }
}

