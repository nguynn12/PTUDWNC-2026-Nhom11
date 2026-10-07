import { create } from "zustand";
import { setAuthToken } from "@/lib/api-client";
import type { AuthResponseDto, UserDto } from "./types";

export type AuthStatus = "loading" | "authenticated" | "unauthenticated";

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

/**
 * Chia sẻ access token cho các module khác đang có trên develop: lib/api/axios.ts đọc
 * localStorage "accessToken", lib/api-client.ts dùng setAuthToken(). Tránh phải sửa code của họ.
 */
function shareAccessToken(token: string | null) {
  try {
    if (token) window.localStorage.setItem("accessToken", token);
    else window.localStorage.removeItem("accessToken");
  } catch {
    // bỏ qua: storage bị chặn
  }
  setAuthToken(token);
}

interface AuthState {
  status: AuthStatus;
  user: UserDto | null;
  /** Access token chỉ giữ trong bộ nhớ (hết hạn sau 15 phút). */
  accessToken: string | null;
  accessTokenExpiresAt: number;
  setSession: (res: AuthResponseDto) => void;
  setUser: (user: UserDto) => void;
  /** Xoá phiên. persist=false khi tab khác đã xoá storage. */
  clear: (persist?: boolean) => void;
  markUnauthenticated: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  status: "loading",
  user: null,
  accessToken: null,
  accessTokenExpiresAt: 0,
  setSession: (res) => {
    writeRefreshToken(res.refreshToken);
    shareAccessToken(res.accessToken);
    set({
      status: "authenticated",
      user: res.user,
      accessToken: res.accessToken,
      accessTokenExpiresAt: Date.now() + res.expiresIn * 1000,
    });
  },
  setUser: (user) => set({ user }),
  clear: (persist = true) => {
    if (persist) writeRefreshToken(null);
    shareAccessToken(null);
    set({ status: "unauthenticated", user: null, accessToken: null, accessTokenExpiresAt: 0 });
  },
  markUnauthenticated: () => {
    shareAccessToken(null);
    set({ status: "unauthenticated", user: null, accessToken: null, accessTokenExpiresAt: 0 });
  },
}));

export const STORAGE_KEY_REFRESH_TOKEN = REFRESH_TOKEN_KEY;
