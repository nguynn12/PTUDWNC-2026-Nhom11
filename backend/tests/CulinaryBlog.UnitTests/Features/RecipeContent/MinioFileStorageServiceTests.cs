namespace CulinaryBlog.UnitTests.Features.RecipeContent;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using CulinaryBlog.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

/// <summary>
/// Unit Tests kiểm tra dịch vụ MinioFileStorageService theo SRS FR-FILE-001, FR-FILE-002.
/// </summary>
public class MinioFileStorageServiceTests : IDisposable
{
    private sealed class DeleteFailureClient(bool missing) : AmazonS3Client("dummy", "dummy",
        new AmazonS3Config { ServiceURL = "http://localhost:9000", ForcePathStyle = true })
    {
        public override Task<DeleteObjectResponse> DeleteObjectAsync(DeleteObjectRequest request,
            CancellationToken cancellationToken = default) => Task.FromException<DeleteObjectResponse>(missing
                ? new AmazonS3Exception("Missing") { StatusCode = System.Net.HttpStatusCode.NotFound }
                : new IOException("Storage unavailable"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteAsync_OnlyMissingObjectsAreSuccess_OtherFailuresReachRetry(bool missing)
    {
        using var client = new DeleteFailureClient(missing);
        var service = new MinioFileStorageService(client, Options.Create(_settings),
            NullLogger<MinioFileStorageService>.Instance, _fallbackService);
        if (missing) await service.DeleteAsync("recipes/missing.jpg");
        else await Assert.ThrowsAsync<IOException>(() => service.DeleteAsync("recipes/existing.jpg"));
    }

    private readonly string _testTempDir;
    private readonly FileStorageService _fallbackService;
    private readonly MinioSettings _settings;
    private readonly MinioFileStorageService _minioService;

    public MinioFileStorageServiceTests()
    {
        _testTempDir = Path.Combine(Path.GetTempPath(), "culinary_minio_unit_test_" + Guid.NewGuid());
        Directory.CreateDirectory(_testTempDir);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("FileStorage:BasePath", _testTempDir)
            })
            .Build();

        _fallbackService = new FileStorageService(config, NullLogger<FileStorageService>.Instance);

        _settings = new MinioSettings
        {
            Enabled = false, // Disabled để test luồng fallback an toàn trong môi trường Unit Test không có MinIO container
            Endpoint = "http://localhost:9000",
            PublicUrl = "http://localhost:9000",
            BucketName = "culinary-blog"
        };

        var options = Options.Create(_settings);
        
        // Mock client dummy
        var s3Config = new AmazonS3Config
        {
            ServiceURL = _settings.Endpoint,
            ForcePathStyle = true
        };
        var dummyClient = new AmazonS3Client("dummy", "dummy", s3Config);

        _minioService = new MinioFileStorageService(
            dummyClient,
            options,
            NullLogger<MinioFileStorageService>.Instance,
            _fallbackService);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testTempDir))
        {
            try
            {
                Directory.Delete(_testTempDir, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Theory]
    [InlineData("http://localhost:9000/culinary-blog/recipes/123/photo.webp", "recipes/123/photo.webp")]
    [InlineData("https://storage.example.com/culinary-blog/avatars/user.jpg", "avatars/user.jpg")]
    [InlineData("/culinary-blog/recipes/test/food.png", "recipes/test/food.png")]
    [InlineData("culinary-blog/recipes/test/food.png", "recipes/test/food.png")]
    [InlineData("recipes/test/food.png", "recipes/test/food.png")]
    public void ExtractObjectKey_ShouldCorrectlyExtractPathWithoutBucket(string inputUrl, string expectedKey)
    {
        // Act
        var result = _minioService.ExtractObjectKey(inputUrl);

        // Assert
        Assert.Equal(expectedKey, result);
    }

    [Fact]
    public void ExtractObjectKey_EmptyString_ShouldReturnEmpty()
    {
        // Act & Assert
        Assert.Equal(string.Empty, _minioService.ExtractObjectKey(""));
        Assert.Equal(string.Empty, _minioService.ExtractObjectKey("   "));
    }

    [Fact]
    public async Task UploadAsync_WhenMinioDisabled_ShouldFallbackToLocalStorage()
    {
        // Arrange
        var content = "Fallback upload content test";
        var bytes = Encoding.UTF8.GetBytes(content);
        using var stream = new MemoryStream(bytes);

        // Act
        var resultUrl = await _minioService.UploadAsync(
            stream,
            "dish.jpg",
            "image/jpeg",
            "recipes/unit-test",
            TestContext.Current.CancellationToken);

        // Assert: Trả về URL của local fallback
        Assert.NotNull(resultUrl);
        Assert.StartsWith("/uploads/recipes/unit-test/", resultUrl);
        Assert.EndsWith(".jpg", resultUrl);
    }

    [Fact]
    public async Task UploadAsync_WhenMinioEnabledAndFails_ShouldThrowServiceUnavailableException()
    {
        // MEDIA-03: Khi MinIO bật mà lỗi kết nối, phải ném ServiceUnavailableException (503), không fallback local
        var enabledSettings = new MinioSettings
        {
            Enabled = true,
            Endpoint = "http://127.0.0.1:59999", // Unreachable port
            PublicUrl = "http://127.0.0.1:59999",
            BucketName = "culinary-blog"
        };
        var s3Config = new AmazonS3Config
        {
            ServiceURL = enabledSettings.Endpoint,
            ForcePathStyle = true,
            Timeout = TimeSpan.FromMilliseconds(500),
            MaxErrorRetry = 0
        };
        var failingClient = new AmazonS3Client("dummy", "dummy", s3Config);
        var service = new MinioFileStorageService(
            failingClient,
            Options.Create(enabledSettings),
            NullLogger<MinioFileStorageService>.Instance,
            _fallbackService);

        var content = "Failing upload content";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        await Assert.ThrowsAsync<CulinaryBlog.Application.Common.Exceptions.ServiceUnavailableException>(() =>
            service.UploadAsync(stream, "dish.jpg", "image/jpeg", "recipes/fail", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DeleteAsync_EmptyUrl_ShouldCompleteWithoutThrowing()
    {
        // Act & Assert (Idempotent)
        await _minioService.DeleteAsync("", TestContext.Current.CancellationToken);
        await _minioService.DeleteAsync("   ", TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DeleteAsync_LegacyLocalUploadUrl_ShouldDeleteViaFallbackStorage()
    {
        // Arrange
        var folder = Path.Combine(_testTempDir, "recipes");
        Directory.CreateDirectory(folder);
        var filePath = Path.Combine(folder, "legacy_file.png");
        await File.WriteAllTextAsync(filePath, "dummy", TestContext.Current.CancellationToken);
        Assert.True(File.Exists(filePath));

        // Act
        await _minioService.DeleteAsync("/uploads/recipes/legacy_file.png", TestContext.Current.CancellationToken);

        // Assert
        Assert.False(File.Exists(filePath));
    }
}
