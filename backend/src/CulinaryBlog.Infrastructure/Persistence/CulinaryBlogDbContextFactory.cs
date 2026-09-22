using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CulinaryBlog.Infrastructure.Persistence;

/// <summary>
/// Factory dùng bởi công cụ dotnet-ef khi chạy migration từ command line
/// (dotnet ef migrations add / database update) — KHÔNG dùng lúc runtime của ứng
/// dụng (lúc runtime, DbContext được đăng ký qua DependencyInjection.AddInfrastructure
/// như bình thường). Cần vì Infrastructure là class library, không tự khởi động được
/// WebApplicationBuilder để đọc appsettings như CulinaryBlog.API.
///
/// Ưu tiên đọc connection string từ biến môi trường ConnectionStrings__DefaultConnection
/// (đúng quy ước cấu hình của .NET); nếu không có, fallback đọc appsettings.Development.json
/// của CulinaryBlog.API (chỉ đúng khi chạy `dotnet ef` từ thư mục
/// backend/src/CulinaryBlog.Infrastructure — xem README lệnh mẫu).
/// </summary>
public class CulinaryBlogDbContextFactory : IDesignTimeDbContextFactory<CulinaryBlogDbContext>
{
    public CulinaryBlogDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var apiProjectPath = Path.Combine("..", "CulinaryBlog.API");
            var basePath = Directory.Exists(apiProjectPath) ? apiProjectPath : Directory.GetCurrentDirectory();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Fallback cuối cùng — khớp appsettings.Development.json hiện tại của CulinaryBlog.API.
        connectionString ??= "Host=localhost;Port=5432;Database=culinary_blog_dev;Username=culinary_blog;Password=culinary_blog_dev";

        var optionsBuilder = new DbContextOptionsBuilder<CulinaryBlogDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new CulinaryBlogDbContext(optionsBuilder.Options);
    }
}
