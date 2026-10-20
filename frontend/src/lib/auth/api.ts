import { apiClient } from "@/lib/api/axios";
import type {
  ForgotPasswordRequest,
  ResendConfirmationResponse,
  ResetPasswordRequest,
  UpdateProfileRequest,
  UserDto,
} from "@/types/auth";

/**
 * API hồ sơ, email và mật khẩu (FR-AUTH-006..009, quên/đặt lại mật khẩu).
 * Đăng nhập/đăng ký/refresh/đăng xuất/đổi mật khẩu (có cấp token mới) nằm ở session.ts.
 */
export const authApi = {
  me: () => apiClient.get<never, UserDto>("/auth/me"),
  updateProfile: (body: UpdateProfileRequest) => apiClient.patch<never, UserDto>("/auth/me", body),
  resendConfirmation: (email: string) =>
    apiClient.post<never, ResendConfirmationResponse>("/auth/email/resend", { email }),
  confirmEmail: (userId: string, token: string) =>
    apiClient.post("/auth/email/confirm", { userId, token }).then(() => undefined),
  /** Luôn thành công (204) dù email có tồn tại hay không — backend chống dò tài khoản. */
  forgotPassword: (email: string) =>
    apiClient
      .post("/auth/password/forgot", { email } satisfies ForgotPasswordRequest, { skipAuth: true })
      .then(() => undefined),
  resetPassword: (body: ResetPasswordRequest) =>
    apiClient.post("/auth/password/reset", body, { skipAuth: true }).then(() => undefined),
};
