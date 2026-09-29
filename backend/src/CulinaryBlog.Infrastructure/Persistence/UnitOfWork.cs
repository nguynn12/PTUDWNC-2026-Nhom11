namespace CulinaryBlog.Infrastructure.Persistence;

using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

/// <summary>
/// Hiện thực Unit of Work quản lý transaction và các repository theo yêu cầu tối thiểu Lab 3.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly CulinaryBlogDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    private IRepository<Recipe>? _recipes;
    private IRecipeIngredientRepository? _recipeIngredients;
    private IRecipeStepRepository? _recipeSteps;
    private IRecipeImageRepository? _recipeImages;

    public UnitOfWork(CulinaryBlogDbContext context)
    {
        _context = context;
    }

    public IRepository<Recipe> Recipes => _recipes ??= new Repository<Recipe>(_context);
    public IRecipeIngredientRepository RecipeIngredients => _recipeIngredients ??= new RecipeIngredientRepository(_context);
    public IRecipeStepRepository RecipeSteps => _recipeSteps ??= new RecipeStepRepository(_context);
    public IRecipeImageRepository RecipeImages => _recipeImages ??= new RecipeImageRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            return;
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
    }
}
