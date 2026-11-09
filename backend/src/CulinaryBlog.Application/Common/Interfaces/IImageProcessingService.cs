namespace CulinaryBlog.Application.Common.Interfaces;

using System.IO;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Kết quả thông tin của một phiên bản ảnh đã được xử lý (Thumbnail / Medium).
/// </summary>
public record ImageVariantResult(
    Stream Stream,
    string FileName,
    string ContentType,
    int Width,
    int Height);

/// <summary>
/// Giao diện xử lý biến đổi hình ảnh (Image Resizing & Optimization).
/// Hiện thực theo yêu cầu FR-JOB-002: sinh thumbnail 300x300 và medium 800x600 bằng ImageSharp.
/// </summary>
public interface IImageProcessingService
{
    /// <summary>
    /// Sinh các phiên bản ảnh thumbnail (300x300) và medium (800x600) từ luồng ảnh gốc.
    /// Nếu ảnh không hỗ trợ giải mã hoặc lỗi xử lý, trả về null để luồng chính tiếp tục dùng ảnh gốc an toàn.
    /// </summary>
    Task<(ImageVariantResult? Thumbnail, ImageVariantResult? Medium)> ProcessVariantsAsync(
        Stream sourceStream,
        string originalFileName,
        CancellationToken cancellationToken = default);
}
