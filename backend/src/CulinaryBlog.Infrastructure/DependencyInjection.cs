using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Infrastructure.Identity;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Repositories;
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

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, Services.CurrentUserService>();

        // Repository riêng của module Auth (mở rộng IRepository<RefreshToken> dùng chung).
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

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

        // Tra cứu thông tin công khai của user (tên, ảnh tác giả) cho tầng Application —
        // thay cho navigation Recipe.Author đã bỏ (RESOLVED-CONFLICTS.md mục D7).
        services.AddScoped<IUserQueryService, UserQueryService>();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IEmailService, CulinaryBlog.Infrastructure.Services.MockEmailService>();
        services.Configure<Services.ClientAppSettings>(configuration.GetSection(Services.ClientAppSettings.SectionName));
        services.AddScoped<IAccountEmailSender, Services.AccountEmailSender>();
        services.AddScoped<IClientInfoService, Services.ClientInfoService>();

        var jwtSettings = new JwtSettings();
        configuration.Bind(JwtSettings.SectionName, jwtSettings);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(jwtSettings.Secret))
            };
        });

        services.AddAuthorization(AuthorizationPolicies.Configure);

        return services;
    }
}
