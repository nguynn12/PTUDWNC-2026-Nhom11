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

        // Cấu hình Rate Limiting:
        // - Auth policy: 10 request/phút/IP (SRS NFR-SEC)
        // - Upload policy: 5 request/phút/IP cho tác vụ tải ảnh công thức (Lab 5 - TV4)
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, token) =>
            {
                if (context.HttpContext.Request.Path.Value?.Contains("/images", StringComparison.OrdinalIgnoreCase) == true)
                {
                    await UploadRateLimiting.OnRejectedAsync(context, token);
                }
                else
                {
                    await AuthRateLimiting.OnRejectedAsync(context, token);
                }
            };
            options.AddPolicy(AuthRateLimiting.PolicyName, AuthRateLimiting.GetPartition);
            options.AddPolicy(UploadRateLimiting.PolicyName, UploadRateLimiting.GetPartition);
        });

        return services;
    }
}
