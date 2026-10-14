/**
 * auth-store.ts — store Zustand quản lý trạng thái đăng nhập của toàn app (module Xác thực, TV1).
 * - Access token chỉ giữ trong bộ nhớ; refresh token ở localStorage (SRS 5.2: gửi trong body, không dùng cookie).
 * - Các thao tác gọi API (login, refresh, logout...) nằm ở lib/auth/session.ts; store chỉ giữ và đồng bộ trạng thái.
 * - Component đọc store qua selector (useCurrentUser, useAuthStatus, useHasRole hoặc useAuthStore(s => s.x))
 *   để chỉ render lại khi đúng phần state đó thay đổi; không gọi useAuthStore() không có selector.
 */
import { create } from "zustand";
import type { AuthResponseDto, AuthStatus, UserDto, UserRole } from "@/types/auth";

export type { AuthStatus } from "@/types/auth";

/** Refresh token (SRS 5.2: gửi trong body, không dùng cookie) lưu ở localStorage để giữ đăng nhập khi tải lại trang. */
const REFRESH_TOKEN_KEY = "cb.refreshToken";
/** Cookie đánh dấu "đã đăng nhập" cho middleware chuyển hướng sớm. KHÔNG chứa token. */
export const AUTH_FLAG_COOKIE = "cb_logged_in";
const FLAG_MAX_AGE_SECONDS = 7 * 24 * 60 * 60; // bằng thời hạn refresh token

export function readRefreshToken(): string | null {
  try {
    return window.localStorage.getItem(REFRESH_TOKEN_KEY);
  } catch {
    return null;
  }
}

function writeRefreshToken(token: string | null) {
  try {
    if (token) window.localStorage.setItem(REFRESH_TOKEN_KEY, token);
    else window.localStorage.removeItem(REFRESH_TOKEN_KEY);
  } catch {
    // Trình duyệt chặn storage: phiên chỉ sống trong tab hiện tại.
  }
  document.cookie = token
    ? `${AUTH_FLAG_COOKIE}=1; path=/; max-age=${FLAG_MAX_AGE_SECONDS}; SameSite=Lax`
    : `${AUTH_FLAG_COOKIE}=; path=/; max-age=0; SameSite=Lax`;
}

/** Xoá access token mà bản cũ từng ghi vào localStorage (nay token chỉ nằm trong bộ nhớ). */
function removeLegacyAccessToken() {
  try {
    window.localStorage.removeItem("accessToken");
  } catch {
    // bỏ qua: storage bị chặn
  }
}

export interface AuthStateData {
  /** "loading" khi đang khôi phục phiên lúc mở app. */
  status: AuthStatus;
  user: UserDto | null;
  /** Access token chỉ giữ trong bộ nhớ (hết hạn sau 15 phút). Đọc qua lib/api/axios.ts, không tự gắn header. */
  accessToken: string | null;
  /** Thời điểm hết hạn của access token (ms, epoch). */
  accessTokenExpiresAt: number;
}

export interface AuthActions {
  /** Lưu phiên sau khi đăng nhập/đăng ký/refresh thành công. */
  setSession: (res: AuthResponseDto) => void;
  /** Cập nhật hồ sơ sau khi sửa profile. */
  setUser: (user: UserDto) => void;
  /** Xoá phiên. persist=false khi tab khác đã xoá storage. */
  clearSession: (persist?: boolean) => void;
  /** Refresh lỗi mạng: coi như chưa đăng nhập nhưng giữ refresh token để thử lại sau. */
  markUnauthenticated: () => void;
}

export type AuthState = AuthStateData & AuthActions;

const SIGNED_OUT: Omit<AuthStateData, "status"> = { user: null, accessToken: null, accessTokenExpiresAt: 0 };

export const useAuthStore = create<AuthState>((set) => ({
  status: "loading",
  ...SIGNED_OUT,
  setSession: (res) => {
    writeRefreshToken(res.refreshToken);
    removeLegacyAccessToken();
    set({
      status: "authenticated",
      user: res.user,
      accessToken: res.accessToken,
      accessTokenExpiresAt: Date.now() + res.expiresIn * 1000,
    });
  },
  setUser: (user) => set({ user }),
  clearSession: (persist = true) => {
    if (persist) writeRefreshToken(null);
    removeLegacyAccessToken();
    set({ status: "unauthenticated", ...SIGNED_OUT });
  },
  markUnauthenticated: () => set({ status: "unauthenticated", ...SIGNED_OUT }),
}));

export const STORAGE_KEY_REFRESH_TOKEN = REFRESH_TOKEN_KEY;

// ----------------------------------------------------------------------------
// Selector dùng chung cho mọi module (header, dashboard, admin...)
// ----------------------------------------------------------------------------

export function hasRole(user: UserDto | null, role: UserRole): boolean {
  return Boolean(user?.roles.includes(role));
}

/** Người dùng đang đăng nhập (null nếu chưa đăng nhập hoặc đang khôi phục phiên). */
export const useCurrentUser = () => useAuthStore((state) => state.user);

/** "loading" | "authenticated" | "unauthenticated". */
export const useAuthStatus = () => useAuthStore((state) => state.status);

export const useIsAuthenticated = () => useAuthStore((state) => state.status === "authenticated");

/** true nếu người dùng hiện tại có vai trò role (Admin có mọi quyền của Author theo SRS 2.3). */
export const useHasRole = (role: UserRole) =>
  useAuthStore((state) => hasRole(state.user, role) || (role === "Author" && hasRole(state.user, "Admin")));
