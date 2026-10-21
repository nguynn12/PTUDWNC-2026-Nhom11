import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

/**
 * Hàm tiện ích dùng chung (Buổi 6).
 * `cn()` do shadcn tạo: ghép className có điều kiện và gộp class Tailwind trùng (clsx + tailwind-merge).
 */
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

const LOCALE = "vi-VN";
const TIME_ZONE = "Asia/Ho_Chi_Minh";

type DateInput = string | number | Date | null | undefined;

/**
 * Định dạng ngày theo kiểu Việt Nam, cố định múi giờ để server và trình duyệt cho cùng kết quả.
 * formatDate("2026-10-14T08:00:00Z") → "14/10/2026"; withTime: true → "14/10/2026, 15:00".
 * Trả "" nếu giá trị rỗng hoặc không hợp lệ.
 */
export function formatDate(value: DateInput, options: { withTime?: boolean } = {}): string {
  if (value === null || value === undefined || value === "") return "";
  const date = value instanceof Date ? value : new Date(value);
  if (Number.isNaN(date.getTime())) return "";
  const day = new Intl.DateTimeFormat(LOCALE, {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    timeZone: TIME_ZONE,
  }).format(date);
  if (!options.withTime) return day;
  const time = new Intl.DateTimeFormat(LOCALE, {
    hour: "2-digit",
    minute: "2-digit",
    hour12: false,
    timeZone: TIME_ZONE,
  }).format(date);
  return `${day}, ${time}`;
}

/** Số phút → chuỗi thời lượng: 45 → "45 phút", 60 → "1 giờ", 80 → "1 giờ 20 phút". */
export function formatDuration(minutes: number | null | undefined): string {
  if (minutes === null || minutes === undefined || !Number.isFinite(minutes) || minutes <= 0) return "0 phút";
  const total = Math.round(minutes);
  const hours = Math.floor(total / 60);
  const rest = total % 60;
  if (hours === 0) return `${rest} phút`;
  return rest === 0 ? `${hours} giờ` : `${hours} giờ ${rest} phút`;
}

/** Định dạng số theo kiểu Việt Nam: 1234567 → "1.234.567"; 1234.5 → "1.234,5". */
export function formatNumber(value: number | null | undefined, maximumFractionDigits = 1): string {
  if (value === null || value === undefined || !Number.isFinite(value)) return "";
  return new Intl.NumberFormat(LOCALE, { maximumFractionDigits }).format(value);
}

type QueryValue = string | number | boolean | null | undefined;

/**
 * Tạo query string từ object, bỏ qua giá trị rỗng (undefined, null, ""); mảng thành nhiều tham số cùng tên.
 * buildQueryString({ page: 2, q: "phở", tags: ["a", "b"], categoryId: undefined }) → "?page=2&q=ph%E1%BB%9F&tags=a&tags=b".
 * Trả "" nếu không có tham số nào.
 */
export function buildQueryString(params: Record<string, QueryValue | QueryValue[]> | object): string {
  const search = new URLSearchParams();
  for (const [key, raw] of Object.entries(params)) {
    const values = Array.isArray(raw) ? raw : [raw];
    for (const value of values) {
      if (value === undefined || value === null || value === "") continue;
      search.append(key, String(value));
    }
  }
  const query = search.toString();
  return query ? `?${query}` : "";
}
