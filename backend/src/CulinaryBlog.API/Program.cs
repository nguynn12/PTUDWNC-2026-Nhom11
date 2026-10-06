using CulinaryBlog.API;
using CulinaryBlog.API.Endpoints;
using CulinaryBlog.API.Middlewares;
using CulinaryBlog.Application;
using CulinaryBlog.Infrastructure;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Seeding;
using CulinaryBlog.Infrastructure.Seeders;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<AuthExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddPresentation();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

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
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
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

// 1. Phân hệ Xác thực & Quản lý người dùng (Thành viên 1)
app.MapAuthEndpoints();
app.MapAdminUserEndpoints();

var api = app.MapGroup("/api/v1");

// 2. Phân hệ Danh mục, Tra cứu & Tìm kiếm FTS (Thành viên 2)
api.MapCategoryEndpoints();
api.MapRecipeEndpoints();

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
});

// Định tuyến Health Checks ở cả root và api group (đáp ứng SRS Mục 8.4: /health, /health/live, /health/ready)
app.MapHealthEndpoints();

app.MapGet("/health/ready", async (
    CulinaryBlogDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

    return canConnect
        ? Results.Ok(new { status = "Ready", checks = new { database = "Healthy" } })
        : Results.Problem(
            title: "Service is not ready to receive traffic",
            statusCode: StatusCodes.Status503ServiceUnavailable);
});

app.Run();

public partial class Program;
