using CulinaryBlog.API.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Xunit;

namespace CulinaryBlog.UnitTests.Api;

/// <summary>Lab 5 – tài liệu API: security scheme Bearer và biểu tượng khoá cho endpoint cần đăng nhập.</summary>
public sealed class OpenApiSecurityTests
{
    private static OpenApiOperationTransformerContext Context(OpenApiDocument document, params object[] metadata) => new()
    {
        DocumentName = "v1",
        Document = document,
        ApplicationServices = new NullServiceProvider(),
        Description = new ApiDescription
        {
            ActionDescriptor = new ActionDescriptor { EndpointMetadata = [.. metadata] },
        },
    };

    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    [Fact]
    public async Task DocumentTransformer_KhaiBaoSchemeBearerJwt()
    {
        var document = new OpenApiDocument();

        await new BearerSecuritySchemeTransformer().TransformAsync(
            document,
            new OpenApiDocumentTransformerContext { DocumentName = "v1", DescriptionGroups = [], ApplicationServices = new NullServiceProvider() },
            TestContext.Current.CancellationToken);

        Assert.NotNull(document.Components?.SecuritySchemes);
        var scheme = document.Components.SecuritySchemes[OpenApiSetup.BearerSchemeName];
        Assert.Equal(SecuritySchemeType.Http, scheme.Type);
        Assert.Equal("bearer", scheme.Scheme);
        Assert.Equal("JWT", scheme.BearerFormat);
        Assert.Equal("Culinary Blog API", document.Info?.Title);
    }

    [Fact]
    public async Task OperationTransformer_EndpointCanDangNhap_GanBearer()
    {
        var operation = new OpenApiOperation();

        await new BearerSecurityRequirementTransformer().TransformAsync(
            operation, Context(new OpenApiDocument(), new AuthorizeAttribute("Admin")), TestContext.Current.CancellationToken);

        var requirement = Assert.Single(operation.Security!);
        Assert.Equal(OpenApiSetup.BearerSchemeName, Assert.Single(requirement.Keys).Reference.Id);
        Assert.True(operation.Responses!.ContainsKey("401"));
    }

    [Fact]
    public async Task OperationTransformer_EndpointCongKhai_KhongGanBearer()
    {
        var operation = new OpenApiOperation();

        await new BearerSecurityRequirementTransformer().TransformAsync(
            operation, Context(new OpenApiDocument()), TestContext.Current.CancellationToken);

        Assert.True(operation.Security is null || operation.Security.Count == 0);
    }

    [Fact]
    public void RequiresAuthorization_AllowAnonymousThangAuthorize()
    {
        Assert.True(OpenApiSetup.RequiresAuthorization([new AuthorizeAttribute()]));
        Assert.True(OpenApiSetup.RequiresAuthorization([new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()]));
        Assert.False(OpenApiSetup.RequiresAuthorization([new AuthorizeAttribute(), new AllowAnonymousAttribute()]));
        Assert.False(OpenApiSetup.RequiresAuthorization([]));
    }
}
