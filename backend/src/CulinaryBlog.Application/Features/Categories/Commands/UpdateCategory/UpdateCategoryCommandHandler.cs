using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Categories.DTOs;
using CulinaryBlog.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Features.Categories.Commands.UpdateCategory;

/// <summary>
/// Handler xử lý UpdateCategoryCommand, cập nhật thông tin và bảo toàn Slug theo SRS C7.
/// </summary>
public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<UpdateCategoryCommandHandler> _logger;

    public UpdateCategoryCommandHandler(
        IApplicationDbContext dbContext,
        ILogger<UpdateCategoryCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Bắt đầu xử lý cập nhật danh mục Id: {Id}", request.Id);

        // 1. Tìm danh mục theo Id (chưa bị soft delete)
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);

        if (category is null)
        {
            _logger.LogWarning("Cập nhật thất bại: Không tìm thấy danh mục Id {Id}.", request.Id);
            throw new NotFoundException($"Không tìm thấy danh mục với Id '{request.Id}'.");
        }

        var trimmedName = request.Name.Trim();

        // 2. Kiểm tra xem Name mới có bị trùng với danh mục khác trong hệ thống không
        var nameConflict = await _dbContext.Categories
            .AnyAsync(c => c.Id != request.Id && c.Name.ToLower() == trimmedName.ToLower() && !c.IsDeleted, cancellationToken);

        if (nameConflict)
        {
            _logger.LogWarning("Cập nhật thất bại: Tên '{Name}' đã bị trùng với danh mục khác.", trimmedName);
            throw new ConflictException($"Tên danh mục '{trimmedName}' đã được sử dụng bởi một danh mục khác.", "CATEGORY_NAME_EXISTS");
        }

        // 3. Cập nhật các trường thông tin (Lưu ý: Slug giữ nguyên không đổi theo quy định SRS C7 & FR-CAT-004)
        category.Name = trimmedName;
        category.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        category.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
        category.OrderIndex = request.OrderIndex;
        category.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        // 4. Lấy số lượng công thức Published đang thuộc danh mục
        var recipeCount = await _dbContext.Recipes
            .CountAsync(r => r.CategoryId == category.Id && r.Status == RecipeStatus.Published && !r.IsDeleted, cancellationToken);

        _logger.LogInformation("Cập nhật danh mục thành công. Id: {Id}", category.Id);

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            OrderIndex = category.OrderIndex,
            RecipeCount = recipeCount,
            CreatedAt = category.CreatedAt
        };
    }
}
