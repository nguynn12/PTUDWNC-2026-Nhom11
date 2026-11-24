import { NextResponse, type NextRequest } from "next/server";

// Trùng với AUTH_FLAG_COOKIE trong store/auth-store.ts (không import để middleware không kéo code client).
const AUTH_FLAG_COOKIE = "cb_logged_in";
// SRS 5.1: /dashboard/*, /profile bắt buộc đăng nhập; /admin/* là khu quản trị của nhóm.
const PROTECTED_PREFIXES = ["/dashboard", "/admin", "/profile"];
// Chỉ hai trang này bị đá về /profile khi đã đăng nhập; /auth/confirm-email vẫn mở được.
const AUTH_ONLY_PAGES = ["/auth/login", "/auth/register"];

/**
 * Chuyển hướng sớm dựa trên cookie đánh dấu (không chứa token). Đây chỉ là lớp tiện lợi:
 * quyền thật do backend kiểm tra bằng Bearer token, và RequireAuth kiểm tra lại phía client.
 */
export async function middleware(request: NextRequest) {
  const { pathname, search } = request.nextUrl;

  // API trả 301 cho slug cũ. Giữ mã 301 ở URL trang web thay vì để HTTP client tự theo redirect.
  if ((request.method === "GET" || request.method === "HEAD") && /^\/recipes\/[^/]+$/.test(pathname)) {
    const backend = process.env.BACKEND_API_URL || "http://localhost:5000";
    try {
      const response = await fetch(`${backend}/api/v1${pathname}`, {
        redirect: "manual", cache: "no-store", signal: AbortSignal.timeout(3000),
        headers: { Accept: "application/json" },
      });
      const location = response.headers.get("location");
      if (response.status === 301 && location) {
        const destination = new URL(location, backend);
        const match = /^\/api\/v1\/recipes\/([a-z0-9]+(?:-[a-z0-9]+)*)$/.exec(destination.pathname);
        if (destination.origin === new URL(backend).origin && match && `/recipes/${match[1]}` !== pathname) {
          return NextResponse.redirect(new URL(`/recipes/${match[1]}${search}`, request.url), 301);
        }
      }
    } catch {
      // Trang chi tiết tự xử lý lỗi/404 khi backend không sẵn sàng.
    }
  }

  // App Router caches prefetched route responses across client-side cookie changes.
  // Only redirect documents here. Client navigation uses the current auth store guards.
  const fetchMode = request.headers.get("sec-fetch-mode");
  if (fetchMode && fetchMode !== "navigate") return NextResponse.next();

  const loggedIn = request.cookies.get(AUTH_FLAG_COOKIE)?.value === "1";
  const isProtected = PROTECTED_PREFIXES.some((p) => pathname === p || pathname.startsWith(`${p}/`));
  if (isProtected && !loggedIn) {
    const url = new URL("/auth/login", request.url);
    url.searchParams.set("callbackUrl", `${pathname}${search}`);
    return NextResponse.redirect(url, { headers: { "Cache-Control": "private, no-store" } });
  }

  if (AUTH_ONLY_PAGES.includes(pathname) && loggedIn) {
    return NextResponse.redirect(new URL("/profile", request.url), { headers: { "Cache-Control": "private, no-store" } });
  }
  return NextResponse.next();
}

export const config = {
  matcher: ["/recipes/:slug", "/dashboard", "/dashboard/:path*", "/admin/:path*", "/profile", "/profile/:path*", "/auth/:path*"],
};
