# BÁO CÁO LAB - MÔN PHÁT TRIỂN ỨNG DỤNG WEB NÂNG CAO

**Lab:** 06  
**Thời gian:** 14/10/2026 đến 21/10/2026  
**MSSV:** 2312731  
**Họ và tên:** Nguyễn Phú Quý  
**Nhóm:** 11  

---

## Bảng phân công & Tiến độ công việc

| STT | Công việc được giao | Liên kết đến GitHub branch | Tiến độ |
|---:|---|---|---:|
| 1 | **Thiết kế lại `RecipeCard` bằng bộ component `Card` và `Badge` chuẩn shadcn/ui.**<br><br>**Đã hoàn thành:**<br>• Chuyển đổi toàn bộ thẻ bài viết công thức sang `Card` và `Badge` của shadcn/ui kết hợp Tailwind CSS;<br>• Tích hợp hệ thống Badges trực quan: Danh mục (glassmorphism/blur), Độ khó chế biến (Dễ / Vừa / Khó với bảng màu ẩm thực), Trạng thái công thức (Bản nháp / Lưu trữ);<br>• Hiển thị chỉ số nhanh (thời gian nấu, lượng calo, khẩu phần) kèm vi hiệu ứng hover zoom mượt mà tỷ lệ 16:10;<br>• Loại bỏ 100% các thuộc tính inline styles `style={{...}}`.<br><br>**Chưa hoàn thành:** Không có. | [2312731_NguyenPhuQuy_Recipe-details_frontend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312731_NguyenPhuQuy_Recipe-details_frontend) | 100% |
| 2 | **Thiết kế lại trang danh sách công thức: `RecipeFilterSidebar`, `RecipeToolbar` và `RecipeListWrapper`.**<br><br>**Đã hoàn thành:**<br>• Tái cấu trúc `RecipeFilterSidebar` bằng `Select`, `Input`, `Button`, hỗ trợ đồng bộ searchParams URL và Drawer trượt cảm ứng trên thiết bị di động (Mobile Drawer);<br>• Tái cấu trúc `RecipeToolbar` bằng `Select` và bộ nút chuyển chế độ xem Grid (Lưới) / List (Danh sách);<br>• Cập nhật `RecipeListWrapper` hỗ trợ cả class CSS sẵn có và lưới responsive chuẩn Tailwind (1 cột mobile, 2 cột tablet, 3 cột desktop);<br>• Loại bỏ toàn bộ inline styles tại `app/recipes/page.tsx` và tích hợp `EmptyState` khi không tìm thấy kết quả.<br><br>**Chưa hoàn thành:** Không có. | [2312731_NguyenPhuQuy_Recipe-details_frontend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312731_NguyenPhuQuy_Recipe-details_frontend) | 100% |
| 3 | **Thiết kế lại trang chi tiết công thức `RecipeDetail`: Thư viện ảnh, Khối thông số, Nguyên liệu, Các bước và Dinh dưỡng.**<br><br>**Đã hoàn thành:**<br>• Bổ sung tính năng **Thư viện ảnh (Image Gallery)**: cho phép xem ảnh chính tỉ lệ 16:9 và chọn thumbnail tương tác giữa các ảnh trong album;<br>• Khối thông số nhanh 5 cột (Thời gian sơ chế, Thời gian nấu, Tổng thời gian, Khẩu phần, Năng lượng calo) nổi bật;<br>• Danh sách nguyên liệu thiết kế dạng thẻ `Card` kèm nhãn định lượng bo tròn;<br>• Khối giá trị dinh dưỡng đa vi chất kèm câu trích dẫn bắt buộc theo đặc tả SRS (Dòng 1295);<br>• Hướng dẫn các bước nấu dạng `Card` với số thứ tự bước tròn nổi bật, badge hẹn giờ đếm ngược và ảnh minh họa từng bước.<br><br>**Chưa hoàn thành:** Không có. | [2312731_NguyenPhuQuy_Recipe-details_frontend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312731_NguyenPhuQuy_Recipe-details_frontend) | 100% |
| 4 | **Thiết kế lại các biểu mẫu của `RecipeWizardEditor` bằng `Input`, `Textarea`, `Select`, `Button`, `Tabs`.**<br><br>**Đã hoàn thành:**<br>• Chuyển đổi thanh Stepper 4 bước sang giao diện dạng Tab bar responsive, hiển thị trạng thái hoàn tất (dấu tích xanh) và active rõ ràng;<br>• **Bước 1 (Thông tin chung & Dinh dưỡng):** Tích hợp `Input`, `Select`, `Textarea`, `Button`, `Badge` và lưới 6 ô dinh dưỡng tùy chọn;<br>• **Bước 2 (Danh sách nguyên liệu):** Form nhập định lượng số thực (decimal) và gia vị vừa đủ (nullable) bằng `Input`, `Button`, danh sách thẻ nguyên liệu với nút Sửa/Xóa đồng bộ;<br>• **Bước 3 (Các bước thực hiện):** Form tạo bước và nút Reorder lên/xuống bằng UI primitives;<br>• **Bước 4 (Ảnh MinIO):** Giao diện kéo thả dropzone, xem trước ảnh, chỉnh sửa Alt Text chuẩn SEO/A11y và chọn ảnh đại diện Primary.<br><br>**Chưa hoàn thành:** Không có. | [2312731_NguyenPhuQuy_Recipe-details_frontend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312731_NguyenPhuQuy_Recipe-details_frontend) | 100% |
| 5 | **Cài đặt thư viện UI shadcn, Tailwind CSS và xây dựng các thành phần Skeleton, EmptyState, ErrorState.**<br><br>**Đã hoàn thành:**<br>• Cấu hình Tailwind CSS v3 với `corePlugins: { preflight: false }` để bảo toàn 2135 dòng `globals.css` hiện có, không làm lệch bất kỳ trang nào;<br>• Tạo bộ UI primitives tại `src/components/ui/` (`card`, `badge`, `button`, `input`, `textarea`, `select`, `checkbox`, `tabs`, `skeleton`);<br>• Tạo `src/lib/utils.ts` với hàm `cn()`, `formatDate()`, `formatDuration()`, `formatNumber()`, `buildQueryString()`;<br>• Xây dựng component `EmptyState.tsx` và `ErrorState.tsx` tại `src/components/common/`;<br>• Cập nhật `components/common/Skeleton.tsx`, bổ sung route loading `app/recipes/loading.tsx`.<br><br>**Chưa hoàn thành:** Không có. | [2312731_NguyenPhuQuy_Recipe-details_frontend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312731_NguyenPhuQuy_Recipe-details_frontend) | 100% |

