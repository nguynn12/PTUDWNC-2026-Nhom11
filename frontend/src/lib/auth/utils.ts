/** Chỉ cho phép đường dẫn nội bộ (hoặc URL cùng origin) để tránh open redirect qua ?callbackUrl=. */
export function safeCallbackUrl(raw: string | null, fallback = "/profile"): string {
  if (!raw) return fallback;
  if (raw.startsWith("/") && !raw.startsWith("//")) return raw;
  // Auth.js middleware gắn callbackUrl dạng URL đầy đủ của chính site.
  try {
    const url = new URL(raw);
    if (typeof window !== "undefined" && url.origin === window.location.origin) {
      return `${url.pathname}${url.search}${url.hash}`;
    }
  } catch {
    // không phải URL hợp lệ -> dùng fallback
  }
  return fallback;
}

export function initialsOf(name: string | null | undefined): string {
  const parts = (name ?? "").trim().split(/\s+/).filter(Boolean);
  if (parts.length === 0) return "?";
  return (parts.length === 1 ? parts[0][0] : parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
}
