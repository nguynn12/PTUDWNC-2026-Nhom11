using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CulinaryBlog.API.OpenApi;

/// <summary>Tài liệu OpenAPI (Scalar) cho Culinary Blog: thông tin chung và cơ chế xác thực Bearer JWT.</summary>
public static class OpenApiSetup
{
    public const string BearerSchemeName = "Bearer";

    public static IServiceCollection AddCulinaryOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            options.AddOperationTransformer<BearerSecurityRequirementTransformer>();
        });

        return services;
    }

    /// <summary>Endpoint có yêu cầu đăng nhập (RequireAuthorization / [Authorize]) và không AllowAnonymous.</summary>
    public static bool RequiresAuthorization(IEnumerable<object> endpointMetadata)
    {
        ArgumentNullException.ThrowIfNull(endpointMetadata);

        if (endpointMetadata.OfType<IAllowAnonymous>().Any())
        {
            return false;
        }

        return endpointMetadata.OfType<IAuthorizeData>().Any()
            || endpointMetadata.OfType<AuthorizationPolicy>().Any();
    }
}

/// <summary>Khai báo security scheme "Bearer" (HTTP bearer, JWT) để Scalar hiện ô nhập access token.</summary>
public sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);

        document.Info ??= new OpenApiInfo();
        document.Info.Title = "Culinary Blog API";
        document.Info.Version = context?.DocumentName ?? "v1";
        document.Info.Description =
            "REST API của Culinary Blog (PTUDWNC Nhóm 11). Các endpoint có biểu tượng khoá yêu cầu header " +
            "Authorization: Bearer <access_token> lấy từ POST /api/v1/auth/login, /auth/register hoặc /auth/google.";

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[OpenApiSetup.BearerSchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Access token JWT (HS256, hạn 15 phút). Chỉ dán phần token, không gõ chữ \"Bearer\".",
        };

        return Task.CompletedTask;
    }
}

/// <summary>Gắn yêu cầu Bearer (biểu tượng khoá trong Scalar) cho đúng các endpoint cần đăng nhập.</summary>
public sealed class BearerSecurityRequirementTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);

        if (!OpenApiSetup.RequiresAuthorization(context.Description.ActionDescriptor.EndpointMetadata))
        {
            return Task.CompletedTask;
        }

        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(OpenApiSetup.BearerSchemeName, context.Document)] = [],
        });

        operation.Responses ??= new OpenApiResponses();
        operation.Responses.TryAdd("401", new OpenApiResponse
        {
            Description = "Chưa đăng nhập hoặc access token sai/hết hạn (AUTH_TOKEN_INVALID / AUTH_TOKEN_EXPIRED).",
        });
        operation.Responses.TryAdd("403", new OpenApiResponse
        {
            Description = "Không đủ quyền (vai trò, chủ sở hữu hoặc email chưa xác nhận).",
        });

        return Task.CompletedTask;
    }
}
