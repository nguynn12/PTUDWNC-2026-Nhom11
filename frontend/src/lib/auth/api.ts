import { apiClient } from "@/lib/api/axios";
import type { ResendConfirmationResponse, UpdateProfileRequest, UserDto } from "@/types/auth";

/** API hồ sơ và email (FR-AUTH-006..009). Đăng nhập/đăng ký/refresh/đăng xuất nằm ở session.ts. */
export const authApi = {
  me: () => apiClient.get<never, UserDto>("/auth/me"),
  updateProfile: (body: UpdateProfileRequest) => apiClient.patch<never, UserDto>("/auth/me", body),
  resendConfirmation: (email: string) =>
    apiClient.post<never, ResendConfirmationResponse>("/auth/email/resend", { email }),
  confirmEmail: (userId: string, token: string) =>
    apiClient.post("/auth/email/confirm", { userId, token }).then(() => undefined),
};
