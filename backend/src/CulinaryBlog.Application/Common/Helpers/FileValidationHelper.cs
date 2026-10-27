namespace CulinaryBlog.Application.Common.Helpers;

/// <summary>
/// Tiện ích xác thực tệp tin hình ảnh an toàn theo SRS v1.2.0 (FR-RCP-008, FR-FILE-001).
/// Kiểm tra dung lượng tối đa 5MB, MIME types cho phép (JPEG, PNG, WebP, AVIF)
/// và kiểm tra chữ ký Magic Bytes để ngăn chặn việc đổi đuôi file độc hại.
/// </summary>
public static class FileValidationHelper
{
    public const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp",
        "image/avif"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".avif"
    };

    /// <summary>
    /// Kiểm tra tính hợp lệ toàn diện của file hình ảnh: dung lượng, đuôi file, MIME type và Magic Bytes.
    /// </summary>
    public static (bool IsValid, string? ErrorMessage) ValidateImage(
        Stream stream,
        string fileName,
        string contentType,
        long length)
    {
        if (length <= 0)
        {
            return (false, "Tệp tin hình ảnh rỗng (dung lượng 0 bytes).");
        }

        if (length > MaxFileSizeBytes)
        {
            return (false, "Dung lượng tệp tin vượt quá giới hạn tối đa cho phép là 5MB.");
        }

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            return (false, $"Phần mở rộng '{extension}' không được hỗ trợ. Chỉ chấp nhận .jpg, .jpeg, .png, .webp, .avif.");
        }

        if (string.IsNullOrWhiteSpace(contentType) || !AllowedMimeTypes.Contains(contentType))
        {
            return (false, $"Định dạng MIME '{contentType}' không hợp lệ. Chỉ chấp nhận JPEG, PNG, WebP, AVIF.");
        }

        // Kiểm tra chữ ký Magic Bytes
        if (!ValidateMagicBytes(stream, extension))
        {
            return (false, "Nội dung tệp tin không đúng với định dạng hình ảnh đã khai báo (sai chữ ký Magic Bytes).");
        }

        return (true, null);
    }

    /// <summary>
    /// Kiểm tra Magic Bytes đầu tệp để đảm bảo tính xác thực của định dạng ảnh.
    /// </summary>
    public static bool ValidateMagicBytes(Stream stream, string extension)
    {
        if (!stream.CanRead)
        {
            return false;
        }

        long originalPosition = 0;
        if (stream.CanSeek)
        {
            originalPosition = stream.Position;
            stream.Position = 0;
        }

        try
        {
            byte[] header = new byte[16];
            int bytesRead = stream.Read(header, 0, header.Length);
            if (bytesRead < 4)
            {
                return false;
            }

            var ext = extension.ToLowerInvariant();

            // JPEG: FF D8 FF
            if (ext is ".jpg" or ".jpeg")
            {
                return header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
            }

            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (ext == ".png")
            {
                if (bytesRead < 8) return false;
                return header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                       header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A;
            }

            // WebP: RIFF (bytes 0-3) + WEBP (bytes 8-11)
            if (ext == ".webp")
            {
                if (bytesRead < 12) return false;
                bool isRiff = header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46;
                bool isWebp = header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50;
                return isRiff && isWebp;
            }

            // AVIF: ISO BMFF box `ftyp` (bytes 4-7: 66 74 79 70) and brand `avif` / `avis` (bytes 8-11: 61 76 69 66 / 61 76 69 73)
            if (ext == ".avif")
            {
                if (bytesRead < 12) return false;
                bool hasFtyp = header[4] == 0x66 && header[5] == 0x74 && header[6] == 0x79 && header[7] == 0x70;
                bool isAvif = (header[8] == 0x61 && header[9] == 0x76 && header[10] == 0x69 && header[11] == 0x66) ||
                              (header[8] == 0x61 && header[9] == 0x76 && header[10] == 0x69 && header[11] == 0x73);
                return hasFtyp && isAvif;
            }

            return false;
        }
        finally
        {
            if (stream.CanSeek)
            {
                stream.Position = originalPosition;
            }
        }
    }
}
