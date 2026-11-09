namespace CulinaryBlog.UnitTests.Features.RecipeContent;

using System.IO;
using System.Threading.Tasks;
using CulinaryBlog.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

/// <summary>
/// Unit Tests kiểm tra dịch vụ biến đổi kích thước ảnh ImageProcessingService.
/// Đáp ứng yêu cầu Lab 8 và FR-JOB-002: sinh thumbnail 300x300 và medium 800x600.
/// </summary>
public class ImageProcessingServiceTests
{
    private readonly ImageProcessingService _service;

    public ImageProcessingServiceTests()
    {
        _service = new ImageProcessingService(NullLogger<ImageProcessingService>.Instance);
    }

    private static MemoryStream CreateValidPngStream(int width = 400, int height = 300)
    {
        var ms = new MemoryStream();
        using var image = new Image<Rgba32>(width, height);
        image.SaveAsPng(ms);
        ms.Position = 0;
        return ms;
    }

    [Fact]
    public async Task ProcessVariantsAsync_ValidImage_ShouldGenerateThumbnailAndMediumVariants()
    {
        // Arrange
        using var sourceStream = CreateValidPngStream(1200, 900);

        // Act
        var (thumb, medium) = await _service.ProcessVariantsAsync(
            sourceStream,
            "delicious_pho.png",
            TestContext.Current.CancellationToken);

        // Assert Thumbnail (300x300, WebP)
        Assert.NotNull(thumb);
        Assert.Equal(300, thumb.Width);
        Assert.Equal(300, thumb.Height);
        Assert.Equal("image/webp", thumb.ContentType);
        Assert.Equal("delicious_pho_thumb.webp", thumb.FileName);
        Assert.True(thumb.Stream.Length > 0);

        // Assert Medium (800x600 max proportionally, WebP)
        Assert.NotNull(medium);
        Assert.True(medium.Width <= 800);
        Assert.True(medium.Height <= 600);
        Assert.Equal("image/webp", medium.ContentType);
        Assert.Equal("delicious_pho_medium.webp", medium.FileName);
        Assert.True(medium.Stream.Length > 0);
    }

    [Fact]
    public async Task ProcessVariantsAsync_EmptyStream_ShouldReturnNullsGracefully()
    {
        // Arrange
        using var emptyStream = new MemoryStream();

        // Act
        var (thumb, medium) = await _service.ProcessVariantsAsync(
            emptyStream,
            "empty.png",
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(thumb);
        Assert.Null(medium);
    }

    [Fact]
    public async Task ProcessVariantsAsync_AvifExtension_ShouldReturnNullsWithoutThrowing()
    {
        // Arrange
        using var stream = new MemoryStream(new byte[32]);

        // Act
        var (thumb, medium) = await _service.ProcessVariantsAsync(
            stream,
            "sample.avif",
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(thumb);
        Assert.Null(medium);
    }

    [Fact]
    public async Task ProcessVariantsAsync_CorruptStream_ShouldHandleExceptionGracefully()
    {
        // Arrange: Dữ liệu nhị phân ngẫu nhiên không phải ảnh
        var corruptBytes = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };
        using var corruptStream = new MemoryStream(corruptBytes);

        // Act
        var (thumb, medium) = await _service.ProcessVariantsAsync(
            corruptStream,
            "corrupt.jpg",
            TestContext.Current.CancellationToken);

        // Assert: Không ném lỗi làm crash hệ thống, trả về null để luồng chính tiếp tục
        Assert.Null(thumb);
        Assert.Null(medium);
    }
}
