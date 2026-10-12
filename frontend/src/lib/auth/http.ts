import axios, { type InternalAxiosRequestConfig } from "axios";
import { getValidAccessToken, logout, refreshSession } from "./session";
import { useAuthStore } from "./store";
import { fromAxiosError } from "./errors";

/**
 * Client cho các API Auth cần đăng nhập (SRS 5.2: Authorization: Bearer <access_token>).
 * Đi qua rewrite /api/v1 của next.config.ts nên không cần CORS. Tự refresh khi token sắp hết hạn.
 */
export const authClient = axios.create({
  baseURL: "/api/v1",
  timeout: 15_000,
  headers: { Accept: "application/json" },
});

authClient.interceptors.request.use(async (config) => {
  const token = await getValidAccessToken();
  if (token) config.headers.set("Authorization", `Bearer ${token}`);
  return config;
});

type RetriableConfig = InternalAxiosRequestConfig & { _retried?: boolean };

authClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error?.config as RetriableConfig | undefined;
    const sentToken = Boolean(original?.headers?.Authorization);

    // Access token bị từ chối (vd. hết hạn sớm hơn dự tính): refresh một lần rồi gửi lại.
    if (error?.response?.status === 401 && original && sentToken && !original._retried) {
      original._retried = true;
      if (await refreshSession()) {
        original.headers.set("Authorization", `Bearer ${useAuthStore.getState().accessToken}`);
        return authClient(original);
      }
    }

    if (error?.response?.status === 401 && sentToken) {
      await logout();
      if (typeof window !== "undefined" && !window.location.pathname.startsWith("/auth/")) {
        const callbackUrl = encodeURIComponent(window.location.pathname + window.location.search);
        window.location.assign(`/auth/login?callbackUrl=${callbackUrl}`);
      }
    }
    return Promise.reject(fromAxiosError(error));
  },
);
