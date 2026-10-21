import axios, { type AxiosRequestConfig, type AxiosResponse, type InternalAxiosRequestConfig } from "axios";
import { AppError } from "@/lib/errors";

/**
 * HTTP client DUY NHẤT của frontend (Buổi 6). Mọi module gọi API qua `apiClient`.
 *
 * - Trình duyệt gọi `/api/v1` (rewrite của next.config.ts sang backend, không cần CORS);
 *   phía server gọi thẳng `BACKEND_API_URL`.
 * - Tự gắn `Authorization: Bearer <access token>` (token lấy từ phiên đăng nhập, tự refresh
 *   nếu sắp hết hạn). Truyền `skipAuth: true` cho request không cần/không được gắn token.
 * - Gặp 401 thì refresh một lần rồi gửi lại; refresh thất bại thì đăng xuất và về trang đăng nhập.
 * - Trả thẳng `response.data` (envelope `{ data, meta }` hoặc DTO); lỗi luôn là `AppError`.
 */

const isServer = typeof window === "undefined";

export const API_BASE_URL = isServer
  ? `${process.env.BACKEND_API_URL || "http://localhost:5000"}/api/v1`
  : "/api/v1";

declare module "axios" {
  interface AxiosRequestConfig {
    /** Không gắn access token và không tự refresh khi 401 (dùng cho /auth/login, /auth/refresh...). */
    skipAuth?: boolean;
  }
}

// Không đặt Content-Type mặc định: axios tự đặt application/json cho object
// và để trình duyệt tự đặt multipart/form-data kèm boundary cho FormData.
export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: { Accept: "application/json" },
  timeout: 15_000,
});

// session.ts cũng dùng apiClient, nên nạp động để tránh vòng import.
const loadSession = () => import("@/lib/auth/session");

apiClient.interceptors.request.use(async (config: InternalAxiosRequestConfig) => {
  if (isServer || config.skipAuth || config.headers.has("Authorization")) return config;
  const { getValidAccessToken } = await loadSession();
  const token = await getValidAccessToken();
  if (token) config.headers.set("Authorization", `Bearer ${token}`);
  return config;
});

type RetriableConfig = InternalAxiosRequestConfig & { _retried?: boolean };

apiClient.interceptors.response.use(
  (response: AxiosResponse) => response.data,
  async (error: unknown) => {
    const appError = AppError.from(error);
    const original = axios.isAxiosError(error) ? (error.config as RetriableConfig | undefined) : undefined;

    if (appError.status === 401 && original && !original.skipAuth && !isServer) {
      const session = await loadSession();
      const sentToken = original.headers.has("Authorization");

      // Access token bị từ chối (vd. hết hạn sớm hơn dự tính): refresh một lần rồi gửi lại.
      if (sentToken && !original._retried) {
        original._retried = true;
        if (await session.refreshSession()) {
          original.headers.delete("Authorization");
          return apiClient(original);
        }
      }

      if (sentToken) {
        await session.logout();
        redirectToLogin();
      }
    }

    return Promise.reject(appError);
  },
);

function redirectToLogin() {
  if (window.location.pathname.startsWith("/auth/")) return;
  const callbackUrl = encodeURIComponent(window.location.pathname + window.location.search);
  window.location.assign(`/auth/login?callbackUrl=${callbackUrl}`);
}

/**
 * Gọi API và nhận đúng kiểu dữ liệu T (interceptor đã trả `response.data`).
 * Dùng trong hàm generic, nơi `apiClient.get<never, T>()` không suy ra được kiểu.
 */
export function apiRequest<T>(config: AxiosRequestConfig): Promise<T> {
  return apiClient.request(config) as Promise<unknown> as Promise<T>;
}

export default apiClient;
