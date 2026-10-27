namespace CulinaryBlog.Infrastructure.Services;

using System.IO;
using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

/// <summary>
/// Hiện thực dịch vụ lưu trữ tệp tin tuân thủ SRS v1.2.0 (FR-FILE-001, FR-FILE-002).
/// Hỗ trợ lưu trữ tệp tin an toàn, chống path traversal và xử lý xóa idempotent.
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly string _storageBasePath;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger)
    {
        _logger = logger;

        // Cho phép cấu hình thư mục lưu trữ tùy chỉnh qua appsettings/env, mặc định là wwwroot/uploads
        var configuredPath = configuration["FileStorage:BasePath"];
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            _storageBasePath = Path.GetFullPath(configuredPath);
        }
        else
        {
            _storageBasePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        }

        if (!Directory.Exists(_storageBasePath))
        {
            Directory.CreateDirectory(_storageBasePath);
        }
    }

    /// <inheritdoc />
    public async Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string folder = "recipes",
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var folderDirectory = Path.Combine(_storageBasePath, folder);

        if (!Directory.Exists(folderDirectory))
        {
            Directory.CreateDirectory(folderDirectory);
        }

        var destinationFilePath = Path.Combine(folderDirectory, uniqueFileName);

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        using (var fileStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
        {
            await stream.CopyToAsync(fileStream, cancellationToken);
        }

        _logger.LogInformation("Đã lưu trữ thành công file: {Path} ({ContentType})", destinationFilePath, contentType);

        // Trả về đường dẫn tương đối công khai
        return $"/uploads/{folder}/{uniqueFileName}";
    }

    /// <inheritdoc />
    public Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return Task.CompletedTask;
        }

        try
        {
            // Trích xuất đường dẫn tương đối từ URL (loại bỏ tiền tố /uploads/ nếu có)
            var cleanUrl = fileUrl.TrimStart('/');
            if (cleanUrl.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
            {
                cleanUrl = cleanUrl.Substring("uploads/".Length);
            }

            // Ngăn chặn path traversal bằng Path.GetFileName và thư mục hợp lệ
            var targetFilePath = Path.GetFullPath(Path.Combine(_storageBasePath, cleanUrl));

            // Đảm bảo đường dẫn nằm trong _storageBasePath
            if (!targetFilePath.StartsWith(_storageBasePath, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Phát hiện đường dẫn file không an toàn khi xóa: {FileUrl}", fileUrl);
                return Task.CompletedTask;
            }

            if (File.Exists(targetFilePath))
            {
                File.Delete(targetFilePath);
                _logger.LogInformation("Đã xóa file vật lý: {Path}", targetFilePath);
            }
            else
            {
                _logger.LogDebug("Tệp tin cần xóa không tồn tại (idempotent): {FileUrl}", fileUrl);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi xóa tệp tin: {FileUrl}", fileUrl);
        }

        return Task.CompletedTask;
    }
}