---

## Chi tiết Minh chứng Triển khai

### 1. Bộ thư viện UI primitives & Tiện ích (shadcn/ui + Tailwind)
- **Vị trí:** `frontend/src/components/ui/` và `frontend/src/lib/utils.ts`
- **Các thành phần đã triển khai:**
  - `card.tsx`: Card, CardHeader, CardTitle, CardDescription, CardContent, CardFooter.
  - `badge.tsx`: Hỗ trợ đầy đủ các biến thể ẩm thực (`easy`, `medium`, `hard`, `category`, `subtle`, `statusDraft`, `statusPublished`, `statusArchived`).
  - `button.tsx`: Các biến thể `default`, `outline`, `secondary`, `destructive`, `ghost`, `link`, `subtle`.
  - `input.tsx`, `textarea.tsx`, `select.tsx`, `checkbox.tsx`, `tabs.tsx`, `skeleton.tsx`.
- **Đặc điểm tương thích:** Cấu hình Tailwind với `preflight: false` và ánh xạ các biến màu thương hiệu `--primary` (`#c85a17`), `--secondary` (`#2b7a4b`).

### 2. Thiết kế lại các giao diện Recipe
- **`RecipeCard.tsx`:** Chuyển sang thẻ Card bo tròn 16px, badge nổi góc ảnh, thông tin tác giả và chỉ số calo/thời gian.
- **`RecipeFilterSidebar.tsx` & `RecipeToolbar.tsx`:** Bộ lọc đa tiêu chí (danh mục, độ khó, thời gian tối đa, khoảng calo) với nút đặt lại và hỗ trợ mobile drawer.
- **`RecipeDetail.tsx`:** Trang chi tiết ẩm thực cao cấp, có gallery chọn thumbnail ảnh, danh sách nguyên liệu và hướng dẫn các bước nấu rõ ràng.
- **`RecipeWizardEditor.tsx`:** Trình tạo công thức 4 bước chuẩn SRS v1.3.0 với các control form đồng bộ và đẹp mắt.

### 3. Kết quả Kiểm tra & Đóng gói (Build & Typecheck)
- **TypeScript Typecheck:** `npm run typecheck` ➔ **Code 0 (Passed, 0 errors)**
- **Next.js Production Build:** `npm run build` (Turbopack) ➔ **Code 0 (Passed 100%)**
  - Tất cả 17 routes (Static, Dynamic, ISR) biên dịch hoàn tất thành công.
- **Responsive Design:** Đã kiểm thử bố cục giao diện ở 3 kích thước:
  - Mobile (375px): 1 cột, drawer bộ lọc trượt từ cạnh phải, thumbnail cuộn ngang.
  - Tablet (768px): 2 cột lưới công thức, thanh toolbar tự động co giãn.
  - Desktop (1280px): Bố cục 2 cột (sidebar sticky 280px + content lưới 3 cột), trang chi tiết 2 cột (40% nguyên liệu + 60% các bước).
