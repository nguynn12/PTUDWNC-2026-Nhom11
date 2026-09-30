# KẾ HOẠCH & BẢNG PHÂN CÔNG NHIỆM VỤ BUỔI 4 (LAB 4)
**Dự án:** Culinary Blog — PTUDWNC-2026-Nhom11  
**Tài liệu căn cứ chuẩn duy nhất:** [SRS.md](./SRS.md) (Chương 3: Yêu cầu chức năng, Chương 5.1: Giao diện người dùng UI, Chương 7: Mô hình dữ liệu, Chương 8: Đặc tả REST API - Chuẩn hóa).  
*(Lưu ý: File PDF Chương 4 đóng vai trò tài liệu tham khảo kỹ thuật về kiến trúc Next.js 15, TanStack Query v5, Auth.js v5, React Hook Form + Zod, Zustand; toàn bộ phạm vi nghiệp vụ và màn hình UI đều được chuẩn hóa 100% theo SRS).*

---

## 📊 BẢNG TỔNG HỢP PHÂN CÔNG THEO 4 THÀNH VIÊN (BUỔI 4)

| Thành viên | MSSV | Phân hệ phụ trách (Backend ➔ Frontend) | Số API Backend | Màn hình UI phụ trách theo SRS (Mục 5.1) |
|---|---|---|:---:|---|
| **1. Liêng Hót Ha Luyến** | 2312682 | **Xác thực & Quản lý Người dùng** (Auth & User Profile) | **10** | • `/auth/login` (CSR)<br>• `/auth/register` (CSR)<br>• `/profile` (CSR)<br>• Cấu hình Auth.js v5, Session, Middleware bảo vệ route |
| **2. Trần Quốc Quân** | 2312726 | **Danh mục, Khám phá Công thức & Giám sát Hệ thống** | **10**  | • `/` Trang chủ (ISR 3600s)<br>• `/categories` & `/categories/[slug]` (ISR)<br>• `/recipes` Danh sách, Lọc, Sắp xếp (SSR)<br>• `/search` Tìm kiếm FTS tiếng Việt (SSR/CSR)<br>• `/recipes/[slug]` Chi tiết công thức, SEO JSON-LD (ISR 300s)<br>• `/dashboard/categories` Quản trị danh mục Admin (CSR) |
| **3. Tạ Nhật Nguyên** | 2312704 | **Vòng đời Công thức, Thùng rác & Sẵn sàng Nhận tải** | **11** | • `/dashboard` Trang tổng quan Author/Admin (CSR)<br>• `/dashboard/recipes` Quản lý bài viết của tôi, hành động Publish/Archive (CSR)<br>• `/admin/recipes/trash` Quản trị Thùng rác Admin, Khôi phục & Xóa vĩnh viễn (CSR)<br>• Xử lý Concurrency Header `If-Match: "{xmin}"` |
| **4. Nguyễn Phú Quý** | 2312731 | **Nội dung Chi tiết Công thức, Media Storage & Upload MinIO** | **10** | • `/dashboard/recipes/new` Form tạo công thức Multi-step Wizard (CSR)<br>• `/dashboard/recipes/[id]/edit` Form chỉnh sửa chi tiết (CSR)<br>• Dynamic Form Arrays: Nguyên liệu & Các bước làm<br>• Module Upload & Quản lý Album ảnh MinIO |
| **TỔNG CỘNG** | | | **41** | **Toàn bộ 14 Màn hình / Routes theo đặc tả SRS** |

---

## 🌐 PHẦN I: DANH SÁCH 41 REST API BACKEND CẦN HOÀN THIỆN (SRS CHƯƠNG 8)

### 👤 1. THÀNH VIÊN 1: LIÊNG HÓT HA LUYẾN (10 API)
*Phân hệ: Xác thực và Quản lý Người dùng (`FR-AUTH-001` đến `FR-AUTH-007`)*

