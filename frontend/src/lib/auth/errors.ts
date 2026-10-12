import type { AxiosError } from "axios";

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly code: string,
    message: string,
    public readonly fieldErrors: Record<string, string[]> = {},
  ) {
    super(message);
    this.name = "ApiError";
  }
}

const MESSAGES: Record<string, string> = {
  AUTH_INVALID_CREDENTIALS: "Email hoặc mật khẩu không đúng.",
  AUTH_EMAIL_EXISTS: "Email này đã được đăng ký.",
  AUTH_ACCOUNT_LOCKED: "Tài khoản bị khóa tạm thời do đăng nhập sai nhiều lần. Vui lòng thử lại sau 15 phút.",
  AUTH_ACCOUNT_DISABLED: "Tài khoản đã bị vô hiệu hoá.",
  AUTH_EMAIL_NOT_CONFIRMED: "Email chưa được xác nhận. Vui lòng kiểm tra hộp thư.",
  AUTH_GOOGLE_TOKEN_INVALID: "Không xác thực được tài khoản Google. Vui lòng thử lại.",
  AUTH_TOKEN_INVALID: "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.",
  AUTH_TOKEN_EXPIRED: "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",
  RATE_LIMIT_EXCEEDED: "Bạn thao tác quá nhanh. Vui lòng thử lại sau ít phút.",
  VALIDATION_ERROR: "Dữ liệu chưa hợp lệ. Vui lòng kiểm tra lại.",
  BACKEND_UNREACHABLE: "Không kết nối được máy chủ. Vui lòng thử lại sau.",
  NETWORK_ERROR: "Không kết nối được máy chủ. Vui lòng thử lại sau.",
};

export const DEFAULT_ERROR_MESSAGE = "Đã có lỗi xảy ra. Vui lòng thử lại.";

export function messageFor(code: string | undefined, fallback = DEFAULT_ERROR_MESSAGE): string {
  return (code && MESSAGES[code]) || fallback;
}

export function fromProblem(status: number, body?: ProblemDetails): ApiError {
  const code = body?.type && /^[A-Z_]+$/.test(body.type) ? body.type : "UNKNOWN_ERROR";
  // FR-AUTH-002 A2: thông báo khoá tài khoản kèm số phút còn lại do backend tính.
  const message =
    code === "AUTH_ACCOUNT_LOCKED" && body?.detail
      ? body.detail
      : messageFor(code, body?.detail ?? DEFAULT_ERROR_MESSAGE);
  return new ApiError(status, code, message, body?.errors);
}

export function fromAxiosError(error: unknown): ApiError {
  if (error instanceof ApiError) return error;
  const e = error as AxiosError<ProblemDetails>;
  if (e.response) return fromProblem(e.response.status, e.response.data);
  return new ApiError(0, "NETWORK_ERROR", MESSAGES.NETWORK_ERROR);
}
