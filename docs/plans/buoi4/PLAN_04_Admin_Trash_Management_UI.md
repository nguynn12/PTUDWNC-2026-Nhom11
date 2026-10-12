# KẾ HOẠCH PHÁT TRIỂN (PLAN 04): FRONTEND ADMIN TRASH MANAGEMENT UI
> **Người thực hiện:** Tạ Nhật Nguyên (MSSV: 2312704 — Nhóm trưởng)  
> **Phân hệ Giao diện:** Quản trị Thùng rác Công thức dành cho Admin (`/admin/recipes/trash`)  
> **Tài liệu căn cứ:** [SRS.md](../../requirements/SRS.md) Mục 5.1 & Chương 3.3 (`FR-RCP-007`)

---

## 🎯 MỤC TIÊU & PHẠM VI CHỨC NĂNG

Xây dựng màn hình Quản trị Thùng rác (`/admin/recipes/trash`) dành riêng cho tài khoản có vai trò Quản trị viên (`Admin`). Màn hình này quản lý toàn bộ các công thức nấu ăn đã bị xóa mềm trong toàn hệ thống, cung cấp cơ chế an toàn để khôi phục hoặc xóa vĩnh viễn:

- **Đường dẫn Route:** `/admin/recipes/trash`
- **Chiến lược Rendering:** **CSR (Client-Side Rendering)** kết hợp **Admin Role Guard** (chỉ tài khoản có quyền `Admin` mới được phép truy cập; nếu tài khoản thường hoặc chưa đăng nhập thì tự động chặn và hiển thị `403 Forbidden` hoặc chuyển hướng).
- **Các tính năng trên giao diện:**
  1. **Danh sách Thùng rác (Trash List):** Bảng hiển thị các công thức đã bị xóa mềm (`IsDeleted == true`), bao gồm: Tiêu đề, Tác giả, Ngày xóa (`DeletedAt`), Số ngày còn lại trước khi hết hạn khôi phục (`DaysRemaining` - đếm ngược từ 30 ngày).
  2. **Hành động Khôi phục (`Restore Action`):** Nút khôi phục đưa bài viết từ thùng rác quay trở lại trạng thái `Draft` của tác giả.
  3. **Hành động Xóa vĩnh viễn (`Purge / Hard Delete Action`):** Nút xóa cứng vĩnh viễn khỏi Database. Yêu cầu hiển thị **Cảnh báo Nguy hiểm (Destructive Modal)** yêu cầu xác nhận trước khi thực thi để tránh mất dữ liệu do vô tình bấm nhầm.
  4. **Phân trang thùng rác:** Hỗ trợ xem nhiều trang khi số lượng bài viết bị xóa lớn.

---

## 📋 CÁC BƯỚC THỰC HIỆN CHI TIẾT (6 BƯỚC CHUẨN)

### Bước 1: Phân tích Nghiệp vụ & Thiết kế UI/UX
- **Xác định API Backend tương ứng:**
  - `GET /api/v1/recipes/trash?pageNumber={page}&pageSize={pageSize}`
  - `POST /api/v1/recipes/{id}/restore`
  - `DELETE /api/v1/recipes/{id}/purge`
- **Thiết kế Cột Đếm ngược Thời gian (`Days Remaining`):**
  - Công thức: `DaysRemaining = 30 - số ngày đã trôi qua kể từ DeletedAt`.
  - Hiển thị badge màu sắc:
    - Còn > 15 ngày: Màu xám/xanh an toàn.
    - Còn 5 - 15 ngày: Màu vàng cảnh báo.
    - Còn < 5 ngày: Màu đỏ nguy cấp sắp bị dọn dẹp tự động.

### Bước 2: Thiết kế Data Types & API Client
- Khai báo Interface `TrashedRecipeItem` trong `frontend/src/features/admin/types/index.ts`:
  ```typescript
  export interface TrashedRecipeItem {
    id: string;
    title: string;
    slug: string;
    status: string;
    authorId: string;
    categoryId: string;
    createdAt: string;
    deletedAt: string;
    daysRemaining: number;
  }
  ```
- Định nghĩa API Service trong `frontend/src/features/admin/api/trashApi.ts`:
  - `fetchTrashedRecipes(pageNumber: number, pageSize: number)`
  - `restoreRecipe(id: string)`
  - `purgeRecipe(id: string)`

