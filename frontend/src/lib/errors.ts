import { isAxiosError } from "axios";

/**
 * Lỗi dùng chung cho toàn bộ frontend (Buổi 6).
 *
 * Backend trả lỗi theo RFC 7807 Problem Details; `type` mang mã lỗi trong SRS Phụ lục B
 * (vd. RECIPE_NOT_FOUND, AUTH_INVALID_CREDENTIALS). Mọi lời gọi API qua `lib/api/axios.ts`
 * đều ném `AppError`, nên chỗ bắt lỗi chỉ cần một kiểu và dùng `getErrorMessage()` để hiển thị.
 */

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
}

/** Mã lỗi phía client (không do backend trả về). */
export const CLIENT_ERROR_CODES = {
  network: "NETWORK_ERROR",
  unknown: "UNKNOWN_ERROR",
} as const;

export interface AppErrorInit {
  /** HTTP status; 0 khi không kết nối được máy chủ. */
  status: number;
  /** Mã lỗi theo SRS Phụ lục B; bỏ trống sẽ suy ra từ `problem.type` hoặc status. */
  code?: string;
  /** Câu hiển thị; bỏ trống sẽ lấy theo `getErrorMessage`. */
  message?: string;
  detail?: string;
  errors?: Record<string, string[]>;
  problem?: ProblemDetails;
}

export class AppError extends Error {
  readonly status: number;
  readonly code: string;
  /** `detail` gốc của backend (có thể rỗng). */
  readonly detail: string;
  /** Lỗi theo từng trường (422): { email: ["..."] }. */
  readonly errors: Record<string, string[]>;
  readonly problem?: ProblemDetails;

  constructor(init: AppErrorInit) {
    const problem = init.problem;
    const code = init.code ?? codeFromProblem(init.status, problem);
    const detail = init.detail ?? problem?.detail ?? "";
    super(init.message ?? resolveMessage(code, init.status, detail || problem?.title));
    this.name = "AppError";
    this.status = init.status;
    this.code = code;
    this.detail = detail;
    this.errors = init.errors ?? problem?.errors ?? {};
    this.problem = problem;
  }

  // Tên cũ còn được dùng trong code của các thành viên (types/api.ts, lib/api-client.ts, lib/auth).
  get errorCode(): string {
    return this.code;
  }
  get validationErrors(): Record<string, string[]> {
    return this.errors;
  }
  get fieldErrors(): Record<string, string[]> {
    return this.errors;
  }
  get problemDetails(): ProblemDetails | undefined {
    return this.problem;
  }

  get isNetworkError(): boolean {
    return this.status === 0;
  }

  /** Lỗi từ Problem Details của backend. */
  static fromProblem(status: number, problem?: ProblemDetails): AppError {
    return new AppError({ status: problem?.status ?? status, problem });
  }

  /** Chuyển mọi lỗi (axios, fetch, lỗi JS) về AppError. */
  static from(error: unknown): AppError {
    if (error instanceof AppError) return error;
    if (isAxiosError<ProblemDetails>(error)) {
      if (error.response) {
        const { status, data } = error.response;
        const problem = data && typeof data === "object" ? data : undefined;
        // Backend luôn trả Problem Details; 5xx không có Problem Details nghĩa là rewrite /api/v1
        // của Next.js không tới được backend (backend chưa chạy).
        if (status >= 500 && !problem?.type) {
          return new AppError({ status: 0, code: "BACKEND_UNREACHABLE" });
        }
        return AppError.fromProblem(status, problem);
      }
      if (error.code === "ERR_CANCELED") {
        return new AppError({ status: 0, code: "REQUEST_CANCELED", message: "Yêu cầu đã bị huỷ." });
      }
      return new AppError({ status: 0, code: CLIENT_ERROR_CODES.network });
    }
    if (error instanceof TypeError) {
      // fetch() ném TypeError khi mất mạng.
      return new AppError({ status: 0, code: CLIENT_ERROR_CODES.network });
    }
    return new AppError({ status: 500, code: CLIENT_ERROR_CODES.unknown });
  }
}

