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
- `src/store`: Quản lý state toàn cục phía Client với Zustand (`useUIStore.ts`, `useAuthUIStore.ts`).

> **Ghi chú về thư mục `src/features/`**: Nhóm đã thống nhất dọn dẹp thư mục `src/features/` (trước đây chỉ có README) để tránh phân mảnh cấu trúc và tập trung toàn bộ components và types về `src/components` và `src/types`.
