import axios, { type AxiosError, type InternalAxiosRequestConfig } from "axios";
import { AppError, type ApiProblemDetails } from "@/types/api";

const isServer = typeof window === "undefined";
const baseURL = isServer
  ? process.env.BACKEND_API_URL
    ? `${process.env.BACKEND_API_URL}/api/v1`
    : "http://localhost:5000/api/v1"
  : "/api/v1";

export const apiClient = axios.create({
  baseURL,
  headers: {
    "Content-Type": "application/json",
    Accept: "application/json",
  },
  timeout: 15000,
});

// ----------------------------------------------------------------------------
// REQUEST INTERCEPTOR: Tự động gắn Bearer Token nếu có session
// ----------------------------------------------------------------------------
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    if (!isServer) {
      const token = localStorage.getItem("accessToken");
      if (token && !config.headers.Authorization) {
        config.headers.Authorization = `Bearer ${token}`;
      }
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// ----------------------------------------------------------------------------
// RESPONSE INTERCEPTOR: Unwrap envelope & Bóc tách RFC 7807 Problem Details
// ----------------------------------------------------------------------------
apiClient.interceptors.response.use(
  (response) => {
    // Trả về thẳng dữ liệu data (thường chứa { data, meta } theo chuẩn envelope)
    return response.data;
  },
  (error: AxiosError<ApiProblemDetails>) => {
    if (error.response?.data) {
      const problem = error.response.data;
      const appError = new AppError({
        type: problem.type || "API_ERROR",
        title: problem.title || `Lỗi ${error.response.status}`,
        status: problem.status || error.response.status,
        detail:
          problem.detail ||
          error.message ||
          "Đã xảy ra lỗi khi giao tiếp với máy chủ.",
        errors: problem.errors,
      });
      return Promise.reject(appError);
    }

    const networkError = new AppError({
      type: "NETWORK_ERROR",
      title: "Lỗi kết nối",
      status: error.response?.status || 500,
      detail:
        error.message === "Network Error"
          ? "Không thể kết nối đến máy chủ Backend. Vui lòng kiểm tra lại đường truyền."
          : error.message || "Lỗi giao tiếp mạng.",
    });

    return Promise.reject(networkError);
  }
);

export default apiClient;