| STT | Method | Endpoint Path | Phân quyền | Mã FR | Mô tả nghiệp vụ theo SRS |
|:---:|:---:|---|---|:---:|---|
| 1 | `POST` | `/api/v1/auth/register` | Public | `FR-AUTH-001` | Đăng ký tài khoản người dùng, mã hóa mật khẩu, gán quyền `Author`, gửi email kích hoạt. |
| 2 | `POST` | `/api/v1/auth/login` | Public | `FR-AUTH-002` | Đăng nhập Email/Password, sinh JWT Access Token (15 phút) và Refresh Token (7 ngày). |
| 3 | `POST` | `/api/v1/auth/google` | Public | `FR-AUTH-003` | Đăng nhập Google OAuth 2.0 bằng Google ID Token, tự động tạo tài khoản và cấp token. |
| 4 | `POST` | `/api/v1/auth/refresh` | Refresh token | `FR-AUTH-004` | Cấp mới Access Token qua cơ chế Token Rotation và Reuse Detection chống đánh cắp phiên. |
| 5 | `POST` | `/api/v1/auth/logout` | Refresh token | `FR-AUTH-005` | Đăng xuất và thu hồi (Revoke) Refresh Token trong DB (`RevokedAt = now()`). |
| 6 | `POST` | `/api/v1/auth/email/confirm` | Public token | `FR-AUTH-001` | Xác thực email người dùng qua confirmation token gửi qua mail. |
| 7 | `POST` | `/api/v1/auth/email/resend` | Public | `FR-AUTH-001` | Tạo confirmation token mới và gửi lại email kích hoạt cho tài khoản. |
| 8 | `GET` | `/api/v1/auth/me` | Bearer (JWT) | `FR-AUTH-006` | Lấy thông tin hồ sơ của người dùng hiện tại từ Claims. |
| 9 | `PATCH` | `/api/v1/auth/me` | Bearer (JWT) | `FR-AUTH-007` | Cập nhật hồ sơ cá nhân: DisplayName, Bio, Avatar. |
| 10 | `PATCH` | `/api/v1/admin/users/{id}/status` | Admin | `FR-AUTH-002` | Quản trị viên khóa/mở khóa trạng thái tài khoản người dùng (`LockoutEnd`). |

---

### 👤 2. THÀNH VIÊN 2: TRẦN QUỐC QUÂN (10 API)
*Phân hệ: Quản lý Danh mục, Khám phá Công thức & Giám sát Dịch vụ (`FR-CAT`, `FR-SRCH`, `FR-RCP-001/002`, `FR-OBS`)*

| STT | Method | Endpoint Path | Phân quyền | Mã FR | Mô tả nghiệp vụ theo SRS |
|:---:|:---:|---|---|:---:|---|
| 1 | `GET` | `/api/v1/categories` | Public | `FR-CAT-001` | Xem danh sách danh mục active kèm số lượng recipe (`RecipeCount`), đệm Redis 30 phút. |
| 2 | `GET` | `/api/v1/categories/{slug}` | Public | `FR-CAT-002` | Xem chi tiết danh mục theo Slug qua B-Tree Index. |
| 3 | `POST` | `/api/v1/categories` | Admin | `FR-CAT-003` | Tạo mới danh mục, tự động tạo slug, kiểm tra trùng lặp và xóa cache Redis. |
| 4 | `PUT` | `/api/v1/categories/{id}` | Admin | `FR-CAT-004` | Cập nhật danh mục, kiểm tra concurrency qua PostgreSQL `xmin`, xóa cache Redis. |
| 5 | `DELETE` | `/api/v1/categories/{id}` | Admin | `FR-CAT-005` | Xóa mềm danh mục; chặn xóa trả về `409 Conflict` nếu danh mục còn công thức active. |
| 6 | `GET` | `/api/v1/recipes` | Public / JWT (`mine=true`) | `FR-RCP-001`<br>`FR-SRCH-002..004` | Danh sách công thức: phân trang chuẩn, lọc đa tiêu chí (danh mục, độ khó, thời gian, calo), sắp xếp linh hoạt. |
| 7 | `GET` | `/api/v1/recipes/search` | Public | `FR-SRCH-001` | Tìm kiếm toàn văn bản PostgreSQL FTS (`tsvector`, unaccent, GIN index), xếp hạng `ts_rank`, fallback mờ Trigram. |
| 8 | `GET` | `/api/v1/recipes/{slug}` | Public / Owner/Admin | `FR-RCP-002` | Chi tiết bài viết công thức theo slug (kèm kiểm tra `RecipeSlugHistory`), trả về ETag `xmin`. |
| 9 | `GET` | `/health` | Public | `FR-OBS-001` | Tổng hợp kiểm tra kết nối các phụ thuộc: PostgreSQL, Redis, MinIO (trả 200 hoặc 503). |
| 10 | `GET` | `/health/live` | Public | `FR-OBS-001` | Liveness Probe: Xác nhận tiến trình backend .NET còn sống để Docker/K8s không restart container. |

---

