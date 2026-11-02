namespace CulinaryBlog.Infrastructure.Services;

/// <summary>
/// Cấu hình kết nối MinIO Object Storage theo chuẩn S3-compatible (FR-FILE-001, FR-FILE-002).
/// Phục vụ lưu trữ tệp tin ảnh công thức và avatar.
/// </summary>
public class MinioSettings
{
    public const string SectionName = "Minio";

    /// <summary>
    /// Cho phép bật/tắt MinIO (nếu false hoặc lỗi sẽ tự động fallback sang local storage).
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Endpoint kết nối nội bộ hoặc URL MinIO S3 API (mặc định http://localhost:9000).
    /// </summary>
    public string Endpoint { get; set; } = "http://localhost:9000";

    /// <summary>
    /// Base URL công khai để trình duyệt truy cập tải ảnh về (mặc định http://localhost:9000).
    /// </summary>
    public string PublicUrl { get; set; } = "http://localhost:9000";

    /// <summary>
    /// Access Key (User) của MinIO.
    /// </summary>
    public string AccessKey { get; set; } = "culinary_minio_admin";

    /// <summary>
    /// Secret Key (Password) của MinIO.
    /// </summary>
    public string SecretKey { get; set; } = "culinary_minio_secret";

    /// <summary>
    /// Tên Bucket lưu trữ tệp tin (mặc định culinary-blog theo SRS 3.5).
    /// </summary>
    public string BucketName { get; set; } = "culinary-blog";

    /// <summary>
    /// Vùng địa lý S3 (mặc định us-east-1).
    /// </summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>
    /// Bắt buộc sử dụng Path Style (http://endpoint/bucket/key) thay vì Virtual Host Style.
    /// </summary>
    public bool ForcePathStyle { get; set; } = true;
}