### Bước 3: Quản lý Server State với TanStack Query v5
- Xây dựng custom hook `useTrashManagement` trong `frontend/src/features/admin/hooks/useTrashManagement.ts`:
  - `useQuery` lấy danh sách thùng rác kèm phân trang.
  - `useMutation` cho hành động Restore: Gọi `queryClient.invalidateQueries({ queryKey: ['admin', 'trash'] })` khi thành công, bắn Toast "Đã khôi phục bài viết thành công".
  - `useMutation` cho hành động Purge: Gọi invalidate queries khi thành công, bắn Toast "Đã xóa vĩnh viễn bài viết".

### Bước 4: Xây dựng các Component Giao diện Con
- `frontend/src/features/admin/components/TrashTable.tsx`: Bảng danh sách bài viết trong thùng rác với đầy đủ cột thông tin và nút thao tác.
- `frontend/src/features/admin/components/PurgeWarningModal.tsx`: Modal cảnh báo màu đỏ với văn bản giải thích rõ: *"Hành động này không thể hoàn tác. Mọi dữ liệu nguyên liệu, bước làm và hình ảnh liên quan sẽ bị xóa vĩnh viễn khỏi hệ thống."*
- `frontend/src/features/admin/components/RestoreConfirmModal.tsx`: Modal xác nhận khôi phục bài viết.

### Bước 5: Lắp ráp Trang `/admin/recipes/trash/page.tsx`
- Tạo file `frontend/src/app/admin/recipes/trash/page.tsx`:
  - Kiểm tra vai trò Admin từ Session; nếu không phải Admin ➔ Hiển thị màn hình 403 Forbidden thân thiện hoặc redirect.
  - Bố cục gồm: Tiêu đề trang "Quản trị Thùng rác Công thức", Thanh tìm kiếm nhanh, Bảng dữ liệu `TrashTable` và thanh phân trang.
  - Trạng thái rỗng (Empty State): Khi thùng rác trống ➔ Hiển thị icon thùng rác sạch kèm thông điệp "Thùng rác hiện đang trống".

### Bước 6: Kiểm thử Giao diện & Quyền hạn (Verification)
- Kiểm tra phân quyền: Đăng nhập bằng tài khoản `Author` thường và cố tình gõ URL `/admin/recipes/trash` ➔ Kiểm tra hệ thống có chặn đúng và từ chối truy cập hay không.
- Đăng nhập bằng tài khoản `Admin` ➔ Thao tác Khôi phục và Xóa vĩnh viễn bài viết ➔ Kiểm tra phản hồi mượt mà, danh sách tự động cập nhật lại ngay tức thì.

---

## 🎤 BỘ CÂU HỎI VẤN ĐÁP THƯỜNG GẶP (Q&A CHEATSHEET)

**Q1: Cơ chế Xóa mềm (Soft Delete) và Xóa vĩnh viễn (Purge/Hard Delete) khác nhau như thế nào về mặt kiến trúc?**
> *Trả lời:* "Khi người dùng bấm Xóa thông thường, hệ thống chỉ thực hiện **Soft Delete** bằng cách đánh dấu cờ `IsDeleted = true` và gán `DeletedAt = UtcNow`. Lúc này EF Core thông qua Global Query Filter sẽ tự động ẩn bài viết này khỏi mọi truy vấn của người dùng bình thường. Chỉ Quản trị viên trong trang `/admin/recipes/trash` mới có thể dùng `IgnoreQueryFilters()` để xem danh sách này. Còn hành động **Purge (Xóa vĩnh viễn)** sẽ gọi trực tiếp câu lệnh DELETE trong SQL để giải phóng dung lượng và xóa hoàn toàn bản ghi khỏi Database."

**Q2: Tại sao cần có thời hạn 30 ngày cho thùng rác?**
> *Trả lời:* "Thời hạn 30 ngày là chuẩn an toàn dữ liệu phổ biến giúp bảo vệ tác giả trong trường hợp xóa nhầm bài viết tâm huyết. Sau 30 ngày nếu không có yêu cầu khôi phục, bài viết có thể được một tiến trình định kỳ (Background Job / Cron) tự động Purge dọn dẹp sạch sẽ."
