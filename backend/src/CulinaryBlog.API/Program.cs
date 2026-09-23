using CulinaryBlog.Application;
using CulinaryBlog.Infrastructure;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Seeding;
using CulinaryBlog.Infrastructure.Seeders;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

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
    }
}

var api = app.MapGroup("/api/v1");
api.MapGet("/", () => Results.Ok(new
{
    name = "Culinary Blog API",
    version = "v1"
}));

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

    var sampleWithRelations = await dbContext.Recipes
        .Include(r => r.Category)
        .Include(r => r.Author)
        .Take(5)
        .Select(r => new
        {
            r.Id,
            r.Title,
            r.Slug,
            Category = r.Category != null ? new { r.Category.Id, r.Category.Name } : null,
            Author = r.Author != null ? new { r.Author.Id, r.Author.DisplayName, r.Author.Email } : null
        })
        .ToListAsync();

    return Results.Ok(new
    {
        totalUsers = usersCount,
        totalCategories = categoriesCount,
        totalRecipes = recipesCount,
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
