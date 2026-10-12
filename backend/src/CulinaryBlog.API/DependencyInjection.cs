using CulinaryBlog.API.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace CulinaryBlog.API;

public static class DependencyInjection
{
    /// <summary>
    /// Đăng ký các thành phần của tầng Presentation (SRS 6.2: AddApplication, AddInfrastructure,
    /// AddPresentation).
    /// </summary>
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        // ProblemDetails + GlobalExceptionHandler/AuthExceptionHandler được đăng ký ở Program.cs
        // (theo quy ước chung của nhóm); ICurrentUserService đăng ký ở Infrastructure.
        services.AddOpenApi();

        // Mặc định Minimal API chỉ throw BadHttpRequestException ở Development, còn Production
        // trả 400 rỗng. Bật luôn để JSON/tham số sai cú pháp đi qua AuthExceptionHandler và
        // trả 400 MALFORMED_REQUEST thống nhất ở mọi môi trường (RESOLVED-CONFLICTS C3).
        services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

        // NFR-SEC: 10 request/phút/IP cho nhóm endpoint /api/v1/auth (AuthEndpoints dùng policy này).
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = AuthRateLimiting.OnRejectedAsync;
            options.AddPolicy(AuthRateLimiting.PolicyName, AuthRateLimiting.GetPartition);
        });

        return services;
    }
}
