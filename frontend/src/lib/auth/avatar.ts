/**
 * Quy tắc ảnh đại diện phía client. Backend là nơi kiểm tra cuối cùng (MIME, kích thước, magic bytes
 * — SRS CONS-007, mục 5.3); phần này chỉ báo lỗi sớm để người dùng không phải chờ tải lên rồi mới biết.
 */

export const AVATAR_MAX_BYTES = 5 * 1024 * 1024; // 5 MB, bằng giới hạn upload ảnh của IFileStorageService (SRS)

export const AVATAR_TYPES = ["image/jpeg", "image/png", "image/webp"] as const;
export type AvatarMimeType = (typeof AVATAR_TYPES)[number];

/** Giá trị cho thuộc tính accept của <input type="file">. */
export const AVATAR_ACCEPT = [...AVATAR_TYPES, ".jpg", ".jpeg", ".png", ".webp"].join(",");

/**
 * Backend CHƯA có endpoint tải ảnh đại diện (chỉ có PATCH /auth/me nhận avatarUrl dạng chuỗi).
 * Chọn ảnh từ thiết bị chỉ được bật khi đặt NEXT_PUBLIC_AVATAR_UPLOAD_ENABLED=true sau khi backend
 * có POST /auth/me/avatar (xem authApi.uploadAvatar).
 */
export const AVATAR_UPLOAD_ENABLED = process.env.NEXT_PUBLIC_AVATAR_UPLOAD_ENABLED === "true";

export function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toLocaleString("vi-VN", { maximumFractionDigits: 0 })} KB`;
  // Làm tròn lên 1 chữ số để tệp vượt giới hạn không hiện cùng số với giới hạn ("5 MB, vượt 5 MB").
  return `${(Math.ceil((bytes / (1024 * 1024)) * 10) / 10).toLocaleString("vi-VN")} MB`;
}

/** Đọc vài byte đầu để biết định dạng thật của tệp (không tin vào đuôi hay file.type). */
export async function sniffImageType(file: Blob): Promise<AvatarMimeType | null> {
  const bytes = new Uint8Array(await file.slice(0, 12).arrayBuffer());
  const at = (i: number, ...sig: number[]) => sig.every((b, k) => bytes[i + k] === b);
  if (at(0, 0xff, 0xd8, 0xff)) return "image/jpeg";
  if (at(0, 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a)) return "image/png";
  if (at(0, 0x52, 0x49, 0x46, 0x46) && at(8, 0x57, 0x45, 0x42, 0x50)) return "image/webp"; // RIFF....WEBP
  return null;
}

/** Trả thông báo lỗi tiếng Việt, hoặc null nếu tệp hợp lệ. */
export async function validateAvatarFile(file: File): Promise<string | null> {
  if (file.size === 0) return "Tệp ảnh trống. Vui lòng chọn ảnh khác.";
  if (file.size > AVATAR_MAX_BYTES) {
    return `Ảnh nặng ${formatFileSize(file.size)}, vượt giới hạn ${formatFileSize(AVATAR_MAX_BYTES)}.`;
  }
  if (!(AVATAR_TYPES as readonly string[]).includes(file.type) || !(await sniffImageType(file))) {
    return "Chỉ hỗ trợ ảnh JPG, PNG hoặc WebP.";
  }
  return null;
}
