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

