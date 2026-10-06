using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Infrastructure.Identity;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Repositories;
using CulinaryBlog.Infrastructure.Persistence.Seeding;
using CulinaryBlog.Infrastructure.Repositories;
using CulinaryBlog.Infrastructure.Services;
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

        // ── Repository và Unit of Work (Yêu cầu chung Lab 3) ─────────────────────
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IRecipeIngredientRepository, RecipeIngredientRepository>();
        services.AddScoped<IRecipeStepRepository, RecipeStepRepository>();
        services.AddScoped<IRecipeImageRepository, RecipeImageRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // ── Dịch vụ lưu trữ tệp tin (Thành viên 4) ───────────────────────────────
        services.AddScoped<IFileStorageService, FileStorageService>();

        // ── ASP.NET Core Identity ───────────────────────────────────────────────
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
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<CulinaryBlogDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<ApplicationDbContextInitialiser>();
        services.AddScoped<IUserQueryService, UserQueryService>();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IEmailService, CulinaryBlog.Infrastructure.Services.MockEmailService>();
        services.Configure<Services.ClientAppSettings>(configuration.GetSection(Services.ClientAppSettings.SectionName));
        services.AddScoped<IAccountEmailSender, Services.AccountEmailSender>();
        services.AddScoped<IClientInfoService, Services.ClientInfoService>();

        // FR-AUTH-003: xác minh Google ID token
        services.Configure<GoogleAuthSettings>(configuration.GetSection(GoogleAuthSettings.SectionName));
        services.AddSingleton<IGoogleTokenValidator>(provider => new GoogleTokenValidator(
            provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<GoogleAuthSettings>>(),
            GoogleTokenValidator.CreateGoogleConfigurationManager(),
            provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<GoogleTokenValidator>>()));

        var jwtSettings = new JwtSettings();
        configuration.Bind(JwtSettings.SectionName, jwtSettings);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options => JwtBearerSetup.Configure(options, jwtSettings));

        services.AddAuthorization(AuthorizationPolicies.Configure);

        return services;
    }
}
