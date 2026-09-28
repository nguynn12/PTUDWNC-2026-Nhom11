using CulinaryBlog.Application.Features.RecipeIngredients.Services;
using CulinaryBlog.Application.Features.RecipeSteps.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRecipeIngredientService, RecipeIngredientService>();
        services.AddScoped<IRecipeStepService, RecipeStepService>();

        return services;
    }
}

