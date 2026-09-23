using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Seeding;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not configured.");

        services.AddDbContext<CulinaryBlogDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<CulinaryBlogDbContext>());

        // ── ASP.NET Core Identity ───────────────────────────────────────────────
        // AddIdentityCore (không phải AddIdentity đầy đủ): API dùng JWT thuần, không cần
        // SignInManager/cookie auth scheme của MVC. Chính sách mật khẩu & lockout theo
        // SRS.md FR-AUTH-001/002.
        services.AddDataProtection();

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                // Mật khẩu tối thiểu 8 ký tự, có hoa/thường/số/ký tự đặc biệt — FR-AUTH-001
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;

                // 5 lần sai → khoá 15 phút — FR-AUTH-002 (A3)
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;

                // Email duy nhất, case-insensitive — FR-AUTH-001
                options.User.RequireUniqueEmail = true;

                // KHÔNG bật RequireConfirmedEmail ở đây: FR-AUTH-002 cho phép login dù
                // email chưa xác nhận; việc chặn chỉ áp dụng khi Publish Recipe, xử lý
                // riêng qua policy "VerifiedAuthor" (Domain.Constants.Policies).
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<CulinaryBlogDbContext>()
            .AddDefaultTokenProviders(); // Cần cho FR-AUTH-008/009 (token xác nhận email)

        // Áp dụng migration + seed dữ liệu mẫu lúc khởi động (chỉ gọi ở môi trường
        // Development — xem Program.cs). Xem ApplicationDbContextInitialiser để biết
        // phạm vi seed (Auth/User) và cách thành viên khác cắm seeder Category/Recipe.
        services.AddScoped<ApplicationDbContextInitialiser>();

        return services;
    }
}