### 👤 3. THÀNH VIÊN 3: TẠ NHẬT NGUYÊN (11 API)
*Phân hệ: Vòng đời Công thức, Quản trị Thùng rác & Sẵn sàng Nhận tải (`FR-RCP-003..007`, `FR-OBS-001`)*

| STT | Method | Endpoint Path | Phân quyền | Mã FR | Mô tả nghiệp vụ theo SRS |
|:---:|:---:|---|---|:---:|---|
| 1 | `POST` | `/api/v1/recipes` | Author / Admin | `FR-RCP-003` | Tạo mới công thức nấu ăn, sinh slug, gán `AuthorId`, trạng thái mặc định `Draft`. |
| 2 | `PUT` | `/api/v1/recipes/{id}` | Owner / Admin + If-Match | `FR-RCP-004` | Cập nhật thông tin công thức với Optimistic Concurrency qua `If-Match` và `xmin`. |
| 3 | `PATCH` | `/api/v1/recipes/{id}/publish` | Verified Owner + If-Match | `FR-RCP-005` | Xuất bản công thức sang `Published` (kiểm tra email confirmed, đủ nguyên liệu và bước làm). |
| 4 | `PATCH` | `/api/v1/recipes/{id}/unpublish` | Owner / Admin + If-Match | `FR-RCP-005` | Hủy xuất bản công thức, chuyển trạng thái quay về `Draft`. |
| 5 | `PATCH` | `/api/v1/recipes/{id}/archive` | Owner / Admin + If-Match | `FR-RCP-006` | Lưu trữ công thức sang `Archived`, ẩn khỏi public listing. |
| 6 | `PATCH` | `/api/v1/recipes/{id}/unarchive` | Owner / Admin + If-Match | `FR-RCP-006` | Mở lưu trữ công thức, đưa về trạng thái `Draft` để rà soát. |
| 7 | `DELETE` | `/api/v1/recipes/{id}` | Owner / Admin + If-Match | `FR-RCP-007` | Xóa mềm công thức (bỏ vào thùng rác), cho phép khôi phục trong 30 ngày. |
| 8 | `GET` | `/api/v1/admin/recipes/trash` | Admin | `FR-RCP-007` | Xem danh sách công thức trong thùng rác qua EF Core `IgnoreQueryFilters()`. |
| 9 | `POST` | `/api/v1/admin/recipes/{id}/restore` | Admin | `FR-RCP-007` | Khôi phục công thức từ thùng rác về trạng thái `Draft` trong vòng 30 ngày. |
| 10 | `DELETE` | `/api/v1/admin/recipes/{id}/purge` | Admin | `FR-RCP-007` | Xóa vĩnh viễn (Hard delete) công thức và các quan hệ phụ thuộc khỏi database. |
| 11 | `GET` | `/health/ready` | Public | `FR-OBS-001` | Readiness Probe: Kiểm tra kết nối DB và Redis sẵn sàng nhận traffic phục vụ người dùng. |

---

### 👤 4. THÀNH VIÊN 4: NGUYỄN PHÚ QUÝ (10 API)
*Phân hệ: Nội dung Chi tiết, Media Storage & Quản lý File (`FR-RCP-008..010`, `FR-FILE-001/002`)*

| STT | Method | Endpoint Path | Phân quyền | Mã FR | Mô tả nghiệp vụ theo SRS |
|:---:|:---:|---|---|:---:|---|
| 1 | `POST` | `/api/v1/recipes/{id}/images` | Owner / Admin | `FR-RCP-008`<br>`FR-FILE-001` | Upload ảnh lên MinIO Storage (tối đa 5MB, format JPEG/PNG/WebP/AVIF), lưu URL vào DB. |
| 2 | `PATCH` | `/api/v1/recipes/{id}/images/{imageId}` | Owner / Admin | `FR-RCP-008` | Cập nhật metadata ảnh, đặt làm ảnh đại diện chính `IsPrimary`, thứ tự hiển thị. |
| 3 | `DELETE` | `/api/v1/recipes/{id}/images/{imageId}` | Owner / Admin | `FR-RCP-008`<br>`FR-FILE-002` | Xóa hình ảnh khỏi công thức và MinIO Storage. |
| 4 | `POST` | `/api/v1/recipes/{id}/ingredients` | Owner / Admin | `FR-RCP-009` | Thêm nguyên liệu mới vào công thức với định lượng số thực (Quantity, Unit). |
| 5 | `PUT` | `/api/v1/recipes/{id}/ingredients/{ingredientId}` | Owner / Admin | `FR-RCP-009` | Cập nhật thông tin nguyên liệu (tên, số lượng, đơn vị, ghi chú). |
| 6 | `DELETE` | `/api/v1/recipes/{id}/ingredients/{ingredientId}` | Owner / Admin | `FR-RCP-009` | Xóa nguyên liệu khỏi công thức. |
| 7 | `POST` | `/api/v1/recipes/{id}/steps` | Owner / Admin | `FR-RCP-010` | Thêm bước làm mới: tiêu đề, mô tả, số thứ tự, thời gian hẹn giờ (`TimerMinutes`). |
| 8 | `PUT` | `/api/v1/recipes/{id}/steps/{stepId}` | Owner / Admin | `FR-RCP-010` | Cập nhật nội dung chi tiết bước thực hiện. |
| 9 | `PUT` | `/api/v1/recipes/{id}/steps/reorder` | Owner / Admin | `FR-RCP-010` | Sắp xếp lại thứ tự tất cả các bước làm trong 1 transaction nguyên tử. |
| 10 | `DELETE` | `/api/v1/recipes/{id}/steps/{stepId}` | Owner / Admin | `FR-RCP-010` | Xóa bước làm khỏi công thức và điều chỉnh lại thứ tự. |

