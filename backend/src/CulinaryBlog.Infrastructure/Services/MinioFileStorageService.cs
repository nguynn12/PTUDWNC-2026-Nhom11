namespace CulinaryBlog.Infrastructure.Services;

using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Dịch vụ lưu trữ tệp tin trên MinIO Object Storage (S3-compatible).
/// Hiện thực theo đặc tả SRS v1.2.0 (FR-FILE-001, FR-FILE-002).
/// Hỗ trợ upload ảnh với tên định danh duy nhất chống path traversal,
/// xóa idempotent không ném lỗi nếu file không tồn tại, và fallback local storage nếu cần.
/// </summary>
public class MinioFileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly MinioSettings _settings;
    private readonly ILogger<MinioFileStorageService> _logger;
    private readonly FileStorageService _fallbackLocalStorage;

    public MinioFileStorageService(
        IAmazonS3 s3Client,
        IOptions<MinioSettings> options,
        ILogger<MinioFileStorageService> logger,
        FileStorageService fallbackLocalStorage)
    {
        _s3Client = s3Client;
        _settings = options.Value;
        _logger = logger;
        _fallbackLocalStorage = fallbackLocalStorage;
    }

    /// <inheritdoc />
    public async Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string folder = "recipes",
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("MinIO đang bị vô hiệu hóa trong cấu hình, chuyển sang lưu trữ Local.");
            return await _fallbackLocalStorage.UploadAsync(stream, fileName, contentType, folder, cancellationToken);
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        
        // Chuẩn hóa tiền tố thư mục: loại bỏ dấu gạch chéo dư thừa
        var cleanFolder = folder.Trim().Trim('/');
        var objectKey = string.IsNullOrWhiteSpace(cleanFolder)
            ? uniqueFileName
            : $"{cleanFolder}/{uniqueFileName}";

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        try
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = objectKey,
                InputStream = stream,
                ContentType = contentType,
                AutoCloseStream = false
            };

            await _s3Client.PutObjectAsync(putRequest, cancellationToken);

            var publicUrl = $"{_settings.PublicUrl.TrimEnd('/')}/{_settings.BucketName}/{objectKey}";
            _logger.LogInformation("Đã lưu trữ thành công file lên MinIO: {ObjectKey} ({ContentType}) -> {PublicUrl}",
                objectKey, contentType, publicUrl);

            return publicUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tải file lên MinIO ({ObjectKey}), tự động chuyển sang Fallback Local Storage.", objectKey);
            
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            return await _fallbackLocalStorage.UploadAsync(stream, fileName, contentType, folder, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return;
        }

        // Nếu là URL cục bộ legacy (/uploads/...) -> xóa bằng fallback local storage
        if (fileUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase) ||
            fileUrl.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
        {
            await _fallbackLocalStorage.DeleteAsync(fileUrl, cancellationToken);
            return;
        }

        var objectKey = ExtractObjectKey(fileUrl);
        if (string.IsNullOrWhiteSpace(objectKey))
        {
            return;
        }

        try
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = objectKey
            };

            await _s3Client.DeleteObjectAsync(deleteRequest, cancellationToken);
            _logger.LogInformation("Đã xóa tệp tin khỏi MinIO thành công: Bucket={Bucket}, Key={Key}", _settings.BucketName, objectKey);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound || ex.ErrorCode == "NoSuchKey")
        {
            // Idempotent: Theo đặc tả SRS FR-FILE-002, nếu object không tồn tại trên MinIO thì không throw exception
            _logger.LogInformation("Tệp tin không tồn tại trên MinIO, bỏ qua xóa (Idempotent): Key={Key}", objectKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Lỗi khi xóa tệp tin khỏi MinIO: Key={Key}", objectKey);
            // Vẫn an toàn không làm crash luồng chính nếu lỗi mạng
        }
    }

    /// <summary>
    /// Trích xuất Object Key từ URL công khai của MinIO.
    /// Hỗ trợ cả URL tuyệt đối (http://localhost:9000/culinary-blog/recipes/...) và URL tương đối.
    /// </summary>
    public string ExtractObjectKey(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return string.Empty;
        }

        var path = fileUrl.Trim();

        // Nếu là URL đầy đủ có scheme http/https
        if (Uri.TryCreate(path, UriKind.Absolute, out var uri))
        {
            path = uri.AbsolutePath;
        }

        path = path.TrimStart('/');

        // Loại bỏ tên bucket khỏi đầu path nếu có
        var bucketPrefix = $"{_settings.BucketName}/";
        if (path.StartsWith(bucketPrefix, StringComparison.OrdinalIgnoreCase))
        {
            path = path.Substring(bucketPrefix.Length);
        }

        return path.TrimStart('/');
    }
}
