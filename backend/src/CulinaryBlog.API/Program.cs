using CulinaryBlog.API;
using CulinaryBlog.API.Configuration;
using CulinaryBlog.API.Endpoints;
using CulinaryBlog.API.Filters;
using CulinaryBlog.API.Middlewares;
using CulinaryBlog.API.OpenApi;
using CulinaryBlog.Application;
using CulinaryBlog.Domain.Constants;
using CulinaryBlog.Infrastructure;
using CulinaryBlog.Infrastructure.Jobs;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Seeding;
using CulinaryBlog.Infrastructure.Seeders;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

// Lab 8 (TV3): Thiết lập Serilog có cấu trúc (Console JSON, File rolling theo ngày, Seq Development)
builder.Host.UseSerilog((context, services, configuration) =>
{
    var seqServerUrl = context.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341";
    var isDev = context.HostingEnvironment.IsDevelopment();

    configuration
        .MinimumLevel.Is(isDev ? LogEventLevel.Debug : LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", isDev ? LogEventLevel.Information : LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "CulinaryBlog.API")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);

    // Sink Console: JSON có cấu trúc trong Prod, định dạng màu trong Dev
    if (isDev)
    {
        configuration.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <CorrelationId:{CorrelationId}> {Properties:j}{NewLine}{Exception}");
    }
    else
    {
        configuration.WriteTo.Console(new CompactJsonFormatter());
    }

    // Sink File: rolling daily trong logs/app-.log (lưu trữ 30 ngày)
    configuration.WriteTo.File(
        path: "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}");

    // Sink Seq (Development/Production)
    if (!string.IsNullOrWhiteSpace(seqServerUrl))
    {
        configuration.WriteTo.Seq(seqServerUrl, restrictedToMinimumLevel: LogEventLevel.Debug);
    }
});

// Lab 5 (TV1): tài liệu OpenAPI có Bearer JWT, API versioning theo URL, CORS allowlist cho frontend.
builder.Services.AddCulinaryOpenApi();
builder.Services.AddCulinaryApiVersioning();
builder.Services.AddFrontendCors(builder.Configuration, builder.Environment);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<AuthExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddPresentation(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
    await next();
});

// CORS đặt đầu pipeline: preflight OPTIONS trả ngay (không tính rate limit) và header CORS
// vẫn có trên response lỗi (401/429/500) để trình duyệt đọc được Problem Details.
app.UseCors(CorsSetup.PolicyName);

// Lab 8 (TV3): CorrelationId & Serilog Request Logging (FR-OBS-002)
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms (UserId: {UserId}, CorrelationId: {CorrelationId})";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("UserId", httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "Anonymous");
        if (httpContext.Response.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
        {
            diagnosticContext.Set("CorrelationId", correlationId.ToString());
        }
        diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
    };
});

// Exception → RFC 7807 Problem Details (AuthExceptionHandler + GlobalExceptionHandler)
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseRateLimiter();

// Cấu hình OpenAPI và giao diện kiểm thử tương tác Scalar UI (thay thế Swagger)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Culinary Blog API - PTUDWNC Nhom 11")
               .WithTheme(ScalarTheme.Moon)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
               // Ô "Authentication" của Scalar dùng scheme Bearer; token được nhớ khi tải lại trang.
               .AddPreferredSecuritySchemes([OpenApiSetup.BearerSchemeName])
               .EnablePersistentAuthentication();
    });

    app.MapGet("/scalar", () => Results.Redirect("/scalar/v1"));
    app.MapGet("/docs", () => Results.Redirect("/scalar/v1"));

    try
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        if (dbContext.Database.IsRelational())
        {
            await initialiser.InitialiseAsync();
            await initialiser.SeedAsync();
            await CategorySeeder.SeedAsync(dbContext);
            await RecipeSeeder.SeedAsync(dbContext);
            await RecipeDetailSeeder.SeedAsync(dbContext);
        }
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Chưa thể kết nối đến cơ sở dữ liệu PostgreSQL để migrate/seed (vui lòng bật Docker nếu cần dữ liệu thật). API và Scalar UI vẫn khởi động bình thường.");
    }
}

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<SystemAuditLoggingMiddleware>();

