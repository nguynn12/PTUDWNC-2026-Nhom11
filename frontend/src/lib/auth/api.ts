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
  /**
   * Xoá ảnh đại diện. PATCH /auth/me coi avatarUrl = null là "giữ nguyên" nhưng chuỗi rỗng là giá trị hợp lệ
   * (validator bỏ qua, ApplicationUser.UpdateProfile ghi lại chuỗi rỗng), nên "" là cách xoá hiện có.
   */
  removeAvatar: () => apiClient.patch<never, UserDto>("/auth/me", { avatarUrl: "" } satisfies UpdateProfileRequest),
  /**
   * CHƯA CÓ TRÊN BACKEND. Hợp đồng đề xuất: POST /auth/me/avatar (Bearer), multipart/form-data, trường "file"
   * (JPG/PNG/WebP, tối đa 5 MB) → 200 + UserDto đã có avatarUrl mới, lưu qua IFileStorageService.
   * Chỉ được gọi khi AVATAR_UPLOAD_ENABLED (lib/auth/avatar.ts).
   */
  uploadAvatar: (file: File, onProgress?: (percent: number) => void) => {
    const form = new FormData();
    form.append("file", file);
    return apiClient.post<never, UserDto>("/auth/me/avatar", form, {
      timeout: 60_000,
      onUploadProgress: (event) => {
        if (onProgress && event.total) onProgress(Math.round((event.loaded / event.total) * 100));
      },
    });
  },
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