---

## 💻 PHẦN II: PHÂN CÔNG GIAO DIỆN FRONTEND NEXT.JS 15 CHUẨN SRS (MỤC 5.1)

### 👤 THÀNH VIÊN 1: LIÊNG HÓT HA LUYẾN — Xác thực & Quản lý Tài khoản Cá nhân
*Căn cứ SRS: Mục 3.1 (`FR-AUTH`), Mục 5.1 (Bảng Màn hình UI), Phụ lục B*

1. **Màn hình `/auth/login` (CSR - Không yêu cầu Auth, tự redirect nếu đã login):**
   - Biểu mẫu đăng nhập Email/Password với validation phía client.
   - Nút đăng nhập nhanh bằng tài khoản Google (Google OAuth 2.0).
   - Tự động lưu trữ session an toàn vào HTTP-only cookie qua Auth.js v5.
2. **Màn hình `/auth/register` (CSR - Public):**
   - Biểu mẫu đăng ký: Email, Password, DisplayName kèm thông báo gửi mail kích hoạt.
3. **Màn hình `/profile` (CSR - Yêu cầu Auth):**
   - Trang cá nhân: Xem thông tin tài khoản hiện tại (`GET /api/v1/auth/me`).
   - Cập nhật thông tin hồ sơ: Thay đổi DisplayName, Bio, ảnh đại diện Avatar (`PATCH /api/v1/auth/me`).
4. **Hạ tầng xác thực & Điều hướng chung:**
   - Cấu hình Auth.js v5 (`lib/auth.ts`) và `middleware.ts` tự động bảo vệ các tuyến đường `/dashboard/*` và `/profile`.
   - Xây dựng component `UserMenu.tsx` trên thanh Header (hiển thị Avatar, Tên, nút vào Dashboard và nút Đăng xuất).

---

### 👤 THÀNH VIÊN 2: TRẦN QUỐC QUÂN — Khám phá, Tìm kiếm & Đọc Công thức Chi tiết
*Căn cứ SRS: Mục 3.2 (`FR-CAT`), 3.3 (`FR-RCP-001/002`), 3.4 (`FR-SRCH`), 4.7 (`NFR-SEO`), 5.1*

1. **Màn hình `/` Trang chủ (ISR `revalidate=3600`):**
   - Hiển thị danh mục món ăn nổi bật và các công thức nấu ăn mới nhất.
2. **Màn hình `/categories` & `/categories/[slug]` (ISR `revalidate=3600 / 600`):**
   - `/categories`: Lưới danh sách toàn bộ danh mục món ăn kèm số lượng recipe (`RecipeCount`).
   - `/categories/[slug]`: Trang chi tiết danh mục và danh sách các bài viết thuộc danh mục đó.
3. **Màn hình `/recipes` Danh sách công thức (SSR dynamic):**
   - Tích hợp thanh bộ lọc đa tiêu chí: Lọc theo danh mục, độ khó (`Easy`/`Medium`/`Hard`), thời gian chuẩn bị/nấu, lượng Calo.
   - Menu dropdown sắp xếp (Mới nhất, thời gian nấu nhanh nhất, calo).
   - Thanh phân trang dữ liệu chuẩn `{ page, pageSize, total, totalPages }`.