// Lab 8 (TV3): Hangfire Dashboard & Recurring Job dọn rác 30 ngày (FR-JOB-004)
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [new HangfireAdminAuthorizationFilter(allowAnonymousInDev: true)],
        DashboardTitle = "Culinary Blog - Background Jobs"
    });

    try
    {
        RecurringJob.AddOrUpdate<PurgeExpiredRecipesJob>(
            "purge-expired-recipes-30days",
            job => job.ExecuteAsync(CancellationToken.None),
            "0 3 * * *");

        RecurringJob.AddOrUpdate<GenerateSitemapJob>(
            "generate-sitemap-daily",
            job => job.RunAsync(CancellationToken.None),
            "0 2 * * *");
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Không thể đăng ký recurring job Hangfire (có thể PostgreSQL chưa sẵn sàng).");
    }
}

// Nhóm /api/v1 có phiên bản (Asp.Versioning, SRS 5.2). Các module gắn endpoint vào nhóm này.
var api = app.MapVersionedApi()
    .RequireRateLimiting(CulinaryBlog.API.RateLimiting.GeneralRateLimiting.PolicyName)
    .AddEndpointFilter(async (context, next) =>
    {
        context.HttpContext.Response.Headers["X-RateLimit-Limit"] = CulinaryBlog.API.RateLimiting.GeneralRateLimiting.PermitLimit.ToString();
        return await next(context);
    });

// 1. Phân hệ Xác thực & Quản lý người dùng (Thành viên 1)
api.MapAuthEndpoints();
api.MapAdminUserEndpoints();
api.MapSystemLogEndpoints();

// 2. Phân hệ Danh mục, Tra cứu & Tìm kiếm FTS (Thành viên 2)
api.MapCategoryEndpoints();
api.MapRecipeEndpoints();
app.MapSitemapEndpoints();

// 3. Phân hệ Vòng đời công thức & Thùng rác (Thành viên 3)
api.MapRecipeLifecycleEndpoints();

// 4. Phân hệ Chi tiết công thức, Nguyên liệu, Bước làm & Hình ảnh (Thành viên 4)
api.MapRecipeIngredientEndpoints();
api.MapRecipeStepEndpoints();
api.MapRecipeImageEndpoints();

api.MapGet("/", () => Results.Ok(new
{
    name = "Culinary Blog API",
    version = "v1"
}));

// Tổng quan dữ liệu (có email người dùng) => chỉ Admin được xem.
api.MapGet("/overview", async (CulinaryBlogDbContext dbContext) =>
{
    var usersCount = await dbContext.Users.CountAsync();
    var categoriesCount = await dbContext.Categories.CountAsync();
    var recipesCount = await dbContext.Recipes.CountAsync();
    var ingredientsCount = await dbContext.RecipeIngredients.CountAsync();
    var stepsCount = await dbContext.RecipeSteps.CountAsync();
    var imagesCount = await dbContext.RecipeImages.CountAsync();

    var sampleWithRelations = await dbContext.Recipes
        .Include(r => r.Category)
        .Include(r => r.Ingredients)
        .Include(r => r.Steps)
        .Include(r => r.Images)
        .Take(5)
        .Select(r => new
        {
            r.Id,
            r.Title,
            r.Slug,
            Category = r.Category != null ? new { r.Category.Id, r.Category.Name } : null,
            Author = dbContext.Users
                .Where(u => u.Id == r.AuthorId)
                .Select(u => new { u.Id, u.DisplayName, u.Email })
                .FirstOrDefault(),
            IngredientsCount = r.Ingredients.Count,
            StepsCount = r.Steps.Count,
            ImagesCount = r.Images.Count
        })
        .ToListAsync();

    return Results.Ok(new
    {
        totalUsers = usersCount,
        totalCategories = categoriesCount,
        totalRecipes = recipesCount,
        totalIngredients = ingredientsCount,
        totalSteps = stepsCount,
        totalImages = imagesCount,
        sampleWithRelations
    });
})
.WithTags("System")
.RequireAuthorization(Roles.Admin);

// Định tuyến Health Checks ở cả root và api group (đáp ứng SRS Mục 8.4: /health, /health/live, /health/ready)
app.MapHealthEndpoints();

app.Run();

public partial class Program;
