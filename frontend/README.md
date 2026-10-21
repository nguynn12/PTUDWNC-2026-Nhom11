# Frontend - Culinary Blog Web

Ứng dụng Frontend xây dựng trên nền tảng **Next.js 16 (App Router)** và **TypeScript** strict mode.

## Cấu trúc Dự án Thống nhất (Lab 5)

Theo quyết định thống nhất của nhóm tại Lab 5, toàn bộ cấu trúc dự án được chuẩn hóa theo mô hình phân tầng tập trung:

- `src/app`: Quản lý các tuyến đường (routes), layouts, loading, và error boundaries (`/`, `/recipes`, `/dashboard`, `/auth`, `/categories`, `/admin`).
- `src/components`: Các UI Components phân chia theo miền nghiệp vụ:
  - `components/recipes`: Component chi tiết công thức, Multi-step Wizard 4 bước (`RecipeWizardEditor.tsx`).
  - `components/auth`: Form đăng nhập, đăng ký, xác thực tài khoản và hồ sơ người dùng.
  - `components/layout`: Header, Footer, Navbar và UserMenu.
  - `components/common`: Các thành phần tái sử dụng (Toast, Dialog, Icons).
- `src/types`: **Hệ thống Type Definitions tập trung (src/types/)**:
  - `recipe-content.ts`: Chứa toàn bộ types cho Recipe Ingredients, Steps, Images, Nutrition và Wizard payloads.
  - `api.ts`: Chứa các types phân trang, Problem Details, Category và Recipe Summary.
  - `index.ts`: Entrypoint xuất khẩu toàn bộ types của hệ thống, không còn tình trạng khai báo trùng lặp.
- `src/lib`: API clients (`api-client.ts`), cấu hình HTTP và auth session.
- `src/hooks`: Custom React hooks phục vụ truy xuất dữ liệu với TanStack React Query.
- `src/store`: Quản lý state toàn cục phía Client với Zustand (`auth-store.ts`, `ui-store.ts`, `auth-ui-store.ts`).

> **Ghi chú về thư mục `src/features/`**: Nhóm đã thống nhất dọn dẹp thư mục `src/features/` (trước đây chỉ có README) để tránh phân mảnh cấu trúc và tập trung toàn bộ components và types về `src/components` và `src/types`.

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

## Giao diện (shadcn/ui)

- Dùng component có sẵn trong `src/components/ui` (Button, Card, Input, Label, Alert, Avatar, Badge, DropdownMenu...) và class Tailwind; không tạo thêm CSS module mới.
- Ghép className bằng `cn()` trong `src/lib/utils.ts` để class truyền vào ghi đè được class mặc định.
- Kiểu mặc định của thẻ HTML trong `globals.css` nằm trong `@layer base`, nên utility Tailwind luôn ghi đè được.
- Ví dụ module Xác thực: `components/auth` (Field, PasswordField, FormAlert, SubmitButton, UserAvatar, RoleBadge) và `components/layout/UserMenu.tsx` (DropdownMenu + Avatar).

### Khu vực tài khoản (`/profile`)

- Token màu theo phạm vi: class `.cb-account` (cuối `globals.css`) ghi đè biến shadcn (`--primary`, `--border`, `--input`...) chỉ trong trang hồ sơ, nên không làm đổi màu các trang khác. Hộp thoại portal ra ngoài phải mang cùng class (`PROFILE_THEME_CLASS` trong `components/auth/profile-theme.ts`).
- Class dùng chung (nút chính/phụ, khối nội dung, chiều cao ô nhập 44px) ở `components/auth/profile-styles.ts`; dùng thay vì viết lại class.
- Mọi thay đổi hồ sơ/ảnh đi qua `hooks/useProfileMutations.ts` để header (`auth-store`) và trang hồ sơ (query `["me"]`) luôn cùng dữ liệu.
- Ảnh đại diện: backend chưa có endpoint tải ảnh. Nút "Chọn ảnh" chỉ hiện khi `NEXT_PUBLIC_AVATAR_UPLOAD_ENABLED=true` (xem `.env.example`) và backend có `POST /auth/me/avatar`; xoá ảnh dùng `PATCH /auth/me` với `avatarUrl: ""`.
- Chặn rời trang khi còn thay đổi chưa lưu: `hooks/useUnsavedChangesGuard.ts`.

## Tiện ích dùng chung

`src/lib/utils.ts`: `cn()` ghép className, `formatDate()` (dd/MM/yyyy, giờ Việt Nam), `formatDuration()` (80 → "1 giờ 20 phút"), `formatNumber()` (1.234.567), `buildQueryString()` (bỏ qua giá trị rỗng).
