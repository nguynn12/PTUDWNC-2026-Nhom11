namespace CulinaryBlog.Infrastructure.Services;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

/// <summary>
/// Dịch vụ xử lý biến đổi kích thước hình ảnh sử dụng SixLabors.ImageSharp.
/// Đáp ứng yêu cầu FR-JOB-002 (Image Resize 300x300 thumbnail và 800x600 medium).
/// </summary>
public class ImageProcessingService : IImageProcessingService
{
    private readonly ILogger<ImageProcessingService> _logger;

    public ImageProcessingService(ILogger<ImageProcessingService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<(ImageVariantResult? Thumbnail, ImageVariantResult? Medium)> ProcessVariantsAsync(
        Stream sourceStream,
        string originalFileName,
        CancellationToken cancellationToken = default)
    {
        if (sourceStream == null || sourceStream.Length == 0)
        {
            return (null, null);
        }

        var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (ext == ".avif")
        {
            // Định dạng AVIF chưa hỗ trợ decode trực tiếp bởi SixLabors core mà không có custom codec
            _logger.LogInformation("Bỏ qua resize ImageSharp cho định dạng AVIF: {FileName}", originalFileName);
            return (null, null);
        }

        long originalPos = 0;
        if (sourceStream.CanSeek)
        {
            originalPos = sourceStream.Position;
            sourceStream.Position = 0;
        }

        try
        {
            // Tạo bản sao stream để ImageSharp có thể đọc độc lập
            using var copyStream = new MemoryStream();
            await sourceStream.CopyToAsync(copyStream, cancellationToken);
            copyStream.Position = 0;

            using var image = await Image.LoadAsync(copyStream, cancellationToken);
            var baseName = Path.GetFileNameWithoutExtension(originalFileName);

            // 1. Tạo Thumbnail (300x300, Crop cân đối trung tâm)
            ImageVariantResult? thumbnailResult = null;
            try
            {
                using var thumbClone = image.Clone(ctx => ctx.Resize(new ResizeOptions
                {
                    Size = new Size(300, 300),
                    Mode = ResizeMode.Crop
                }));

                var thumbStream = new MemoryStream();
                await thumbClone.SaveAsWebpAsync(thumbStream, new WebpEncoder { Quality = 80 }, cancellationToken);
                thumbStream.Position = 0;

                thumbnailResult = new ImageVariantResult(
                    thumbStream,
                    $"{baseName}_thumb.webp",
                    "image/webp",
                    thumbClone.Width,
                    thumbClone.Height);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi tạo ảnh Thumbnail 300x300 cho {FileName}", originalFileName);
            }

            // 2. Tạo Medium (800x600, giữ nguyên tỉ lệ khung hình Max)
            ImageVariantResult? mediumResult = null;
            try
            {
                using var mediumClone = image.Clone(ctx => ctx.Resize(new ResizeOptions
                {
                    Size = new Size(800, 600),
                    Mode = ResizeMode.Max
                }));

                var mediumStream = new MemoryStream();
                await mediumClone.SaveAsWebpAsync(mediumStream, new WebpEncoder { Quality = 85 }, cancellationToken);
                mediumStream.Position = 0;

                mediumResult = new ImageVariantResult(
                    mediumStream,
                    $"{baseName}_medium.webp",
                    "image/webp",
                    mediumClone.Width,
                    mediumClone.Height);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi tạo ảnh Medium 800x600 cho {FileName}", originalFileName);
            }

            return (thumbnailResult, mediumResult);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không thể giải mã hoặc xử lý ảnh ImageSharp cho tệp: {FileName}. Giữ nguyên ảnh gốc.", originalFileName);
            return (null, null);
        }
        finally
        {
            if (sourceStream.CanSeek)
            {
                sourceStream.Position = originalPos;
            }
        }
    }
}
