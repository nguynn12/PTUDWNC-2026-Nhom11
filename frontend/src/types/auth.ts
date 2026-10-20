// ============================================================================
// CULINARY BLOG — TS TYPES CỦA MODULE TÀI KHOẢN & XÁC THỰC (TV1)
// Khớp backend /api/v1/auth/* và /api/v1/admin/users (SRS FR-AUTH-001..010, mục 8.1, Phụ lục B).
// ============================================================================

import type { ApiProblemDetails } from "./api";

// ----------------------------------------------------------------------------
// VAI TRÒ & TRẠNG THÁI PHIÊN
// ----------------------------------------------------------------------------

/** SRS 2.3: chỉ 2 role lưu trong DB. Guest = chưa đăng nhập, không phải role. */
export type UserRole = "Author" | "Admin";

/** Trạng thái phiên đăng nhập phía client. */
export type AuthStatus = "loading" | "authenticated" | "unauthenticated";

// ----------------------------------------------------------------------------
// DTO TRẢ VỀ TỪ BACKEND
// ----------------------------------------------------------------------------

/** FR-AUTH-006: GET /auth/me (không có password hash, security stamp hay token). */
export interface UserDto {
  id: string;
  email: string;
  displayName: string;
  avatarUrl: string | null;
  bio: string | null;
  roles: UserRole[];
  emailConfirmed: boolean;
  /** ISO 8601 (DateTimeOffset). */
  createdAt: string;
}

/** Register/login/google/refresh: token ở top-level, không bọc {data} (RESOLVED-CONFLICTS C2). */
export interface AuthResponseDto {
  /** JWT HS256, hạn 15 phút; gửi qua header Authorization: Bearer. */
  accessToken: string;
  /** 32 byte Base64URL, hạn 7 ngày, xoay vòng mỗi lần refresh; gửi trong body (SRS 5.2). */
  refreshToken: string;
  /** Số giây còn hạn của access token. */
  expiresIn: number;
  user: UserDto;
}

/** FR-AUTH-009: luôn 202 kèm thông điệp chung. */
export interface ResendConfirmationResponse {
  message: string;
}

// ----------------------------------------------------------------------------
// REQUEST BODY
// ----------------------------------------------------------------------------

/** FR-AUTH-001: POST /auth/register. Password ≥ 8 ký tự, có hoa, thường, số, ký tự đặc biệt. */
export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
}

/** FR-AUTH-002: POST /auth/login. */
export interface LoginRequest {
  email: string;
  password: string;
}

/** FR-AUTH-003: POST /auth/google với ID token từ Google Identity Services. */
export interface GoogleLoginRequest {
  idToken: string;
  /** Nonce đã truyền vào google.accounts.id.initialize; backend đối chiếu claim "nonce". */
  nonce?: string;
}

/** FR-AUTH-004: POST /auth/refresh. */
export interface RefreshTokenRequest {
  refreshToken: string;
}

/** FR-AUTH-005: POST /auth/logout (không cần access token, luôn 204). */
export interface LogoutRequest {
  refreshToken: string;
}

/** FR-AUTH-007: PATCH /auth/me. Field null hoặc không gửi = giữ nguyên. */
export interface UpdateProfileRequest {
  /** 2–100 ký tự. */
  displayName?: string | null;
  /** URL http/https, tối đa 500 ký tự. */
  avatarUrl?: string | null;
  /** Tối đa 1000 ký tự. */
  bio?: string | null;
}

/** FR-AUTH-008: POST /auth/email/confirm (204, idempotent). */
export interface ConfirmEmailRequest {
  userId: string;
  token: string;
}

/** FR-AUTH-009: POST /auth/email/resend. */
export interface ResendConfirmationRequest {
  email: string;
}

/** Quên mật khẩu: POST /auth/password/forgot (luôn 204). */
export interface ForgotPasswordRequest {
  email: string;
}

/** Đặt lại mật khẩu bằng link trong email: POST /auth/password/reset (204). */
export interface ResetPasswordRequest {
  email: string;
  token: string;
  newPassword: string;
}

/** Đổi mật khẩu khi đã đăng nhập: POST /auth/password/change (Bearer) → 200 + AuthResponseDto mới. */
export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

/** FR-AUTH-010: PATCH /admin/users/{id}/status (chỉ Admin). */
export interface ToggleUserStatusRequest {
  isActive: boolean;
}

// ----------------------------------------------------------------------------
// LỖI (RFC 7807, Phụ lục B)
// ----------------------------------------------------------------------------

/** Mã lỗi backend trả trong trường "type" của Problem Details cho module Auth. */
export type AuthErrorCode =
  | "MALFORMED_REQUEST"
  | "VALIDATION_ERROR"
  | "AUTH_EMAIL_EXISTS"
  | "AUTH_INVALID_CREDENTIALS"
  | "AUTH_TOKEN_INVALID"
  | "AUTH_TOKEN_EXPIRED"
  | "AUTH_REFRESH_TOKEN_EXPIRED"
  | "AUTH_REFRESH_TOKEN_REVOKED"
  | "AUTH_GOOGLE_TOKEN_INVALID"
  | "AUTH_ACCOUNT_DISABLED"
  | "AUTH_ACCOUNT_LOCKED"
  | "AUTH_EMAIL_NOT_CONFIRMED"
  | "AUTH_RESET_TOKEN_INVALID"
  | "RATE_LIMIT_EXCEEDED";

/** Mã lỗi chỉ sinh ở phía client (không tới được máy chủ). */
export type ClientErrorCode = "NETWORK_ERROR" | "BACKEND_UNREACHABLE" | "UNKNOWN_ERROR";

/** Body lỗi có thể thiếu trường (proxy, lỗi mạng) nên mọi trường đều tuỳ chọn. */
export type AuthProblemDetails = Partial<ApiProblemDetails>;
