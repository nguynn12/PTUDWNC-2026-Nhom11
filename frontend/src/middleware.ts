import { NextResponse, type NextRequest } from "next/server";

// Trùng với AUTH_FLAG_COOKIE trong lib/auth/store.ts (không import để middleware không kéo code client).
const AUTH_FLAG_COOKIE = "cb_logged_in";
// SRS 5.1: /dashboard/*, /profile bắt buộc đăng nhập; /admin/* là khu quản trị của nhóm.
const PROTECTED_PREFIXES = ["/dashboard", "/admin", "/profile"];
// Chỉ hai trang này bị đá về /profile khi đã đăng nhập; /auth/confirm-email vẫn mở được.
const AUTH_ONLY_PAGES = ["/auth/login", "/auth/register"];

/**
 * Chuyển hướng sớm dựa trên cookie đánh dấu (không chứa token). Đây chỉ là lớp tiện lợi:
 * quyền thật do backend kiểm tra bằng Bearer token, và RequireAuth kiểm tra lại phía client.
 */
export function middleware(request: NextRequest) {
  const loggedIn = request.cookies.get(AUTH_FLAG_COOKIE)?.value === "1";
  const { pathname, search } = request.nextUrl;

  const isProtected = PROTECTED_PREFIXES.some((p) => pathname === p || pathname.startsWith(`${p}/`));
  if (isProtected && !loggedIn) {
    const url = new URL("/auth/login", request.url);
    url.searchParams.set("callbackUrl", `${pathname}${search}`);
    return NextResponse.redirect(url);
  }

  if (AUTH_ONLY_PAGES.includes(pathname) && loggedIn) {
    return NextResponse.redirect(new URL("/profile", request.url));
  }
  return NextResponse.next();
}

export const config = {
  matcher: ["/dashboard", "/dashboard/:path*", "/admin/:path*", "/profile", "/profile/:path*", "/auth/:path*"],
};
