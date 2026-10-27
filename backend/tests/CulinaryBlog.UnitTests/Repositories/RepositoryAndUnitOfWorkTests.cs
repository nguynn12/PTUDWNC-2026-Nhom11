namespace CulinaryBlog.UnitTests.Repositories;

using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// Unit Tests kiểm tra khai báo và đăng ký DI của Repository và Unit of Work.
/// Tuân thủ yêu cầu tối thiểu Lab 3: Hoàn thành Repository và Unit of Work.
/// </summary>
public class RepositoryAndUnitOfWorkTests
{
    [Fact]
    public void DependencyInjection_ShouldRegisterRepositoriesAndUnitOfWork()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", "Host=localhost;Database=test_db;Username=test;Password=test")
            })
            .Build();

        // Act
        services.AddLogging();
        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IUnitOfWork>());
        Assert.NotNull(serviceProvider.GetService<IRecipeIngredientRepository>());
        Assert.NotNull(serviceProvider.GetService<IRecipeStepRepository>());
        Assert.NotNull(serviceProvider.GetService<IRecipeImageRepository>());
        Assert.NotNull(serviceProvider.GetService<IRepository<Recipe>>());
    }
}
