namespace CulinaryBlog.Application.Common.Interfaces;

using CulinaryBlog.Domain.Entities;

/// <summary>
/// Giao diện Unit of Work quản lý tính nhất quán của giao dịch (Transactions) theo yêu cầu tối thiểu của Lab 3.
/// Tập hợp các Repository và đảm bảo toàn bộ thao tác thêm, sửa, xóa chạy trong cùng transaction.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IRepository<Recipe> Recipes { get; }

    IRecipeIngredientRepository RecipeIngredients { get; }

    IRecipeStepRepository RecipeSteps { get; }

    IRecipeImageRepository RecipeImages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
