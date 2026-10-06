using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Features.Categories.Commands.DeleteCategory;

/// <summary>
/// Handler xử lý DeleteCategoryCommand, kiểm tra ràng buộc nghiệp vụ không còn Recipe và thực hiện soft delete.
/// </summary>
public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<DeleteCategoryCommandHandler> _logger;

    public DeleteCategoryCommandHandler(
        IApplicationDbContext dbContext,
        ILogger<DeleteCategoryCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Bắt đầu xử lý yêu cầu xóa danh mục Id: {Id}", request.Id);

        // 1. Kiểm tra danh mục có tồn tại và chưa bị soft delete không
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);

        if (category is null)
        {
            _logger.LogWarning("Xóa danh mục thất bại: Không tìm thấy danh mục Id {Id}.", request.Id);
            throw new NotFoundException($"Không tìm thấy danh mục với Id '{request.Id}'.");
        }

        // 2. Kiểm tra ràng buộc nghiệp vụ bắt buộc (SRS FR-CAT-005):
        // Nếu còn bất kỳ Recipe nào (chưa bị xóa mềm) đang thuộc Category này -> CHẶN XÓA
        var hasActiveRecipes = await _dbContext.Recipes
            .AnyAsync(r => r.CategoryId == request.Id && !r.IsDeleted, cancellationToken);

        if (hasActiveRecipes)
        {
            _logger.LogWarning("Xóa danh mục bị từ chối: Danh mục Id {Id} vẫn còn công thức liên kết.", request.Id);
            throw new ConflictException(
                "Không thể xóa danh mục vì vẫn còn công thức nấu ăn liên kết với danh mục này.",
                "CATEGORY_DELETE_HAS_RECIPES");
        }

        // 3. Thực hiện Soft Delete an toàn
        category.IsDeleted = true;
        category.DeletedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Xóa mềm danh mục thành công. Id: {Id}", category.Id);

        return true;
    }
}
