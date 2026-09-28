using CulinaryBlog.Application.Features.RecipeIngredients.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRecipeIngredientService, RecipeIngredientService>();

        return services;
    }
}

