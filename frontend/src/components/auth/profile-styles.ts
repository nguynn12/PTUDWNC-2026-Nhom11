/**
 * Class dùng chung cho khu vực tài khoản, để Button/Input/card giữ một chuẩn duy nhất:
 * ô nhập và nút cao 44px (vùng chạm), bo góc theo --radius của .cb-account.
 */

/** Khối nội dung trắng trên nền kem: viền nhẹ, gần như không đổ bóng. */
export const surfaceClass = "rounded-xl border border-border bg-card shadow-[0_1px_2px_rgba(53,39,31,0.04)]";

/** Nút chính: cam đất, hover đậm hơn (không dùng độ mờ làm nhạt màu). */
export const primaryButtonClass =
  "h-11 px-6 text-[15px] font-semibold shadow-none hover:bg-(color:--cb-primary-hover) active:bg-(color:--cb-primary-hover)";

/** Nút phụ: nền trắng, viền be đậm vừa, hover kem. */
export const outlineButtonClass =
  "h-11 px-5 text-[15px] font-medium border-(color:--cb-border-strong) bg-card text-foreground shadow-none hover:bg-accent hover:text-foreground";

/** Nút nguy hiểm (xác nhận xoá). */
export const dangerButtonClass = "h-11 px-5 text-[15px] font-semibold bg-destructive text-white shadow-none hover:bg-destructive/90";

/** Chiều cao ô nhập. */
export const controlClass = "h-11";

/** Tiêu đề section theo phong cách biên tập. */
export const sectionTitleClass = "font-serif text-xl font-semibold leading-snug tracking-tight text-foreground";
