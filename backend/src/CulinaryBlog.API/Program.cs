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

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<AuthExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddPresentation();
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Exception → RFC 7807 Problem Details (AuthExceptionHandler + GlobalExceptionHandler)
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseRateLimiter();

// Tự động Migrate và Seed dữ liệu mẫu (User/Role/Recipe) ở môi trường Development
if (app.Environment.IsDevelopment())
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

    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapAdminUserEndpoints();

var api = app.MapGroup("/api/v1");
api.MapCategoryEndpoints();
api.MapRecipeEndpoints();
api.MapRecipeLifecycleEndpoints();

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

// Định tuyến Health Checks ở cả root và api group (đáp ứng SRS Mục 8.4: /health, /health/live)
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
