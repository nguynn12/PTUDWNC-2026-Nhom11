using CulinaryBlog.Application.Common.Behaviors;
using CulinaryBlog.Application.Features.RecipeImages.Services;
using CulinaryBlog.Application.Features.RecipeIngredients.Services;
using CulinaryBlog.Application.Features.RecipeSteps.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<IRecipeIngredientService, RecipeIngredientService>();
        services.AddScoped<IRecipeStepService, RecipeStepService>();
        services.AddScoped<IRecipeImageService, RecipeImageService>();

        return services;
    }
}
