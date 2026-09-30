using CulinaryBlog.API.Endpoints;
using CulinaryBlog.API.Infrastructure;
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
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

// Cấu hình OpenAPI và giao diện kiểm thử tương tác Scalar UI (thay thế Swagger)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Culinary Blog API - Recipe Content & Media")
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

var api = app.MapGroup("/api/v1");
api.MapGet("/", () => Results.Ok(new
{
    name = "Culinary Blog API",
    version = "v1"
}));

// Đăng ký các endpoints của phân hệ Recipe Content và Media (Thành viên 4)
api.MapRecipeIngredientEndpoints();
api.MapRecipeStepEndpoints();
api.MapRecipeImageEndpoints();

api.MapGet("/recipes", async (CulinaryBlogDbContext dbContext) =>
{
    var count = await dbContext.Recipes.CountAsync();
    var sample = await dbContext.Recipes.Take(10).ToListAsync();
    return Results.Ok(new { total = count, sample });
});

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
        .Include(r => r.Author)
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
            Author = r.Author != null ? new { r.Author.Id, r.Author.DisplayName, r.Author.Email } : null,
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

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.MapGet("/health/database", async (
    CulinaryBlogDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

    return canConnect
        ? Results.Ok(new { status = "Healthy", dependency = "PostgreSQL" })
        : Results.Problem(
            title: "Database is unavailable",
            statusCode: StatusCodes.Status503ServiceUnavailable);
});

app.Run();

public partial class Program;
