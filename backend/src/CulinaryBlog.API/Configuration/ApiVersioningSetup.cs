using Asp.Versioning;

namespace CulinaryBlog.API.Configuration;

/// <summary>
/// API versioning theo đường dẫn URL (SRS 5.2 "Versioning: URL Path versioning /api/v1/"), dùng Asp.Versioning.
/// Mọi endpoint nghiệp vụ nằm trong nhóm <c>/api/v{version}</c>; hiện chỉ có v1.
/// Khi có thay đổi phá vỡ tương thích: thêm <c>V2</c> và nhóm mới, giữ v1 tối thiểu 6 tháng.
/// </summary>
public static class ApiVersioningSetup
{
    public static readonly ApiVersion V1 = new(1, 0);

    /// <summary>Tiền tố route của nhóm API có phiên bản.</summary>
    public const string RoutePrefix = "/api/v{version:apiVersion}";

    /// <summary>Tên tài liệu OpenAPI tương ứng với v1 (GroupNameFormat "'v'VVV" → "v1").</summary>
    public const string V1DocumentName = "v1";

    public static IServiceCollection AddCulinaryApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = V1;
                options.AssumeDefaultVersionWhenUnspecified = false; // URL luôn ghi rõ phiên bản
                options.ReportApiVersions = true;                    // header api-supported-versions
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true; // tài liệu hiển thị /api/v1/... thay vì /api/v{version}/...
            });

        return services;
    }

    /// <summary>Tạo nhóm route <c>/api/v1</c> dùng chung cho mọi module.</summary>
    public static RouteGroupBuilder MapVersionedApi(this IEndpointRouteBuilder app) =>
        app.NewVersionedApi("CulinaryBlog")
           .MapGroup(RoutePrefix)
           .HasApiVersion(V1);
}