4. **Màn hình `/search` Kết quả Tìm kiếm FTS (SSR / CSR):**
   - Thanh tìm kiếm từ khóa tiếng Việt (có dấu/không dấu), hiển thị kết quả xếp hạng theo độ khớp liên quan (`ts_rank`).
5. **Màn hình `/recipes/[slug]` Chi tiết Công thức (ISR `revalidate=300`):**
   - Render thông tin chi tiết: Ảnh chính Hero image, tổng quan thời gian nấu, khẩu phần, thông tin dinh dưỡng (`RecipeNutrition`).
   - Tối ưu SEO: Nhúng siêu dữ liệu JSON-LD Schema.org (`Recipe` markup), Open Graph tags (`generateMetadata()`).
   - Skeleton loading stream UI mượt mà (`loading.tsx`).
6. **Màn hình `/dashboard/categories` Quản trị Danh mục (CSR - Admin):**
   - Giao diện thêm, sửa, xóa mềm danh mục dành riêng cho quản trị viên Admin.

---

### 👤 THÀNH VIÊN 3: TẠ NHẬT NGUYÊN — Quản trị Tác giả, Vòng đời & Xử lý Đồng thời
*Căn cứ SRS: Mục 3.3 (`FR-RCP-003..007`), Mục 5.1, Phụ lục D*

1. **Màn hình `/dashboard` Trang tổng quan (CSR - Bắt buộc Author/Admin):**
   - Thống kê tổng số lượng bài viết của tác giả theo từng trạng thái: `Draft`, `Published`, `Archived`.
2. **Màn hình `/dashboard/recipes` Quản lý Bài viết của tôi (CSR - Author/Admin):**
   - Danh sách công thức do chính user tạo (`mine=true`).
   - Các nút kích hoạt luồng Vòng đời:
     - **Xuất bản (`Publish`):** Kiểm tra điều kiện (email confirmed, có ít nhất 1 nguyên liệu và 1 bước nấu, category active).
     - **Hủy xuất bản (`Unpublish`):** Chuyển về `Draft`.
     - **Lưu trữ (`Archive`):** Ẩn khỏi public listing.
     - **Mở lưu trữ (`Unarchive`):** Đưa bài viết về `Draft`.
     - **Xóa mềm (`Delete`):** Đưa bài viết vào Thùng rác.
3. **Màn hình Quản trị Thùng rác Admin (`/admin/recipes/trash`):**
   - Danh sách các công thức đã bị xóa mềm (`IsDeleted == true`).
   - Nút **Khôi phục (`Restore`)** bài viết trong thời hạn 30 ngày.
   - Nút **Xóa vĩnh viễn (`Purge`)** xóa hoàn toàn khỏi DB.
4. **Kiểm soát Concurrency trên Giao diện:**
   - Tự động đính kèm header `If-Match: "{xmin}"` khi gửi request sửa hoặc đổi trạng thái recipe.
   - Bắt mã lỗi `409 Conflict` (`RECIPE_CONCURRENCY_CONFLICT`) và hiển thị cảnh báo người dùng khi dữ liệu bị sửa đổi bởi phiên khác.

---

### 👤 THÀNH VIÊN 4: NGUYỄN PHÚ QUÝ — Multi-step Wizard, Media Storage & Upload MinIO
*Căn cứ SRS: Mục 3.3 (`FR-RCP-008..010`), Mục 3.5 (`FR-FILE`), Mục 5.1*

1. **Màn hình `/dashboard/recipes/new` Form Tạo Công thức Multi-step Wizard (CSR):**
   - Theo đặc tả SRS dòng 1796, biểu mẫu tạo công thức mới được thiết kế dạng Multi-step Wizard:
     - **Bước 1 (Thông tin chung):** Tiêu đề, mô tả, chọn danh mục, độ khó, thời gian chuẩn bị/nấu, số khẩu phần, dữ liệu dinh dưỡng (Calories, Protein, Fat, Carbs).
     - **Bước 2 (Danh sách Nguyên liệu):** Quản lý mảng động `useFieldArray`: thêm, sửa, xóa nguyên liệu, nhập số lượng định lượng (decimal) và đơn vị đo lường.
     - **Bước 3 (Các bước thực hiện):** Quản lý các bước làm: tiêu đề, mô tả chi tiết, thời gian hẹn giờ (`TimerMinutes`), sắp xếp lại thứ tự các bước.
     - **Bước 4 (Upload Ảnh MinIO):** Tải lên album ảnh qua API `multipart/form-data`, chọn ảnh đại diện chính `IsPrimary`.
