namespace CulinaryBlog.UnitTests.Features.RecipeContent;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CulinaryBlog.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

/// <summary>
/// Unit Tests kiểm tra dịch vụ lưu trữ tệp tin FileStorageService.
/// Tuân thủ SRS v1.2.0 (FR-FILE-001, FR-FILE-002) về upload, xóa idempotent và phòng chống path traversal.
/// </summary>
public class FileStorageServiceTests : IDisposable
{
    private readonly string _testStorageDir;
    private readonly FileStorageService _storageService;

    public FileStorageServiceTests()
    {
        _testStorageDir = Path.Combine(Path.GetTempPath(), "culinary_blog_test_storage_" + Guid.NewGuid());
        Directory.CreateDirectory(_testStorageDir);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("FileStorage:BasePath", _testStorageDir)
            })
            .Build();

        _storageService = new FileStorageService(config, NullLogger<FileStorageService>.Instance);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testStorageDir))
        {
            try
            {
                Directory.Delete(_testStorageDir, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public async Task UploadAsync_ValidStream_ShouldStoreFileAndReturnPublicUrl()
    {
        // Arrange
        var content = "Sample image byte content for testing";
        var bytes = Encoding.UTF8.GetBytes(content);
        using var stream = new MemoryStream(bytes);

        // Act
        var url = await _storageService.UploadAsync(
            stream,
            "dish.jpg",
            "image/jpeg",
            "recipes/test-recipe",
            TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(url);
        Assert.StartsWith("/uploads/recipes/test-recipe/", url);
        Assert.EndsWith(".jpg", url);

        // Kiểm tra file vật lý đã được tạo và nội dung khớp
        var relativePath = url.Substring("/uploads/".Length);
        var fullPath = Path.Combine(_testStorageDir, relativePath);
        Assert.True(File.Exists(fullPath));

        var savedContent = await File.ReadAllTextAsync(fullPath, TestContext.Current.CancellationToken);
        Assert.Equal(content, savedContent);
    }

    [Fact]
    public async Task DeleteAsync_ExistingFile_ShouldDeletePhysicalFile()
    {
        // Arrange: Tạo file trước
        var folder = Path.Combine(_testStorageDir, "recipes");
        Directory.CreateDirectory(folder);
        var filePath = Path.Combine(folder, "to_delete.png");
        await File.WriteAllTextAsync(filePath, "dummy content", TestContext.Current.CancellationToken);
        Assert.True(File.Exists(filePath));

        // Act
        await _storageService.DeleteAsync("/uploads/recipes/to_delete.png", TestContext.Current.CancellationToken);

        // Assert
        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public async Task DeleteAsync_NonExistingFile_ShouldBeIdempotent_AndNotThrow()
    {
        // Act & Assert (FR-FILE-002: Không gây ngoại lệ nếu object không tồn tại)
        await _storageService.DeleteAsync("/uploads/recipes/non_existing_file.png", TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DeleteAsync_PathTraversalAttempt_ShouldBeBlockedSafely()
    {
        // Arrange: Giả lập tấn công path traversal nhằm xóa file bên ngoài thư mục cho phép
        var maliciousUrl = "/uploads/../../system32/cmd.exe";

        // Act & Assert (Không throw lỗi và không crash server)
        await _storageService.DeleteAsync(maliciousUrl, TestContext.Current.CancellationToken);
    }
}
