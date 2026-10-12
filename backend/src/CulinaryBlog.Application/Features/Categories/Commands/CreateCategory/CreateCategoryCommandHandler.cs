using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Helpers;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Categories.DTOs;
using CulinaryBlog.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Features.Categories.Commands.CreateCategory;

/// <summary>
/// Handler xử lý CreateCategoryCommand, thực hiện tạo mới danh mục và sinh slug chuẩn SEO.
/// </summary>
public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<CreateCategoryCommandHandler> _logger;

    public CreateCategoryCommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CreateCategoryCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var trimmedName = request.Name.Trim();
        _logger.LogInformation("Bắt đầu xử lý tạo danh mục mới với tên: '{Name}'", trimmedName);

        // 1. Kiểm tra Name đã tồn tại trong database chưa (không phân biệt hoa/thường)
        var nameExists = await _dbContext.Categories
            .AnyAsync(c => c.Name.ToLower() == trimmedName.ToLower() && !c.IsDeleted, cancellationToken);

        if (nameExists)
        {
            _logger.LogWarning("Tạo danh mục thất bại: Tên danh mục '{Name}' đã tồn tại.", trimmedName);
            throw new ConflictException($"Danh mục với tên '{trimmedName}' đã tồn tại trong hệ thống.", "CATEGORY_NAME_EXISTS");
        }

        // 2. Tự động sinh Slug tiếng Việt chuẩn SEO từ Name
        var baseSlug = SlugHelper.Generate(trimmedName);
        if (string.IsNullOrWhiteSpace(baseSlug))
        {
            baseSlug = "danh-muc";
        }

        // 3. Đảm bảo Slug là duy nhất, nếu trùng thì thêm suffix số (-2, -3...) theo quy định SRS FR-CAT-003
        var finalSlug = baseSlug;
        var suffix = 2;
        while (await _dbContext.Categories.AnyAsync(c => c.Slug == finalSlug && !c.IsDeleted, cancellationToken))
        {
            finalSlug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        // 4. Khởi tạo thực thể Category mới
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = trimmedName,
            Slug = finalSlug,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
            OrderIndex = request.OrderIndex,
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tạo danh mục thành công. Id: {Id}, Slug: '{Slug}'", category.Id, category.Slug);

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            OrderIndex = category.OrderIndex,
            RecipeCount = 0,
            CreatedAt = category.CreatedAt
        };
    }
}
