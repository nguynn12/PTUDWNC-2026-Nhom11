# KẾ HOẠCH PHÁT TRIỂN (PLAN 02): FRONTEND DASHBOARD OVERVIEW UI
> **Người thực hiện:** Tạ Nhật Nguyên (MSSV: 2312704 — Nhóm trưởng)  
> **Phân hệ Giao diện:** Trang Tổng quan Tác giả / Quản trị viên (`/dashboard`)  
> **Tài liệu căn cứ:** [SRS.md](../../requirements/SRS.md) Mục 5.1 (Bảng UI Routes) & Chương 3.3 (`FR-RCP`)

---

## 🎯 MỤC TIÊU & PHẠM VI CHỨC NĂNG

Xây dựng trang Dashboard tổng quan (`/dashboard`) cho người dùng có vai trò `Author` và `Admin`. Trang này đóng vai trò trung tâm điều khiển (Control Center) hiển thị các số liệu thống kê quan trọng, các lối tắt nhanh (Quick Actions) và danh sách bài viết cần chú ý.

- **Đường dẫn Route:** `/dashboard`
- **Chiến lược Rendering:** **CSR (Client-Side Rendering)** kết hợp **Auth Route Protection Middleware** (yêu cầu đăng nhập tài khoản có role `Author` hoặc `Admin`).
- **Các thành phần giao diện chính:**
  1. **Thẻ thống kê bài viết (Metrics Cards):** Tổng số bài viết, Số bài `Published` (Đã xuất bản), Số bài `Draft` (Bản nháp), Số bài `Archived` (Đã lưu trữ).
  2. **Thanh tác vụ nhanh (Quick Actions):** Nút "Tạo công thức mới" (điều hướng sang `/dashboard/recipes/new`), "Quản lý bài viết của tôi" (điều hướng sang `/dashboard/recipes`), "Thùng rác" (điều hướng sang `/admin/recipes/trash` nếu là Admin).
  3. **Bảng bài viết mới cập nhật (Recent Recipes Activity):** Hiển thị 5 bài viết cập nhật gần nhất với badge trạng thái màu sắc trực quan (Xanh: Published, Vàng: Draft, Xám: Archived).

---

## 📋 CÁC BƯỚC THỰC HIỆN CHI TIẾT (6 BƯỚC CHUẨN)

### Bước 1: Phân tích Nghiệp vụ & Thiết kế UI/UX
- **Xác định Quyền truy cập:**
  - Nếu người dùng chưa đăng nhập ➔ NextAuth Middleware tự động redirect về `/auth/login?callbackUrl=/dashboard`.
  - Nếu người dùng là `Author` / `Admin` ➔ Hiển thị giao diện Dashboard.
- **Xác định Dữ liệu cần nạp:**
  - Gọi API lấy danh sách bài viết của user hiện tại: `GET /api/v1/recipes?mine=true&pageSize=100`.
  - Tính toán số lượng theo trạng thái:
    - `totalDraft = recipes.filter(r => r.status === 'Draft').length`
    - `totalPublished = recipes.filter(r => r.status === 'Published').length`
    - `totalArchived = recipes.filter(r => r.status === 'Archived').length`

### Bước 2: Thiết kế TypeScript Types & API Client
- Khai báo kiểu dữ liệu `RecipeSummary` và `DashboardStats` trong `frontend/src/features/dashboard/types/index.ts`:
  ```typescript
  export interface DashboardStats {
    totalRecipes: number;
    publishedCount: number;
    draftCount: number;
    archivedCount: number;
  }
  ```
- Định nghĩa API Service trong `frontend/src/features/dashboard/api/dashboardApi.ts`:
  - Hàm `fetchMyRecipesOverview()` gọi qua Axios Client đã tích hợp Bearer token.

### Bước 3: Quản lý Server State với TanStack Query v5
- Tạo custom hook `useDashboardStats` trong `frontend/src/features/dashboard/hooks/useDashboardStats.ts`:
  - Sử dụng `useQuery({ queryKey: ['dashboard', 'stats'], queryFn: ... })`.
  - Cấu hình `staleTime: 60 * 1000` (1 phút) để giảm thiểu số lần gọi API thừa.

### Bước 4: Xây dựng các Component UI Con (Modular Components)
- `frontend/src/features/dashboard/components/StatsCard.tsx`: Thẻ hiển thị số liệu đẹp mắt với icon, hiệu ứng hover, gradient viền nhẹ.
- `frontend/src/features/dashboard/components/QuickActions.tsx`: Các nút bấm điều hướng nhanh với màu nhấn Primary.
- `frontend/src/features/dashboard/components/RecentRecipesTable.tsx`: Bảng hiển thị danh sách bài viết gần đây, hỗ trợ skeleton loading khi đang tải dữ liệu.

### Bước 5: Lắp ráp Trang Chính (`app/dashboard/page.tsx`)
- Tạo file `frontend/src/app/dashboard/page.tsx`:
  - Khai báo `'use client'`.
  - Kết hợp `StatsCard`, `QuickActions`, và `RecentRecipesTable`.
  - Xử lý trạng thái Loading (Skeleton) và Error (Nút "Thử lại").
  - Giao diện đáp ứng đầy đủ trên Mobile, Tablet, Desktop (Tailwind CSS responsive classes `grid-cols-1 md:grid-cols-3 lg:grid-cols-4`).

### Bước 6: Kiểm thử Giao diện & Trải nghiệm (UI/UX Verification)
- Kiểm tra tính mượt mà khi chuyển trang giữa `/dashboard` và các màn hình con.
- Kiểm tra trạng thái rỗng (Empty State) khi tác giả mới tạo tài khoản chưa có bài viết nào: Hiển thị banner hướng dẫn bấm nút "Tạo công thức đầu tiên của bạn".
- Đảm bảo điểm số Lighthouse về Performance và Accessibility đạt chuẩn cao.

---

## 🎤 BỘ CÂU HỎI VẤN ĐÁP THƯỜNG GẶP (Q&A CHEATSHEET)

**Q1: Tại sao trang `/dashboard` lại chọn chiến lược CSR thay vì SSR hay SSG?**
> *Trả lời:* "Trang `/dashboard` là trang cá nhân hóa chứa dữ liệu riêng tư của từng tác giả (Private Route) và thay đổi thường xuyên theo thời gian thực khi tác giả tạo/sửa bài. Do đó, trang này không cần index SEO của công cụ tìm kiếm, chọn CSR kết hợp TanStack Query v5 giúp client tải nhanh khung layout, hiển thị skeleton tức thì và tự động cache/revalidate dữ liệu mượt mà."

**Q2: Làm sao để ngăn chặn người dùng chưa đăng nhập truy cập vào trang này?**
> *Trả lời:* "Em bảo vệ 2 lớp: Lớp 1 ở Middleware (`middleware.ts` của Auth.js v5) chặn ngay từ Edge Server trước khi render page; Lớp 2 ở Client Component kiểm tra `useSession()` để xử lý fallback an toàn nếu session hết hạn."
