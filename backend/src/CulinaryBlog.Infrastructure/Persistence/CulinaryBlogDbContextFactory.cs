namespace CulinaryBlog.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

/// <summary>
/// Factory khởi tạo DbContext lúc design-time để hỗ trợ các lệnh EF Core CLI (migrations, database update).
/// </summary>
public class CulinaryBlogDbContextFactory : IDesignTimeDbContextFactory<CulinaryBlogDbContext>
{
    public CulinaryBlogDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CulinaryBlogDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=culinaryblog;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString, b =>
        {
            b.MigrationsAssembly(typeof(CulinaryBlogDbContext).Assembly.FullName);
        });

        return new CulinaryBlogDbContext(optionsBuilder.Options);
    }
}
