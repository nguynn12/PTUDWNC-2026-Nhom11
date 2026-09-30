using CulinaryBlog.API.ErrorHandling;
using CulinaryBlog.API.Services;
using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace CulinaryBlog.API;

public static class DependencyInjection
{
    /// <summary>
    /// Đăng ký các thành phần của tầng Presentation (SRS 6.2: AddApplication, AddInfrastructure,
    /// AddPresentation).
    /// </summary>
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        
        services.AddOpenApi();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        // Mặc định Minimal API chỉ throw BadHttpRequestException ở Development, còn Production
        // trả 400 rỗng. Bật luôn để JSON/tham số sai cú pháp đi qua GlobalExceptionHandler và
        // trả 400 MALFORMED_REQUEST thống nhất ở mọi môi trường (RESOLVED-CONFLICTS C3).
        services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            
            options.AddFixedWindowLimiter("Auth", limiterOptions =>
            {
                limiterOptions.PermitLimit = 5;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });
        });

        return services;
    }
}
