namespace CulinaryBlog.Application.Features.RecipeImages.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Helpers;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.RecipeImages.DTOs;
using CulinaryBlog.Application.Features.RecipeImages.Validators;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Hiện thực nghiệp vụ quản lý hình ảnh công thức nấu ăn.
/// Tuân thủ SRS v1.2.0 (FR-RCP-008, FR-FILE-001/002), RESOLVED-CONFLICTS.md (E6, E7)
/// và kế hoạch sửa lỗi SEC-01, SEC-02, API-03, API-08, API-18.
/// </summary>
public class RecipeImageService : IRecipeImageService
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly IImageProcessingService? _imageProcessingService;
    private readonly ICacheInvalidator? _cacheInvalidator;

    public RecipeImageService(
        IApplicationDbContext context,
        IFileStorageService fileStorageService,
        IImageProcessingService? imageProcessingService = null,
        ICacheInvalidator? cacheInvalidator = null)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _imageProcessingService = imageProcessingService;
        _cacheInvalidator = cacheInvalidator;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RecipeImageDto>> GetImagesAsync(
        Guid recipeId,
        string? currentUserId = null,
        bool isAdmin = false,
        CancellationToken cancellationToken = default)
    {
        var recipe = await _context.Recipes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);

        if (recipe == null)
        {
            throw new NotFoundException(nameof(Recipe), recipeId, "RECIPE_NOT_FOUND");
        }

        // SEC-02: Kiểm tra quyền xem nếu Recipe không phải Published
        if (recipe.Status != RecipeStatus.Published)
        {
            if (!isAdmin && (string.IsNullOrWhiteSpace(currentUserId) ||
                !string.Equals(recipe.AuthorId, currentUserId, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ForbiddenException("Bạn không có quyền xem hình ảnh của công thức chưa xuất bản.");
            }
        }

        var images = await _context.RecipeImages
            .AsNoTracking()
            .Where(i => i.RecipeId == recipeId)
            .OrderByDescending(i => i.IsPrimary)
            .ThenBy(i => i.OrderIndex)
            .Select(i => MapToDto(i))
            .ToListAsync(cancellationToken);

        return images;
    }

    /// <inheritdoc />
    public async Task<RecipeImageDto> UploadImageAsync(
        Guid recipeId,
        Stream stream,
        string fileName,
        string contentType,
        long length,
        string? altText,
        bool? isPrimary,
        string? currentUserId,
        bool isAdmin,
        uint? concurrencyToken = null,
        CancellationToken cancellationToken = default)
    {
        var recipe = await _context.Recipes
            .FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);

        if (recipe == null)
        {
            throw new NotFoundException(nameof(Recipe), recipeId, "RECIPE_NOT_FOUND");
        }

        VerifyRecipeOwnerOrAdmin(recipe, currentUserId, isAdmin);
        await VerifyConcurrencyTokenAsync(recipe, concurrencyToken, cancellationToken);

        // Kiểm tra tính hợp lệ toàn diện của file ảnh (dung lượng, MIME, Magic Bytes)
        var (isValid, errorMessage) = FileValidationHelper.ValidateImage(stream, fileName, contentType, length);
        if (!isValid)
        {
            throw new BadRequestException(errorMessage ?? "Tệp tin hình ảnh không hợp lệ.");
        }

        if (!string.IsNullOrWhiteSpace(altText) && altText.Length > 200)
        {
            throw new ValidationException(nameof(altText), "Văn bản thay thế (AltText) không được vượt quá 200 ký tự.");
        }

        var existingImages = await _context.RecipeImages
            .Where(i => i.RecipeId == recipeId)
            .ToListAsync(cancellationToken);

        // Ảnh đầu tiên mặc định là primary; hoặc khi client chỉ định isPrimary = true (FR-RCP-008, E6)
        bool shouldBePrimary = (isPrimary == true) || (existingImages.Count == 0);

        if (shouldBePrimary)
        {
            // Trong cùng transaction: set toàn bộ ảnh khác của Recipe về IsPrimary = false (E6)
            var currentPrimaries = existingImages.Where(i => i.IsPrimary).ToList();
            if (currentPrimaries.Count > 0)
            {
                foreach (var existing in currentPrimaries)
                {
                    existing.IsPrimary = false;
                    existing.UpdatedAt = DateTimeOffset.UtcNow;
                }
                // Lưu trạng thái hạ cờ primary trước để tránh vi phạm Unique Index (API-18)
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        int orderIndex = existingImages.Count > 0 ? existingImages.Max(i => i.OrderIndex) + 1 : 0;

        // 1. Lưu trữ file gốc vào Object Storage (MinIO)
        var publicUrl = await _fileStorageService.UploadAsync(
            stream,
            fileName,
            contentType,
            $"recipes/{recipeId}",
            cancellationToken);

        string? thumbnailUrl = null;
        string? mediumUrl = null;

        // 2. Tự động sinh thumbnail (300x300) và medium (800x600) bằng ImageSharp (FR-JOB-002)
        if (_imageProcessingService != null)
        {
            try
            {
                var (thumb, med) = await _imageProcessingService.ProcessVariantsAsync(stream, fileName, cancellationToken);
                if (thumb != null)
                {
                    using (thumb.Stream)
                    {
                        thumbnailUrl = await _fileStorageService.UploadAsync(
                            thumb.Stream,
                            thumb.FileName,
                            thumb.ContentType,
                            $"recipes/{recipeId}",
                            cancellationToken);
                    }
                }

                if (med != null)
                {
                    using (med.Stream)
                    {
                        mediumUrl = await _fileStorageService.UploadAsync(
                            med.Stream,
                            med.FileName,
                            med.ContentType,
                            $"recipes/{recipeId}",
                            cancellationToken);
                    }
                }
            }
            catch
            {
                // Nếu xử lý variant thất bại: ảnh gốc vẫn hiển thị an toàn, chỉ thiếu thumbnail/medium theo SRS FR-JOB-002
            }
        }

        var image = new RecipeImage
        {
            Id = Guid.NewGuid(),
            RecipeId = recipeId,
            OriginalUrl = publicUrl,
            ThumbnailUrl = thumbnailUrl,
            MediumUrl = mediumUrl,
            AltText = altText?.Trim(),
            IsPrimary = shouldBePrimary,
            OrderIndex = orderIndex,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.RecipeImages.Add(image);
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // API-08: Hủy cache Recipe/Search khi thêm ảnh
        if (_cacheInvalidator != null)
        {
            await _cacheInvalidator.InvalidateRecipeAsync(recipeId, recipe.Slug, cancellationToken);
        }

        return MapToDto(image);
    }

    /// <inheritdoc />
    public async Task<RecipeImageDto> UpdateImageAsync(
        Guid recipeId,
        Guid imageId,
        UpdateRecipeImageRequest request,
        string? currentUserId,
        bool isAdmin,
        uint? concurrencyToken = null,
        CancellationToken cancellationToken = default)
    {
        RecipeImageValidator.ValidateUpdate(request);

        var recipe = await _context.Recipes
            .FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);

        if (recipe == null)
        {
            throw new NotFoundException(nameof(Recipe), recipeId, "RECIPE_NOT_FOUND");
        }

        VerifyRecipeOwnerOrAdmin(recipe, currentUserId, isAdmin);
        await VerifyConcurrencyTokenAsync(recipe, concurrencyToken, cancellationToken);

        var image = await _context.RecipeImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.RecipeId == recipeId, cancellationToken);

        if (image == null)
        {
            throw new NotFoundException(nameof(RecipeImage), imageId, "RECIPE_IMAGE_NOT_FOUND");
        }

        if (request.AltText != null)
        {
            image.AltText = request.AltText.Trim();
        }

        if (request.OrderIndex.HasValue)
        {
            image.OrderIndex = request.OrderIndex.Value;
        }

        if (request.IsPrimary.HasValue)
        {
            if (request.IsPrimary.Value && !image.IsPrimary)
            {
                // Khi isPrimary = true, set toàn bộ ảnh khác của Recipe về IsPrimary = false trước (E6, API-18)
                var otherPrimaryImages = await _context.RecipeImages
                    .Where(i => i.RecipeId == recipeId && i.Id != imageId && i.IsPrimary)
                    .ToListAsync(cancellationToken);

                foreach (var other in otherPrimaryImages)
                {
                    other.IsPrimary = false;
                    other.UpdatedAt = DateTimeOffset.UtcNow;
                }

                // Lưu hạ cờ trước để không trùng unique index
                await _context.SaveChangesAsync(cancellationToken);

                image.IsPrimary = true;
            }
            else if (!request.IsPrimary.Value && image.IsPrimary)
            {
                image.IsPrimary = false;

                // Tự động chuyển ảnh có orderIndex nhỏ nhất thành primary để luôn có ít nhất 1 ảnh đại diện
                var nextPrimary = await _context.RecipeImages
                    .Where(i => i.RecipeId == recipeId && i.Id != imageId)
                    .OrderBy(i => i.OrderIndex)
                    .FirstOrDefaultAsync(cancellationToken);

                if (nextPrimary != null)
                {
                    nextPrimary.IsPrimary = true;
                    nextPrimary.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }
        }

        image.UpdatedAt = DateTimeOffset.UtcNow;
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // API-08: Hủy cache Recipe/Search khi sửa ảnh
        if (_cacheInvalidator != null)
        {
            await _cacheInvalidator.InvalidateRecipeAsync(recipeId, recipe.Slug, cancellationToken);
        }

        return MapToDto(image);
    }

    /// <inheritdoc />
    public async Task DeleteImageAsync(
        Guid recipeId,
        Guid imageId,
        string? currentUserId,
        bool isAdmin,
        uint? concurrencyToken = null,
        CancellationToken cancellationToken = default)
    {
        var recipe = await _context.Recipes
            .FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);

        if (recipe == null)
        {
            throw new NotFoundException(nameof(Recipe), recipeId, "RECIPE_NOT_FOUND");
        }

        VerifyRecipeOwnerOrAdmin(recipe, currentUserId, isAdmin);
        await VerifyConcurrencyTokenAsync(recipe, concurrencyToken, cancellationToken);

        var imageToDelete = await _context.RecipeImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.RecipeId == recipeId, cancellationToken);

        if (imageToDelete == null)
        {
            throw new NotFoundException(nameof(RecipeImage), imageId, "RECIPE_IMAGE_NOT_FOUND");
        }

        bool wasPrimary = imageToDelete.IsPrimary;

        // Xóa mềm theo quy chuẩn BaseEntity
        imageToDelete.IsDeleted = true;
        imageToDelete.DeletedAt = DateTimeOffset.UtcNow;

        // Khi xóa ảnh primary, ảnh có orderIndex nhỏ nhất còn lại trở thành primary (FR-RCP-008)
        if (wasPrimary)
        {
            var nextPrimary = await _context.RecipeImages
                .Where(i => i.RecipeId == recipeId && i.Id != imageId)
                .OrderBy(i => i.OrderIndex)
                .FirstOrDefaultAsync(cancellationToken);

            if (nextPrimary != null)
            {
                nextPrimary.IsPrimary = true;
                nextPrimary.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        // Xóa file gốc và các biến thể Thumbnail, Medium khỏi storage (FR-FILE-002 idempotent)
        await _fileStorageService.DeleteAsync(imageToDelete.OriginalUrl, cancellationToken);
        if (!string.IsNullOrWhiteSpace(imageToDelete.ThumbnailUrl))
        {
            await _fileStorageService.DeleteAsync(imageToDelete.ThumbnailUrl, cancellationToken);
        }
        if (!string.IsNullOrWhiteSpace(imageToDelete.MediumUrl))
        {
            await _fileStorageService.DeleteAsync(imageToDelete.MediumUrl, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // API-08: Hủy cache Recipe/Search khi xóa ảnh
        if (_cacheInvalidator != null)
        {
            await _cacheInvalidator.InvalidateRecipeAsync(recipeId, recipe.Slug, cancellationToken);
        }
    }

    private static void VerifyRecipeOwnerOrAdmin(Recipe recipe, string? currentUserId, bool isAdmin)
    {
        if (isAdmin)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(currentUserId) ||
            !string.Equals(recipe.AuthorId, currentUserId, StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException("Bạn không có quyền chỉnh sửa nội dung công thức của tác giả khác.");
        }
    }

    private async Task VerifyConcurrencyTokenAsync(Recipe recipe, uint? concurrencyToken, CancellationToken cancellationToken)
    {
        if (concurrencyToken.HasValue)
        {
            var currentXmin = await _context.Recipes
                .Where(r => r.Id == recipe.Id)
                .Select(r => EF.Property<uint>(r, "xmin"))
                .FirstOrDefaultAsync(cancellationToken);

            if (currentXmin != 0 && concurrencyToken.Value != currentXmin)
            {
                throw new ConflictException("Công thức đã bị thay đổi bởi người dùng khác. Vui lòng tải lại trang.", "RECIPE_CONCURRENCY_CONFLICT");
            }
        }
    }

    private static RecipeImageDto MapToDto(RecipeImage image)
    {
        return new RecipeImageDto
        {
            Id = image.Id,
            RecipeId = image.RecipeId,
            OriginalUrl = image.OriginalUrl,
            MediumUrl = image.MediumUrl,
            ThumbnailUrl = image.ThumbnailUrl,
            AltText = image.AltText,
            IsPrimary = image.IsPrimary,
            OrderIndex = image.OrderIndex,
            CreatedAt = image.CreatedAt
        };
    }
}
