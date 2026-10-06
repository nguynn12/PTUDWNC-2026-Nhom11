# KẾ HOẠCH PHÁT TRIỂN (PLAN 03): FRONTEND MY RECIPES & LIFECYCLE ACTIONS UI
> **Người thực hiện:** Tạ Nhật Nguyên (MSSV: 2312704 — Nhóm trưởng)  
> **Phân hệ Giao diện:** Quản lý Bài viết của tôi & Thao tác Vòng đời (`/dashboard/recipes`)  
> **Tài liệu căn cứ:** [SRS.md](../../requirements/SRS.md) Mục 5.1, Chương 3.3 (`FR-RCP-003..007`) & Phụ lục D (Concurrency Control)

---

## 🎯 MỤC TIÊU & PHẠM VI CHỨC NĂNG

Xây dựng màn hình danh sách bài viết do chính tác giả tạo ra (`/dashboard/recipes`), cung cấp đầy đủ các công cụ quản lý toàn bộ vòng đời bài viết (Lifecycle Actions) và xử lý chuyên sâu kiểm soát xung đột dữ liệu Optimistic Concurrency:

- **Đường dẫn Route:** `/dashboard/recipes`
- **Chiến lược Rendering:** **CSR (Client-Side Rendering)**.
- **Các tính năng & Hành động trên giao diện:**
  1. **Bộ lọc & Phân trang:** Lọc theo trạng thái (`All`, `Draft`, `Published`, `Archived`), tìm kiếm theo tên bài viết cá nhân, phân trang.
  2. **Các nút thao tác Vòng đời (Action Buttons / Dropdown):**
     - **Xuất bản (`Publish`):** Kích hoạt kiểm tra điều kiện xuất bản (đủ nguyên liệu, bước làm).
     - **Hủy xuất bản (`Unpublish`):** Đưa bài viết về `Draft`.
     - **Lưu trữ (`Archive`):** Ẩn bài viết cũ sang `Archived`.
     - **Mở lưu trữ (`Unarchive`):** Đưa bài viết đã lưu trữ về `Draft`.
     - **Chỉnh sửa (`Edit`):** Điều hướng tới `/dashboard/recipes/[id]/edit`.
     - **Xóa mềm (`Delete`):** Mở Dialog xác nhận chuyển bài viết vào Thùng rác.
  3. **Xử lý Optimistic Concurrency (`If-Match` & `xmin`):**
     - Đính kèm header `If-Match: "{xmin}"` khi gọi API cập nhật hoặc đổi trạng thái.
     - Khi Backend trả về `409 Conflict` (`RECIPE_CONCURRENCY_CONFLICT`), giao diện tự động bật **Conflict Resolution Modal** thông báo cho tác giả và cung cấp nút "Tải lại dữ liệu mới nhất".

---

## 📋 CÁC BƯỚC THỰC HIỆN CHI TIẾT (6 BƯỚC CHUẨN)

### Bước 1: Phân tích Nghiệp vụ & Thiết kế Luồng Tương tác UI
- **Xác định các API Backend tương ứng:**
  - `GET /api/v1/recipes?mine=true&status={status}&page={page}`
  - `PATCH /api/v1/recipes/{id}/publish` kèm header `If-Match`
  - `PATCH /api/v1/recipes/{id}/unpublish` kèm header `If-Match`
  - `PATCH /api/v1/recipes/{id}/archive` kèm header `If-Match`
  - `PATCH /api/v1/recipes/{id}/unarchive` kèm header `If-Match`
  - `DELETE /api/v1/recipes/{id}` kèm header `If-Match`
- **Thiết kế Modal Xác nhận & Modal Xung đột:**
  - *Confirm Dialog:* Hỏi lại người dùng trước khi Xóa / Lưu trữ / Hủy xuất bản.
  - *Conflict Dialog:* Hiển thị khi gặp lỗi 409, giải thích rằng "Dữ liệu đã bị thay đổi bởi phiên làm việc khác. Bạn muốn tải lại dữ liệu mới nhất không?".

### Bước 2: Thiết kế Data Types & API Mutation Services
- Khai báo Interface `RecipeItem` và `LifecycleActionPayload` trong `frontend/src/features/recipes/types/index.ts`:
  ```typescript
  export interface RecipeItem {
    id: string;
    title: string;
    slug: string;
    status: 'Draft' | 'Published' | 'Archived';
    concurrencyToken?: number;
    updatedAt: string;
    createdAt: string;
  }
  ```
