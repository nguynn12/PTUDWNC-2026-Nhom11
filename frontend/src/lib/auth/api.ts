import { authClient as api } from "./http";
import type { UpdateProfileRequest, UserDto } from "./types";

/** API hồ sơ và email (FR-AUTH-006..009). Đăng nhập/đăng ký/refresh/đăng xuất nằm ở session.ts. */
export const authApi = {
  me: () => api.get<UserDto>("/auth/me").then((r) => r.data),
  updateProfile: (body: UpdateProfileRequest) =>
    api.patch<UserDto>("/auth/me", body).then((r) => r.data),
  resendConfirmation: (email: string) =>
    api.post<{ message: string }>("/auth/email/resend", { email }).then((r) => r.data),
  confirmEmail: (userId: string, token: string) =>
    api.post("/auth/email/confirm", { userId, token }).then(() => undefined),
};
