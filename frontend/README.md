# Frontend - Culinary Blog Web

Ứng dụng Frontend xây dựng trên nền tảng **Next.js 16 (App Router)** và **TypeScript** strict mode.

## Cấu trúc Dự án Thống nhất (Lab 5)

Theo quyết định thống nhất của nhóm tại Lab 5, toàn bộ cấu trúc dự án được chuẩn hóa theo mô hình phân tầng tập trung:

- `src/app`: Quản lý các tuyến đường (routes), layouts, loading, và error boundaries (`/`, `/recipes`, `/dashboard`, `/auth`, `/categories`, `/admin`).
- `src/components`: Các UI Components phân chia theo miền nghiệp vụ:
  - `components/ui`: Các UI primitives chuẩn **shadcn/ui** (`card`, `badge`, `button`, `input`, `textarea`, `select`, `checkbox`, `tabs`, `skeleton`).
  - `components/recipe` & `components/recipes`: Component danh sách, bộ lọc, chi tiết công thức (`RecipeCard.tsx`, `RecipeFilterSidebar.tsx`, `RecipeToolbar.tsx`, `RecipeListWrapper.tsx`, `RecipeDetail.tsx`), và Multi-step Wizard 4 bước (`RecipeWizardEditor.tsx`).
  - `components/auth`: Form đăng nhập, đăng ký, xác thực tài khoản và hồ sơ người dùng.
  - `components/layout`: Header, Footer, Navbar và UserMenu.
  - `components/common`: Các thành phần tái sử dụng (`EmptyState`, `ErrorState`, `Skeleton`, `Toast`, `ConfirmDialog`, `ConflictDialog`, `Pagination`, `Icons`).
- `src/types`: **Hệ thống Type Definitions tập trung (src/types/)**:
  - `recipe-content.ts`: Chứa toàn bộ types cho Recipe Ingredients, Steps, Images, Nutrition và Wizard payloads.
  - `api.ts`: Chứa các types phân trang, Problem Details, Category và Recipe Summary.
  - `index.ts`: Entrypoint xuất khẩu toàn bộ types của hệ thống, không còn tình trạng khai báo trùng lặp.
- `src/lib`:
  - `utils.ts`: Hàm tiện ích `cn` (kết hợp `clsx` + `tailwind-merge`), `formatDate`, `formatDuration`, `formatNumber`, `buildQueryString`.
  - `api-client.ts`, `api/`: API clients, cấu hình HTTP và auth session.
- `src/hooks`: Custom React hooks phục vụ truy xuất dữ liệu với TanStack React Query.
- `src/store`: Quản lý state toàn cục phía Client với Zustand (`useUIStore.ts`, `useAuthUIStore.ts`).

## Cấu hình Giao diện & shadcn/ui (Lab 6)

- **Tailwind CSS v3**: Cấu hình tại `tailwind.config.ts` với `corePlugins: { preflight: false }` nhằm bảo toàn 100% hệ thống biến và typography của `globals.css` (2135 dòng) mà không gây xung đột hay vỡ bố cục các trang cũ.
- **Bảng màu & Tokens**: Ánh xạ toàn bộ biến theme `--primary` (`#c85a17`), `--secondary` (`#2b7a4b`), `--bg-main`, `--bg-card` vào Tailwind theme colors.
- **Quy chuẩn Component**: Tất cả các component mới và được thiết kế lại đều loại bỏ inline styles `style={{...}}`, sử dụng class Tailwind và các UI primitives từ `@/components/ui/`.