- Định nghĩa các hàm Mutation trong `frontend/src/features/recipes/api/recipeLifecycleApi.ts`:
  - `publishRecipe(id: string, concurrencyToken?: number)`
  - `unpublishRecipe(id: string, concurrencyToken?: number)`
  - `archiveRecipe(id: string, concurrencyToken?: number)`
  - `unarchiveRecipe(id: string, concurrencyToken?: number)`
  - `deleteRecipe(id: string, concurrencyToken?: number)`

### Bước 3: Quản lý Mutation State với TanStack Query v5
- Xây dựng custom hook `useRecipeLifecycleMutations` trong `frontend/src/features/recipes/hooks/useRecipeLifecycleMutations.ts`:
  - `useMutation` cho từng hành động.
  - `onSuccess`: Tự động gọi `queryClient.invalidateQueries({ queryKey: ['my-recipes'] })` và bắn thông báo thành công (Toast notification).
  - `onError`: Kiểm tra nếu `error.response?.status === 409` ➔ Mở `ConflictModal` và lưu ID bài viết xung đột. Nếu `error.response?.status === 422` ➔ Hiển thị Toast cảnh báo bài viết chưa đủ điều kiện xuất bản.

### Bước 4: Xây dựng các Component Giao diện
- `frontend/src/features/recipes/components/RecipeStatusBadge.tsx`: Badge màu sắc tương ứng từng trạng thái.
- `frontend/src/features/recipes/components/RecipeLifecycleActions.tsx`: Menu hành động thông minh (chỉ hiển thị nút Publish khi đang là Draft, chỉ hiển thị Archive khi là Published/Draft...).
- `frontend/src/features/recipes/components/ConcurrencyConflictModal.tsx`: Modal xử lý xung đột 409 Conflict.
- `frontend/src/features/recipes/components/DeleteConfirmModal.tsx`: Modal xác nhận xóa mềm.

### Bước 5: Lắp ráp Trang `/dashboard/recipes/page.tsx`
- Xây dựng bố cục danh sách:
  - Thanh tìm kiếm và Tabs lọc trạng thái (`Tất cả`, `Bản nháp`, `Đã xuất bản`, `Đã lưu trữ`).
  - Nút "Tạo công thức mới" nổi bật.
  - Bảng danh sách hoặc Grid Card (chuyển đổi linh hoạt qua Zustand `useUIStore`).
  - Phân trang phân đoạn (Pagination bar).

### Bước 6: Kiểm thử Trải nghiệm & Xử lý Lỗi (Verification)
- Kiểm thử luồng xuất bản: Bấm Publish một bài nháp thiếu nguyên liệu ➔ Kiểm tra xem Toast lỗi 422 có hiển thị thông báo rõ ràng không.
- Kiểm thử luồng xóa mềm: Bấm Delete ➔ Bài viết biến mất khỏi `/dashboard/recipes` và xuất hiện trong thùng rác `/admin/recipes/trash`.
- Kiểm thử giả lập xung đột Concurrency 409: Mở 2 tab cùng sửa 1 bài, tab 1 lưu trước, tab 2 lưu sau ➔ Tab 2 bật Modal cảnh báo 409 thành công.

---

## 🎤 BỘ CÂU HỎI VẤN ĐÁP THƯỜNG GẶP (Q&A CHEATSHEET)

**Q1: Trên giao diện Frontend, em xử lý lỗi 409 Conflict như thế nào để người dùng không bị bối rối?**
> *Trả lời:* "Khi Frontend nhận phản hồi HTTP 409 kèm mã lỗi `RECIPE_CONCURRENCY_CONFLICT`, thay vì chỉ báo lỗi chung chung, em bắt riêng mã này để kích hoạt một Modal thông báo: *'Dữ liệu công thức này vừa được cập nhật từ một thiết bị hoặc phiên làm việc khác. Để tránh mất dữ liệu, vui lòng bấm Tải lại để xem phiên bản mới nhất'*. Khi người dùng bấm nút 'Tải lại', hệ thống sẽ tự động gọi lại API để refresh dữ liệu và cập nhật lại mã `xmin` mới nhất."

**Q2: Tại sao các nút hành động (Publish, Archive, Delete) lại phải phụ thuộc vào trạng thái hiện tại của Recipe?**
> *Trả lời:* "Để đảm bảo UX tốt nhất và tránh việc người dùng gửi các request không hợp lệ lên Server (State Machine violation), giao diện sẽ dựa vào trường `status` của Recipe để hiển thị hành động phù hợp: Ví dụ bài viết đang ở trạng thái `Draft` thì chỉ hiện nút *Xuất bản* hoặc *Lưu trữ*, không hiện nút *Hủy xuất bản*."
