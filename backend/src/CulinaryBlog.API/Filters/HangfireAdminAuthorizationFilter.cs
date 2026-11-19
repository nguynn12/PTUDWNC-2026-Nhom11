using CulinaryBlog.Domain.Constants;
using Hangfire.Dashboard;

namespace CulinaryBlog.API.Filters;

/// <summary>
/// Dashboard Hangfire chỉ cho Admin đã đăng nhập ở mọi môi trường.
/// </summary>
public class HangfireAdminAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole(Roles.Admin);
    }
}