export function isAppError(error: unknown): error is AppError {
  return error instanceof AppError;
}

// ---------------------------------------------------------------------------
// Thông báo tiếng Việt theo mã lỗi (SRS Phụ lục B)
// ---------------------------------------------------------------------------

export const DEFAULT_ERROR_MESSAGE = "Đã có lỗi xảy ra. Vui lòng thử lại.";

const ERROR_MESSAGES: Record<string, string> = {
  // Xác thực & tài khoản
  AUTH_INVALID_CREDENTIALS: "Email hoặc mật khẩu không đúng.",
  AUTH_EMAIL_EXISTS: "Email này đã được đăng ký.",
  AUTH_ACCOUNT_LOCKED: "Tài khoản bị khóa tạm thời do đăng nhập sai nhiều lần. Vui lòng thử lại sau 15 phút.",
  AUTH_ACCOUNT_DISABLED: "Tài khoản đã bị vô hiệu hoá.",
  AUTH_EMAIL_NOT_CONFIRMED: "Email chưa được xác nhận. Vui lòng kiểm tra hộp thư.",
  AUTH_GOOGLE_TOKEN_INVALID: "Không xác thực được tài khoản Google. Vui lòng thử lại.",
  AUTH_TOKEN_INVALID: "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.",
  AUTH_TOKEN_EXPIRED: "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",
  AUTH_REFRESH_TOKEN_EXPIRED: "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",
  AUTH_REFRESH_TOKEN_REVOKED: "Phiên đăng nhập đã bị thu hồi. Vui lòng đăng nhập lại.",
  AUTH_CONFIRMATION_TOKEN_INVALID: "Liên kết xác nhận không hợp lệ hoặc đã hết hạn.",
  AUTH_RESET_TOKEN_INVALID: "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.",
  AUTH_CANNOT_DEACTIVATE_SELF: "Bạn không thể tự vô hiệu hoá tài khoản của mình.",
  USER_NOT_FOUND: "Không tìm thấy người dùng.",

  // Công thức
  RECIPE_NOT_FOUND: "Không tìm thấy công thức.",
  RECIPE_FORBIDDEN: "Bạn không có quyền thao tác trên công thức này.",
  RECIPE_CONCURRENCY_CONFLICT: "Công thức vừa được cập nhật ở nơi khác. Vui lòng tải lại để xem bản mới nhất.",
  RECIPE_PUBLISH_INCOMPLETE: "Công thức chưa đủ thông tin để xuất bản (cần nguyên liệu, các bước và ảnh đại diện).",
  RECIPE_INVALID_STATUS_TRANSITION: "Không thể chuyển công thức sang trạng thái này.",
  RECIPE_ALREADY_PUBLISHED: "Công thức đã được xuất bản.",
  RECIPE_ALREADY_ARCHIVED: "Công thức đã được lưu trữ.",
  RECIPE_NOT_ARCHIVED: "Công thức chưa được lưu trữ.",
  RECIPE_NOT_DELETED: "Công thức không nằm trong thùng rác.",
  RECIPE_RESTORE_WINDOW_EXPIRED: "Đã quá thời hạn khôi phục công thức.",

  // Danh mục
  CATEGORY_NOT_FOUND: "Không tìm thấy danh mục.",
  CATEGORY_NAME_EXISTS: "Tên danh mục đã tồn tại.",
  CATEGORY_DELETE_HAS_RECIPES: "Không thể xoá danh mục đang có công thức.",
  CATEGORY_HAS_RECIPES: "Không thể xoá danh mục đang có công thức.",

  // Tệp
  FILE_SIZE_EXCEEDED: "Tệp vượt quá dung lượng cho phép.",
  FILE_MIME_INVALID: "Định dạng tệp không được hỗ trợ.",

  // Chung
  RATE_LIMIT_EXCEEDED: "Bạn thao tác quá nhanh. Vui lòng thử lại sau ít phút.",
  VALIDATION_ERROR: "Dữ liệu chưa hợp lệ. Vui lòng kiểm tra lại.",
  FORBIDDEN: "Bạn không có quyền thực hiện thao tác này.",
  INTERNAL_SERVER_ERROR: "Máy chủ gặp sự cố. Vui lòng thử lại sau.",
  NETWORK_ERROR: "Không kết nối được máy chủ. Vui lòng kiểm tra mạng và thử lại.",
  BACKEND_UNREACHABLE: "Không kết nối được máy chủ. Vui lòng thử lại sau.",
};

