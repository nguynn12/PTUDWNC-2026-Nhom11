import { type ClassValue, clsx } from "clsx";
import { twMerge } from "tailwind-merge";

/**
 * Hàm cn kết hợp class names với clsx và tailwind-merge (chuẩn shadcn/ui)
 */
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

/**
 * Định dạng ngày tháng theo locale vi-VN
 */
export function formatDate(date: string | number | Date | null | undefined): string {
  if (!date) return "";
  try {
    const d = new Date(date);
    if (isNaN(d.getTime())) return "";
    return new Intl.DateTimeFormat("vi-VN", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    }).format(d);
  } catch {
    return "";
  }
}

/**
 * Định dạng thời lượng từ phút sang chuỗi tiếng Việt dễ hiểu
 * Ví dụ: 80 -> "1 giờ 20 phút", 30 -> "30 phút", 120 -> "2 giờ"
 */
export function formatDuration(minutes: number | null | undefined): string {
  if (minutes == null || isNaN(minutes) || minutes <= 0) return "0 phút";
  const hours = Math.floor(minutes / 60);
  const remainingMins = minutes % 60;

  if (hours > 0 && remainingMins > 0) {
    return `${hours} giờ ${remainingMins} phút`;
  }
  if (hours > 0) {
    return `${hours} giờ`;
  }
  return `${remainingMins} phút`;
}

/**
 * Định dạng số có dấu phân cách hàng nghìn
 */
export function formatNumber(num: number | null | undefined): string {
  if (num == null || isNaN(num)) return "0";
  return new Intl.NumberFormat("vi-VN").format(num);
}

/**
 * Xây dựng query string từ object params, loại bỏ các giá trị undefined, null hoặc chuỗi rỗng
 */
export function buildQueryString(params: Record<string, string | number | boolean | null | undefined>): string {
  const searchParams = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== null && value !== "") {
      searchParams.set(key, String(value));
    }
  }
  const str = searchParams.toString();
  return str ? `?${str}` : "";
}
