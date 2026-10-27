namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>
/// Giao diện trừu tượng hóa dịch vụ lưu trữ tệp tin (Object Storage / File Storage).
/// Tuân thủ SRS v1.2.0 (FR-FILE-001, FR-FILE-002) và Clean Architecture.
/// Cho phép hoán đổi linh hoạt giữa MinIO, AWS S3 và Local Storage.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Lưu trữ tệp tin từ Stream lên hệ thống lưu trữ và trả về đường dẫn URL công khai.
    /// Tạo tên file ngẫu nhiên chống path traversal: {folder}/{Guid.NewGuid()}{ext}
    /// </summary>
    /// <param name="stream">Luồng dữ liệu nhị phân của tệp tin.</param>
    /// <param name="fileName">Tên tệp tin gốc để trích xuất phần mở rộng an toàn.</param>
    /// <param name="contentType">MIME type của tệp tin.</param>
    /// <param name="folder">Thư mục/tiền tố lưu trữ (mặc định "recipes").</param>
    /// <param name="cancellationToken">Token hủy tác vụ.</param>
    /// <returns>Đường dẫn URL công khai của tệp tin.</returns>
    Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string folder = "recipes",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa tệp tin khỏi hệ thống lưu trữ.
    /// Tính chất Idempotent: không gây lỗi nếu tệp tin không tồn tại trên storage.
    /// </summary>
    /// <param name="fileUrl">Đường dẫn URL của tệp tin cần xóa.</param>
    /// <param name="cancellationToken">Token hủy tác vụ.</param>
    Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default);
}