2. **Màn hình `/dashboard/recipes/[id]/edit` Form Chỉnh sửa Toàn diện (CSR):**
   - Tải dữ liệu hiện tại của recipe và cho phép tác giả cập nhật thông tin chung, danh sách nguyên liệu, các bước làm và quản lý bộ ảnh.
3. **Tối ưu hóa Media & Upload Storage:**
   - Kiểm tra định dạng file (JPEG, PNG, WebP, AVIF) và dung lượng tối đa 5MB trước khi upload lên MinIO.
   - Cấu hình tối ưu hiển thị Next.js `<Image>`: Responsive sizes, chống giật layout (CLS), và blur placeholder.

---

## 🤝 PHẦN III: CÔNG VIỆC CHUNG CỦA CẢ NHÓM (BUỔI 4)

1. **Khởi tạo và Thiết lập Dự án Frontend (`culinary-blog-web`):**
   - Setup dự án Next.js 15 App Router với TypeScript và Tailwind CSS.
   - Cài đặt các thư viện lõi phục vụ đồ án: `@tanstack/react-query`, `next-auth@beta`, `react-hook-form`, `@hookform/resolvers`, `zod`, `zustand`, `axios`.
2. **Cấu hình Root Layout & Providers (`app/layout.tsx` & `app/providers.tsx`):**
   - Tích hợp `QueryClientProvider` cho quản lý Server State và `SessionProvider` cho phiên đăng nhập Auth.js.
   - Cấu hình font Google Inter hỗ trợ Tiếng Việt, thiết lập thẻ Title chuẩn `%s | Culinary Blog`.
3. **Thiết lập Axios Client tập trung (`lib/api/axios.ts`):**
   - Cấu hình `baseURL` kết nối tới Backend .NET 10 (`http://localhost:5000/api/v1`).
   - Request Interceptor: Tự động trích xuất JWT Access Token từ Auth.js session và gắn vào header `Authorization: Bearer <token>`.
   - Response Interceptor: Bắt lỗi toàn cục `401 Unauthorized` để điều hướng về `/auth/login`.
4. **Quản lý Global UI State với Zustand (`store/useUIStore.ts`):**
   - Quản lý trạng thái mở/đóng Sidebar điều hướng, Modal xác nhận.
   - Quản lý chế độ xem danh sách công thức `recipesViewMode` (`'grid'` hoặc `'list'`), lưu bền vững vào `localStorage` qua middleware `persist`.
5. **Kiểm thử tích hợp Full-Stack (E2E Integration):**
   - Khởi động đồng thời cả Backend (.NET 10 API trên cổng 5000) và Frontend (Next.js trên cổng 3000).
   - Kiểm tra thông suốt luồng người dùng: Đăng ký ➔ Đăng nhập ➔ Multi-step Wizard tạo bài kèm ảnh ➔ Danh sách công thức ➔ Tìm kiếm FTS ➔ Đọc chi tiết bài viết.

---

## 🎯 PHẦN IV: KẾT QUẢ CẦN ĐẠT SAU KHI KẾT THÚC BUỔI 4 (LAB 4)

- [x] **Backend .NET 10:**
  - Toàn bộ **41 REST API Endpoints** đều hoàn thiện 100%, biên dịch 0 warning, 0 error.
  - Vượt qua toàn bộ Unit Tests và Integration Tests.
  - Ba cổng giám sát sức khỏe `GET /health`, `GET /health/live` và `GET /health/ready` phản hồi chính xác.
- [x] **Frontend Next.js 15:**
  - Hoàn thành đầy đủ **14 Màn hình / Routes** theo chuẩn Mục 5.1 trong [SRS.md](./SRS.md).
  - Áp dụng chính xác các Rendering Strategies: **SSG**, **SSG + ISR** (`/recipes`), **SSR** (`/recipes/[slug]`, `/recipes`), **CSR** (`/dashboard/*`, `/auth/*`, `/profile`).
  - Hệ thống xác thực Auth.js v5 hoạt động ổn định với Email/Password và Google Login; bảo vệ route chuẩn xác qua Middleware.
  - Multi-step Wizard tạo công thức hoạt động mượt mà với React Hook Form + Zod và mảng động `useFieldArray`.
  - Tải ảnh lên MinIO thành công và render ảnh tối ưu qua Next.js `<Image>`.
  - Toàn bộ nhóm sẵn sàng cho buổi nghiệm thu và báo cáo đồ án.
