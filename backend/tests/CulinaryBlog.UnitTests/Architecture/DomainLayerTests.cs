using CulinaryBlog.Domain.Common;
using CulinaryBlog.Infrastructure.Identity;
using Xunit;

namespace CulinaryBlog.UnitTests.Architecture;

/// <summary>
/// Kiểm tra tự động NFR-MAINT-004: project Domain chỉ dùng .NET BCL, không phụ thuộc thư viện
/// hạ tầng (Identity, EF Core, Npgsql, ASP.NET Core) hay tầng ngoài (Application, Infrastructure).
/// Quyết định liên quan: docs/decisions/RESOLVED-CONFLICTS.md mục D7.
/// </summary>
public sealed class DomainLayerTests
{
    private static readonly string[] ForbiddenAssemblyPrefixes =
    [
        "Microsoft.AspNetCore",
        "Microsoft.Extensions.Identity",
        "Microsoft.EntityFrameworkCore",
        "Npgsql",
        "CulinaryBlog.Application",
        "CulinaryBlog.Infrastructure",
    ];

    [Fact]
    public void Domain_KhongThamChieuThuVienHaTangHoacTangNgoai()
    {
        var violations = typeof(BaseEntity).Assembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .Where(name => ForbiddenAssemblyPrefixes.Any(p => name.StartsWith(p, StringComparison.Ordinal)))
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void ApplicationUser_NamOInfrastructure_KhongNamODomain()
    {
        Assert.DoesNotContain(typeof(BaseEntity).Assembly.GetTypes(), t => t.Name == "ApplicationUser");
        Assert.Equal("CulinaryBlog.Infrastructure.Identity", typeof(ApplicationUser).Namespace);
    }
}
