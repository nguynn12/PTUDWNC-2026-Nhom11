import axios from "axios";
import { ApiError, fromAxiosError } from "./errors";
import type { AuthResponseDto, RegisterRequest } from "./types";
import { readRefreshToken, STORAGE_KEY_REFRESH_TOKEN, useAuthStore } from "./store";

/** Client riêng cho /auth/*: không gắn interceptor để tránh vòng lặp refresh. */
const authHttp = axios.create({
  baseURL: "/api/v1",
  timeout: 15_000,
  headers: { Accept: "application/json" },
});

async function post<T>(path: string, body: unknown): Promise<T> {
  try {
    const res = await authHttp.post<T>(path, body);
    return res.data;
  } catch (error) {
    throw fromAxiosError(error);
  }
}

const REFRESH_MARGIN_MS = 60_000;

export async function login(email: string, password: string) {
  const res = await post<AuthResponseDto>("/auth/login", { email, password });
  useAuthStore.getState().setSession(res);
  return res.user;
}

export async function loginWithGoogle(idToken: string, nonce: string) {
  const res = await post<AuthResponseDto>("/auth/google", { idToken, nonce });
  useAuthStore.getState().setSession(res);
  return res.user;
}

/** FR-AUTH-001: đăng ký trả 201 kèm cặp token nên người dùng đăng nhập luôn. */
export async function register(body: RegisterRequest) {
  const res = await post<AuthResponseDto>("/auth/register", body);
  useAuthStore.getState().setSession(res);
  return res.user;
}

/** FR-AUTH-005: chỉ cần refresh token trong body; luôn xoá phiên phía client kể cả khi API lỗi. */
export async function logout() {
  const refreshToken = readRefreshToken();
  if (refreshToken) {
    await post("/auth/logout", { refreshToken }).catch(() => undefined);
  }
  useAuthStore.getState().clear();
}

// Mỗi refresh token chỉ dùng được một lần (backend xoay vòng và thu hồi cả family nếu dùng lại).
// Gộp các lần gọi trong cùng tab bằng một promise, và giữa các tab bằng Web Locks.
let refreshing: Promise<boolean> | null = null;

export function refreshSession(): Promise<boolean> {
  if (!refreshing) {
    refreshing = runExclusive(doRefresh).finally(() => {
      refreshing = null;
    });
  }
  return refreshing;
}

function runExclusive(task: () => Promise<boolean>): Promise<boolean> {
  if (typeof navigator !== "undefined" && navigator.locks) {
    // request() gói kết quả callback trong một Promise nữa; then() để TypeScript gỡ một lớp.
    return navigator.locks.request("cb-auth-refresh", task).then((result) => result);
  }
  return task();
}

async function doRefresh(): Promise<boolean> {
  // Đọc lại trong khoá: tab khác có thể vừa xoay vòng token.
  const refreshToken = readRefreshToken();
  if (!refreshToken) {
    useAuthStore.getState().clear();
    return false;
  }
  try {
    const res = await post<AuthResponseDto>("/auth/refresh", { refreshToken });
    useAuthStore.getState().setSession(res);
    return true;
  } catch (error) {
    if (error instanceof ApiError && (error.status === 401 || error.status === 403)) {
      useAuthStore.getState().clear(); // token hết hạn/bị thu hồi/tài khoản bị khoá
    } else {
      useAuthStore.getState().markUnauthenticated(); // lỗi mạng: giữ token để thử lại sau
    }
    return false;
  }
}

/** Trả access token còn hạn (tự refresh nếu còn dưới 60 giây), hoặc null nếu chưa đăng nhập. */
export async function getValidAccessToken(): Promise<string | null> {
  const { accessToken, accessTokenExpiresAt } = useAuthStore.getState();
  if (accessToken && Date.now() < accessTokenExpiresAt - REFRESH_MARGIN_MS) return accessToken;
  if (!readRefreshToken()) return null;
  return (await refreshSession()) ? useAuthStore.getState().accessToken : null;
}

/**
 * Gọi một lần khi app khởi động: khôi phục phiên từ refresh token, tự làm mới access token
 * trước khi hết hạn (để các module khác đọc token dùng chung luôn có token còn hạn),
 * và đồng bộ đăng xuất giữa các tab.
 */
export function startSession(): () => void {
  let timer: ReturnType<typeof setTimeout> | undefined;
  const schedule = (expiresAt: number) => {
    clearTimeout(timer);
    if (!expiresAt) return;
    const delay = Math.max(expiresAt - Date.now() - REFRESH_MARGIN_MS, 5_000);
    timer = setTimeout(() => void refreshSession(), delay);
  };
  const unsubscribe = useAuthStore.subscribe((state, prev) => {
    if (state.accessTokenExpiresAt !== prev.accessTokenExpiresAt) schedule(state.accessTokenExpiresAt);
  });

  const current = useAuthStore.getState();
  if (current.status === "loading") {
    if (readRefreshToken()) void refreshSession();
    else current.clear();
  } else {
    schedule(current.accessTokenExpiresAt);
  }

  const onStorage = (event: StorageEvent) => {
    if (event.key === STORAGE_KEY_REFRESH_TOKEN && event.newValue === null) {
      useAuthStore.getState().clear(false);
    }
  };
  window.addEventListener("storage", onStorage);

  return () => {
    clearTimeout(timer);
    unsubscribe();
    window.removeEventListener("storage", onStorage);
  };
}
