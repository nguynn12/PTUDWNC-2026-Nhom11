# Frontend

Frontend dùng Next.js App Router và TypeScript strict mode.

## Tổ chức code

- `src/app`: routes, layouts và route-level loading/error UI.
- `src/features`: module nghiệp vụ theo lát cắt dọc (`auth`, `categories`, `recipes`, `search`).
- `src/components`: UI dùng chung, không chứa business rules.
- `src/lib`: API client và utility dùng chung.

Khi thêm một module, ưu tiên đặt component, schema, service và type riêng trong `src/features/<module>` thay vì gom tất cả theo loại file ở cấp toàn dự án.

## Gọi API

Chỉ có **một** HTTP client: `apiClient` trong `src/lib/api/axios.ts`. Không tạo `axios.create()` hay `fetch()` riêng.

- Trình duyệt gọi `/api/v1/...` (rewrite sang backend trong `next.config.ts`); phía server gọi `BACKEND_API_URL`.
- Tự gắn `Authorization: Bearer <access token>` và tự refresh khi gặp 401; refresh thất bại thì đăng xuất và về `/auth/login`. Request không được gắn token truyền `{ skipAuth: true }`.
- Interceptor trả thẳng `response.data`, nên khai báo kiểu ở tham số thứ hai: `apiClient.get<never, ApiResponse<RecipeSummaryDto[]>>("/recipes")`. Trong hàm generic dùng `apiRequest<T>(config)`.
- Mỗi module viết hàm gọi API trong `src/lib/api/<module>.ts` (hoặc `src/lib/<module>/api.ts`), component chỉ gọi qua TanStack Query.

## Xử lý lỗi

Mọi lỗi từ `apiClient` là `AppError` (`src/lib/errors.ts`) gồm `status` (0 = không kết nối được backend), `code` (mã lỗi SRS Phụ lục B, vd. `RECIPE_NOT_FOUND`), `detail` và `errors` (lỗi từng trường).

- Hiển thị cho người dùng: `getErrorMessage(error)` trả câu tiếng Việt theo mã lỗi.
- Lỗi 422 của form react-hook-form: `applyFieldErrors(error, ["email", "password"], setError)`.
- Thêm mã lỗi mới thì bổ sung câu thông báo vào `ERROR_MESSAGES` trong `src/lib/errors.ts`.

## Store (Zustand)

- Tên file kebab-case, đuôi `-store.ts`: `auth-store.ts`, `ui-store.ts`, `auth-ui-store.ts`. Hook export tên `use<Tên>Store`.
- Khai báo rõ phần state và phần action trong interface; action đặt tên theo động từ (`setSession`, `clearSession`).
- Đọc store bằng selector để tránh render thừa: `useAuthStore((s) => s.user)` hoặc selector có sẵn (`useCurrentUser()`, `useAuthStatus()`, `useHasRole("Admin")`). Không gọi `useAuthStore()` không có selector.
- Trạng thái đăng nhập chỉ đọc từ `src/store/auth-store.ts`; access token nằm trong bộ nhớ, không đọc/ghi `localStorage` trực tiếp.

## Tiện ích dùng chung

`src/lib/utils.ts`: `cn()` ghép className, `formatDate()` (dd/MM/yyyy, giờ Việt Nam), `formatDuration()` (80 → "1 giờ 20 phút"), `formatNumber()` (1.234.567), `buildQueryString()` (bỏ qua giá trị rỗng).
