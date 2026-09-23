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
        await RecipeDetailSeeder.SeedAsync(dbContext);
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
            // Recipe không còn navigation Author (mục D7) — lấy tác giả bằng subquery theo AuthorId,
            // JSON trả về giữ nguyên dạng { Id, DisplayName, Email }.
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
