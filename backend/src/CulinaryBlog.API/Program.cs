using CulinaryBlog.Application;
using CulinaryBlog.Infrastructure;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

// Áp dụng migration EF Core + seed dữ liệu mẫu (module Auth/User) — chỉ chạy ở
// Development, KHÔNG chạy ở Production (SRS.md — seed dữ liệu chỉ phục vụ dev/test).
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var initialiser = scope.ServiceProvider
        .GetRequiredService<CulinaryBlog.Infrastructure.Persistence.Seeding.ApplicationDbContextInitialiser>();

    await initialiser.InitialiseAsync();
    await initialiser.SeedAsync();
}

var api = app.MapGroup("/api/v1");
api.MapGet("/", () => Results.Ok(new
{
    name = "Culinary Blog API",
    version = "v1"
}));

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

