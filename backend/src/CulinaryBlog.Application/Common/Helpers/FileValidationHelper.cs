namespace CulinaryBlog.Application.Common.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using SixLabors.ImageSharp;

/// <summary>
/// Tiện ích xác thực tệp tin hình ảnh an toàn theo SRS v1.2.0 (FR-RCP-008, FR-FILE-001) và Lab 8.
/// Kiểm tra dung lượng tối đa 5MB trước khi xử lý, MIME types cho phép (JPEG, PNG, WebP, AVIF),
/// kiểm tra chữ ký Magic Bytes và giải mã ảnh thực nghiệm bằng SixLabors.ImageSharp.
/// </summary>
public static class FileValidationHelper
{
    public const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    /// <summary>Giới hạn số điểm ảnh (chống ảnh nén nhỏ nhưng giải nén rất lớn).</summary>
    public const long MaxPixels = 40_000_000;

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
    /// Kiểm tra tính hợp lệ toàn diện của file hình ảnh: dung lượng <= 5MB, đuôi file, MIME type,
    /// Magic Bytes và giải mã nội dung ảnh thực tế bằng ImageSharp.
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

        // 1. Kiểm tra chữ ký Magic Bytes đầu tệp
        if (!ValidateMagicBytes(stream, extension))
        {
            return (false, "Nội dung tệp tin không đúng với định dạng hình ảnh đã khai báo (sai chữ ký Magic Bytes).");
        }

        // 2. Giải mã thông số ảnh thật cho MỌI tệp (kể cả ảnh rất nhỏ). AVIF mà ImageSharp không hỗ trợ
        //    thì kiểm tra cấu trúc hộp ISO-BMFF (ftyp + brand avif/avis, kích thước hộp hợp lệ).
        if (!CanDecodeWithImageSharp(stream, extension))
        {
            return (false, "Nội dung tệp tin hình ảnh bị hỏng hoặc không thể giải mã thành ảnh hợp lệ.");
        }

        return (true, null);
    }

    /// <summary>
    /// Giải mã thông số ảnh thực sự bằng SixLabors.ImageSharp (kích thước, định dạng hợp lệ).
    /// </summary>
    public static bool CanDecodeWithImageSharp(Stream stream, string extension)
    {
        if (!stream.CanRead || !stream.CanSeek)
        {
            return false;
        }

        var originalPosition = stream.Position;
        stream.Position = 0;

        try
        {
            if (extension.Equals(".avif", StringComparison.OrdinalIgnoreCase))
            {
                return HasValidAvifStructure(stream);
            }

            var imageInfo = Image.Identify(stream);
            return imageInfo is { Width: > 0, Height: > 0 }
                   && (long)imageInfo.Width * imageInfo.Height <= MaxPixels;
        }
        catch
        {
            return false;
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }

    /// <summary>
    /// Kiểm tra hộp <c>ftyp</c> đầu tệp AVIF: kích thước hộp nằm trong tệp và brand chính hoặc
    /// brand tương thích là <c>avif</c>/<c>avis</c>.
    /// </summary>
    private static bool HasValidAvifStructure(Stream stream)
    {
        Span<byte> head = stackalloc byte[8];
        if (stream.Read(head) < 8 || !head.Slice(4, 4).SequenceEqual("ftyp"u8))
        {
            return false;
        }

        var boxSize = (long)System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(head);
        // Tối thiểu: 8 (header) + 4 (major brand) + 4 (minor version); và không vượt quá tệp.
        if (boxSize < 16 || boxSize > stream.Length)
        {
            return false;
        }

        var body = new byte[boxSize - 8];
        if (stream.Read(body, 0, body.Length) != body.Length)
        {
            return false;
        }

        bool IsAvifBrand(ReadOnlySpan<byte> brand) => brand.SequenceEqual("avif"u8) || brand.SequenceEqual("avis"u8);

        if (IsAvifBrand(body.AsSpan(0, 4)))
        {
            return true;
        }

        // Brand tương thích bắt đầu từ byte 8 của phần thân, mỗi brand 4 byte.
        for (var i = 8; i + 4 <= body.Length; i += 4)
        {
            if (IsAvifBrand(body.AsSpan(i, 4)))
            {
                return true;
            }
        }

        return false;
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
            if (ext is ".png")
            {
                if (bytesRead < 8) return false;
                return header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                       header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A;
            }

            // WebP: RIFF .... WEBP
            if (ext is ".webp")
            {
                if (bytesRead < 12) return false;
                return header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                       header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50;
            }

            // AVIF: .... ftyp avif (hoặc avis)
            if (ext is ".avif")
            {
                if (bytesRead < 12) return false;
                return header[4] == 0x66 && header[5] == 0x74 && header[6] == 0x79 && header[7] == 0x70 &&
                       header[8] == 0x61 && header[9] == 0x76 && header[10] == 0x69 && (header[11] == 0x66 || header[11] == 0x73);
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
