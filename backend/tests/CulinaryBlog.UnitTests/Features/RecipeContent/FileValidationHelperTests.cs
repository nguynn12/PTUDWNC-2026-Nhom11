namespace CulinaryBlog.UnitTests.Features.RecipeContent;

using System.IO;
using CulinaryBlog.Application.Common.Helpers;
using Xunit;

/// <summary>
/// Unit Tests kiểm tra tính toàn vẹn và độ an toàn của FileValidationHelper.
/// Tuân thủ SRS v1.2.0 (FR-RCP-008, FR-FILE-001) về kiểm tra Magic Bytes và định dạng ảnh.
/// </summary>
public class FileValidationHelperTests
{
    [Fact]
    public void ValidateImage_ValidJpegStream_ShouldReturnValid()
    {
        // Arrange: Header chuẩn của JPEG: FF D8 FF E0
        var jpegBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x01, 0x00, 0x60 };
        using var stream = new MemoryStream(jpegBytes);

        // Act
        var (isValid, errorMessage) = FileValidationHelper.ValidateImage(stream, "test_food.jpg", "image/jpeg", jpegBytes.Length);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    [Fact]
    public void ValidateImage_ValidPngStream_ShouldReturnValid()
    {
        // Arrange: Header chuẩn của PNG: 89 50 4E 47 0D 0A 1A 0A
        var pngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52 };
        using var stream = new MemoryStream(pngBytes);

        // Act
        var (isValid, errorMessage) = FileValidationHelper.ValidateImage(stream, "dish.png", "image/png", pngBytes.Length);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    [Fact]
    public void ValidateImage_ValidWebPStream_ShouldReturnValid()
    {
        // Arrange: Header chuẩn của WebP: 'RIFF' + 4 bytes size + 'WEBP'
        var webpBytes = new byte[] {
            0x52, 0x49, 0x46, 0x46, // RIFF
            0x24, 0x00, 0x00, 0x00, // Size
            0x57, 0x45, 0x42, 0x50, // WEBP
            0x56, 0x50, 0x38, 0x20  // VP8 
        };
        using var stream = new MemoryStream(webpBytes);

        // Act
        var (isValid, errorMessage) = FileValidationHelper.ValidateImage(stream, "photo.webp", "image/webp", webpBytes.Length);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    [Fact]
    public void ValidateImage_ValidAvifStream_ShouldReturnValid()
    {
        // Arrange: Header chuẩn của AVIF: 4 bytes box size + 'ftyp' (66 74 79 70) + 'avif' (61 76 69 66)
        var avifBytes = new byte[] {
            0x00, 0x00, 0x00, 0x20, // Box size
            0x66, 0x74, 0x79, 0x70, // ftyp
            0x61, 0x76, 0x69, 0x66, // avif
            0x00, 0x00, 0x00, 0x00
        };
        using var stream = new MemoryStream(avifBytes);

        // Act
        var (isValid, errorMessage) = FileValidationHelper.ValidateImage(stream, "photo.avif", "image/avif", avifBytes.Length);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    [Fact]
    public void ValidateImage_Exceeding5MB_ShouldReturnInvalid()
    {
        // Arrange: Dung lượng 5MB + 1 byte
        var stream = new MemoryStream(new byte[16]);
        long oversized = (5 * 1024 * 1024) + 1;

        // Act
        var (isValid, errorMessage) = FileValidationHelper.ValidateImage(stream, "huge.jpg", "image/jpeg", oversized);

        // Assert
        Assert.False(isValid);
        Assert.Contains("vượt quá giới hạn", errorMessage);
    }

    [Fact]
    public void ValidateImage_EmptyFile_ShouldReturnInvalid()
    {
        // Arrange
        using var stream = new MemoryStream();

        // Act
        var (isValid, errorMessage) = FileValidationHelper.ValidateImage(stream, "empty.png", "image/png", 0);

        // Assert
        Assert.False(isValid);
        Assert.Contains("rỗng", errorMessage);
    }

    [Fact]
    public void ValidateImage_DisallowedExtension_ShouldReturnInvalid()
    {
        // Arrange
        var bytes = new byte[] { 0x4D, 0x5A, 0x90, 0x00 }; // Header EXE
        using var stream = new MemoryStream(bytes);

        // Act
        var (isValid, errorMessage) = FileValidationHelper.ValidateImage(stream, "script.exe", "application/x-msdownload", bytes.Length);

        // Assert
        Assert.False(isValid);
        Assert.Contains("không được hỗ trợ", errorMessage);
    }

    [Fact]
    public void ValidateImage_FakeExtensionWithMaliciousContent_ShouldFailMagicBytes()
    {
        // Arrange: File đuôi .jpg nhưng nội dung thực tế là text file
        var fakeJpgBytes = System.Text.Encoding.UTF8.GetBytes("<?php echo 'malicious payload'; ?>");
        using var stream = new MemoryStream(fakeJpgBytes);

        // Act
        var (isValid, errorMessage) = FileValidationHelper.ValidateImage(stream, "fake.jpg", "image/jpeg", fakeJpgBytes.Length);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Magic Bytes", errorMessage);
    }
}
