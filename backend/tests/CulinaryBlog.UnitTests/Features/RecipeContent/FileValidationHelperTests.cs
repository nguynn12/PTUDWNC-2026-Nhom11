namespace CulinaryBlog.UnitTests.Features.RecipeContent;

using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
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
        // Arrange: ảnh JPEG thật 1x1 (ảnh rất nhỏ cũng phải được giải mã)
        var jpegBytes = EncodeImage("jpg");
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
        // Arrange: ảnh PNG thật 1x1
        var pngBytes = EncodeImage("png");
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
        // Arrange: ảnh WebP thật 1x1
        var webpBytes = EncodeImage("webp");
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
        // Arrange: hộp ftyp AVIF hợp lệ (kích thước 24 byte, brand avif + brand tương thích mif1)
        var avifBytes = BuildAvif(majorBrand: "avif", compatibleBrand: "mif1");
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

    [Fact]
    public void ValidateImage_PngMagicBytesButGarbageBody_ShouldFailDecode()
    {
        // Đúng chữ ký PNG nhưng thân là rác, dưới 64 byte: trước đây bị bỏ qua bước giải mã.
        var fake = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52 };
        using var stream = new MemoryStream(fake);

        var (isValid, errorMessage) = FileValidationHelper.ValidateImage(stream, "fake.png", "image/png", fake.Length);

        Assert.False(isValid);
        Assert.Contains("giải mã", errorMessage);
    }

    [Fact]
    public void ValidateImage_AvifBoxSizeExceedsFile_ShouldReturnInvalid()
    {
        var bytes = BuildAvif("avif", "mif1");
        bytes[3] = 0xFF; // kích thước hộp ftyp lớn hơn tệp
        using var stream = new MemoryStream(bytes);

        var (isValid, _) = FileValidationHelper.ValidateImage(stream, "bad.avif", "image/avif", bytes.Length);

        Assert.False(isValid);
    }

    [Fact]
    public void ValidateImage_AvifWithoutAvifBrand_ShouldReturnInvalid()
    {
        // Đúng "ftyp" nhưng brand là heic: magic bytes cũ chỉ so 3 byte đầu brand nên vẫn lọt nếu bắt đầu "avi".
        var bytes = BuildAvif("heic", "mif1");
        using var stream = new MemoryStream(bytes);

        var (isValid, _) = FileValidationHelper.ValidateImage(stream, "x.avif", "image/avif", bytes.Length);

        Assert.False(isValid);
    }

    [Fact]
    public void ValidateImage_ExceedsPixelLimit_ShouldReturnInvalid()
    {
        using var image = new Image<Rgba32>(7000, 7000); // 49 triệu điểm ảnh > 40 triệu
        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        ms.Position = 0;

        var (isValid, _) = FileValidationHelper.ValidateImage(ms, "huge.png", "image/png", ms.Length);

        Assert.False(isValid);
    }

    private static byte[] EncodeImage(string ext)
    {
        using var image = new Image<Rgba32>(1, 1);
        using var ms = new MemoryStream();
        switch (ext)
        {
            case "jpg": image.SaveAsJpeg(ms); break;
            case "png": image.SaveAsPng(ms); break;
            default: image.SaveAsWebp(ms); break;
        }
        return ms.ToArray();
    }

    /// <summary>Hộp ftyp 24 byte: size, "ftyp", major brand, minor version, 1 compatible brand.</summary>
    private static byte[] BuildAvif(string majorBrand, string compatibleBrand)
    {
        var bytes = new byte[24];
        bytes[3] = 24;
        "ftyp"u8.CopyTo(bytes.AsSpan(4));
        System.Text.Encoding.ASCII.GetBytes(majorBrand).CopyTo(bytes, 8);
        System.Text.Encoding.ASCII.GetBytes(compatibleBrand).CopyTo(bytes, 16);
        return bytes;
    }
}