/** Mã lỗi mà `detail` của backend cụ thể hơn câu chung (vd. số phút còn bị khoá). */
const PREFER_DETAIL = new Set(["AUTH_ACCOUNT_LOCKED", "RECIPE_PUBLISH_INCOMPLETE"]);

const STATUS_MESSAGES: Record<number, string> = {
  0: ERROR_MESSAGES.NETWORK_ERROR,
  400: "Yêu cầu không hợp lệ.",
  401: "Bạn cần đăng nhập để tiếp tục.",
  403: ERROR_MESSAGES.FORBIDDEN,
  404: "Không tìm thấy dữ liệu yêu cầu.",
  409: "Dữ liệu đã thay đổi. Vui lòng tải lại trang.",
  422: ERROR_MESSAGES.VALIDATION_ERROR,
  429: ERROR_MESSAGES.RATE_LIMIT_EXCEEDED,
};

function codeFromProblem(status: number, problem?: ProblemDetails): string {
  const type = problem?.type;
  if (type && /^[A-Z][A-Z0-9_]*$/.test(type)) return type;
  if (status === 0) return CLIENT_ERROR_CODES.network;
  if (status === 429) return "RATE_LIMIT_EXCEEDED";
  if (status === 422) return "VALIDATION_ERROR";
  if (status >= 500) return "INTERNAL_SERVER_ERROR";
  return CLIENT_ERROR_CODES.unknown;
}

function resolveMessage(code: string, status: number, detail?: string): string {
  if (detail && PREFER_DETAIL.has(code)) return detail;
  return ERROR_MESSAGES[code] ?? detail ?? STATUS_MESSAGES[status] ?? DEFAULT_ERROR_MESSAGE;
}

/** Câu thông báo theo mã lỗi; dùng khi chỉ có mã (vd. đọc từ query string). */
export function messageFor(code: string | undefined, fallback = DEFAULT_ERROR_MESSAGE): string {
  return (code && ERROR_MESSAGES[code]) || fallback;
}

/**
 * Thông báo tiếng Việt cho người dùng từ một lỗi bất kỳ.
 * Lỗi không phải từ API (lỗi lập trình) trả câu chung để không lộ chi tiết kỹ thuật.
 */
export function getErrorMessage(error: unknown, fallback = DEFAULT_ERROR_MESSAGE): string {
  if (error instanceof AppError) return error.message || fallback;
  if (isAxiosError(error)) return AppError.from(error).message;
  return fallback;
}

/**
 * Gắn lỗi 422 của backend ({ errors: { email: ["..."] } }) vào từng ô của form react-hook-form.
 * Trả về true nếu gắn được ít nhất một ô; false thì nên hiện thông báo chung.
 */
export function applyFieldErrors<TField extends string>(
  error: AppError,
  fields: readonly TField[],
  setError: (field: TField, error: { type: string; message: string }) => void,
): boolean {
  let applied = false;
  for (const field of fields) {
    const message = findFieldError(error.errors, field);
    if (message) {
      setError(field, { type: "server", message });
      applied = true;
    }
  }
  return applied;
}

/** ASP.NET có thể trả khoá "Email" hoặc "email"; so khớp không phân biệt hoa thường. */
function findFieldError(errors: Record<string, string[]>, field: string): string | undefined {
  const key = Object.keys(errors).find((k) => k.toLowerCase() === field.toLowerCase());
  return key ? errors[key]?.[0] : undefined;
}
