using CulinaryBlog.API.ErrorHandling;

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

        // Mặc định Minimal API chỉ throw BadHttpRequestException ở Development, còn Production
        // trả 400 rỗng. Bật luôn để JSON/tham số sai cú pháp đi qua GlobalExceptionHandler và
        // trả 400 MALFORMED_REQUEST thống nhất ở mọi môi trường (RESOLVED-CONFLICTS C3).
        services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

        return services;
    }
}
