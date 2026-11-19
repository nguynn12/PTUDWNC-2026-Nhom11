namespace CulinaryBlog.Infrastructure.Services;

using CulinaryBlog.Application.Common.Interfaces;
using ImageMagick;
using Microsoft.Extensions.Logging;
using Amazon.S3;
using Amazon.S3.Model;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

/// <summary>Sinh ảnh WebP thumbnail và medium từ ảnh đã được kiểm tra.</summary>
public class ImageProcessingService(ILogger<ImageProcessingService> logger) : IImageProcessingService
{
    public Task<(ImageVariantResult? Thumbnail, ImageVariantResult? Medium)> ProcessVariantsAsync(
        Stream sourceStream, string originalFileName, CancellationToken cancellationToken = default)
    {
        if (!sourceStream.CanSeek || sourceStream.Length == 0)
            return Task.FromResult<(ImageVariantResult?, ImageVariantResult?)>((null, null));
        var position = sourceStream.Position;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            sourceStream.Position = 0;
            using var image = new MagickImage(sourceStream);
            var name = Path.GetFileNameWithoutExtension(originalFileName);
            using var thumb = image.Clone();
            thumb.Resize(new MagickGeometry(300, 300) { FillArea = true });
            thumb.Crop(300, 300, Gravity.Center);
            thumb.ResetPage();
            thumb.Quality = 80;
            using var medium = image.Clone();
            medium.Resize(new MagickGeometry(800, 600));
            medium.Quality = 85;
            return Task.FromResult<(ImageVariantResult?, ImageVariantResult?)>((
                Encode(thumb, name + "_thumb.webp"), Encode(medium, name + "_medium.webp")));
        }
        catch (OperationCanceledException) { throw; }
        catch (MagickException ex)
        {
            logger.LogWarning(ex, "Không thể xử lý ảnh {FileName}", originalFileName);
            return Task.FromResult<(ImageVariantResult?, ImageVariantResult?)>((null, null));
        }
        finally { sourceStream.Position = position; }
    }

    private static ImageVariantResult Encode(IMagickImage<byte> image, string name)
    {
        var stream = new MemoryStream();
        image.Write(stream, MagickFormat.WebP);
        stream.Position = 0;
        return new(stream, name, "image/webp", checked((int)image.Width), checked((int)image.Height));
    }
}

/// <summary>Hangfire lưu các tác vụ ảnh trong PostgreSQL và thử lại khi dịch vụ tạm lỗi.</summary>
public sealed class RecipeImageJobs(
    IBackgroundJobClient jobs,
    IApplicationDbContext context,
    IAmazonS3 s3,
    IOptions<MinioSettings> settings,
    IImageProcessingService processor,
    IFileStorageService storage,
    ICacheInvalidator cache) : IRecipeImageJobs
{
    public void EnqueueResize(Guid imageId) => jobs.Enqueue<RecipeImageJobs>(job => job.ResizeAsync(imageId, CancellationToken.None));
    public void EnqueueDelete(string fileUrl) => jobs.Enqueue<RecipeImageJobs>(job => job.DeleteAsync(fileUrl, CancellationToken.None));

    [AutomaticRetry(Attempts = 3)]
    public Task DeleteAsync(string fileUrl, CancellationToken ct) => storage.DeleteAsync(fileUrl, ct);

    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(120)]
    public async Task ResizeAsync(Guid imageId, CancellationToken ct)
    {
        var image = await context.RecipeImages.FirstOrDefaultAsync(i => i.Id == imageId, ct);
        if (image == null || image.IsDeleted || (image.ThumbnailUrl != null && image.MediumUrl != null)) return;
        var bucket = settings.Value.BucketName;
        var path = new Uri(image.OriginalUrl).AbsolutePath.TrimStart('/');
        var prefix = bucket + "/";
        if (!path.StartsWith(prefix, StringComparison.Ordinal)) throw new InvalidOperationException("Ảnh gốc không thuộc bucket đã cấu hình.");
        var key = Uri.UnescapeDataString(path[prefix.Length..]);
        using var response = await s3.GetObjectAsync(new GetObjectRequest { BucketName = bucket, Key = key }, ct);
        using var input = new MemoryStream();
        await response.ResponseStream.CopyToAsync(input, ct);
        input.Position = 0;
        var (thumbnail, medium) = await processor.ProcessVariantsAsync(input, Path.GetFileName(key), ct);
        using var thumbnailStream = thumbnail?.Stream;
        using var mediumStream = medium?.Stream;
        if (thumbnail == null || medium == null) throw new InvalidDataException("Không thể tạo đủ các kích thước ảnh.");
        // Ghi từng URL để retry không tải lại biến thể đã lưu thành công.
        if (image.ThumbnailUrl == null)
        {
            image.ThumbnailUrl = await storage.UploadAsync(thumbnail.Stream, thumbnail.FileName, thumbnail.ContentType, $"recipes/{image.RecipeId}", ct);
            await context.SaveChangesAsync(ct);
        }
        if (image.MediumUrl == null)
        {
            image.MediumUrl = await storage.UploadAsync(medium.Stream, medium.FileName, medium.ContentType, $"recipes/{image.RecipeId}", ct);
            await context.SaveChangesAsync(ct);
        }
        var recipe = await context.Recipes.AsNoTracking().FirstOrDefaultAsync(r => r.Id == image.RecipeId, ct);
        await cache.InvalidateRecipeAsync(image.RecipeId, recipe?.Slug, ct);
    }

    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(60)]
    public async Task ReconcileAsync(CancellationToken ct)
    {
        var deleted = await context.RecipeImages.IgnoreQueryFilters().AsNoTracking()
            .Where(i => i.IsDeleted).Select(i => new { i.OriginalUrl, i.ThumbnailUrl, i.MediumUrl }).ToListAsync(ct);
        foreach (var image in deleted)
            foreach (var url in new[] { image.OriginalUrl, image.ThumbnailUrl, image.MediumUrl })
                if (!string.IsNullOrWhiteSpace(url)) await DeleteAsync(url, ct);
        var prefix = settings.Value.PublicUrl.TrimEnd('/') + "/" + settings.Value.BucketName + "/";
        var pending = await context.RecipeImages.AsNoTracking()
            .Where(i => i.OriginalUrl.StartsWith(prefix) && (i.ThumbnailUrl == null || i.MediumUrl == null))
            .Select(i => i.Id).ToListAsync(ct);
        foreach (var id in pending) EnqueueResize(id);
    }
}
