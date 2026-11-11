# Changelog

Tất cả những thay đổi quan trọng của dự án **CulinaryBlog** sẽ được ghi chép trong tệp tin này.

Định dạng dựa trên [Keep a Changelog](https://keepachangelog.com/vi/1.0.0/),
và dự án tuân thủ [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0] - 2026-11-02

### Added
- **Hệ thống Upload ảnh & Object Storage (FR-FILE-001, FR-FILE-002, FR-RCP-008):**
  - Tích hợp dịch vụ lưu trữ đối tượng MinIO tương thích chuẩn S3 thông qua `AWSSDK.S3`.
  - Khởi tạo bucket `culinary-blog` với chính sách công khai `public-read`.
  - Tự động sinh phiên bản ảnh tối ưu Thumbnail (`300x300`) và Medium (`800x600`) định dạng WebP bằng thư viện `SixLabors.ImageSharp`.
  - Cơ chế xóa ảnh đồng bộ 3 phiên bản đảm bảo tính chất idempotent (không ném lỗi khi khóa không tồn tại).
  - Xác thực an toàn tệp tin tải lên: kiểm tra dung lượng `<= 5MB` trước khi đọc stream, xác thực MIME type, Magic Bytes và giải mã ảnh cấu trúc.
- **Giao diện Quản lý và Tải ảnh lên trong Wizard (NFR-USE-004):**
  - Cho phép chọn tải lên nhiều ảnh đồng thời.
  - Thanh tiến trình phần trăm (%) thời gian thực theo từng tệp tin (`onUploadProgress`).
  - Gán ảnh đại diện món ăn (Primary image), đổi thứ tự ảnh trực quan (`OrderIndex`).
  - Cấu hình Next.js `remotePatterns` tối ưu tải ảnh qua `next/image`.
- **Hạ tầng Docker Compose Full-Stack (SRS 6.5, NFR-SCALE):**
  - Cấu hình 7 dịch vụ hoàn chỉnh: `postgres` (16-alpine), `redis` (7-alpine), `pgadmin` (4), `minio` + `minio-init` (`mc`), `seq`, `api` (multi-stage .NET 10), `web` (multi-stage Next.js 16), `nginx` (reverse proxy).
  - Viết `Dockerfile` và `.dockerignore` tối ưu đa tầng (multi-stage build), chạy dưới người dùng non-root (`app`, `nextjs`) an toàn.
  - Cấu hình Nginx reverse proxy định tuyến thông minh: `/` chuyển tiếp Next.js Web, `/api/` chuyển tiếp ASP.NET Core API.
- **Tự động hóa CI Pipeline (.github/workflows/ci.yml):**
  - Xây dựng quy trình tích hợp liên tục tự động kiểm thử Backend (.NET 10 restore, build, 322 test cases).
  - Tự động kiểm tra chất lượng Frontend (Node 22 lint, typecheck, build static pages).
- **Tài liệu Kiến trúc & Quyết định (ADR):**
  - Bổ sung các tài liệu ADR từ 0001 đến 0005: Runtime, MinIO Object Storage, Redis Caching, Hangfire Background Jobs, Serilog Structured Logging.
- **Trang Quản trị Danh sách Người dùng (FR-AUTH-010):**
  - Giao diện `/admin/users` hỗ trợ lọc theo vai trò Admin / Tác giả, tìm kiếm theo tên và email, phân trang.
  - Thao tác khóa / mở khóa tài khoản với hộp thoại xác nhận an toàn.
- **Vòng đời Công thức Nấu ăn hoàn thiện (FR-RCP-001...007):**
  - Đầy đủ luồng trạng thái Draft → Published → Unpublish → Archive → Delete → Restore → Purge.
  - Xử lý xung đột lạc quan (Optimistic Concurrency) với `ConcurrencyToken`.

### Changed
- Cải thiện hiệu năng tải ảnh bằng cách sử dụng phiên bản Thumbnail cho danh sách thẻ và Medium cho trang chi tiết công thức.
- Tối ưu hóa file cấu hình môi trường `.env.example` đầy đủ các thông số cổng kết nối và biến hệ thống.

### Fixed
- Khắc phục triệt để nguy cơ Path Traversal bằng cơ chế sinh tên tệp GUID ngẫu nhiên `{folder}/{Guid}{ext}`.
- Sửa lỗi xử lý cảnh báo bảo mật NuGet thành Build Error trong môi trường .NET 10 (`Directory.Build.props`).

---

## [0.7.0] - 2026-10-27 (Lab 7)
### Added
- Trang chi tiết công thức hoàn chỉnh (hiển thị nguyên liệu, các bước làm kèm ảnh, thông tin tác giả, dinh dưỡng).
- Trang chủ với bố cục 2 cột (sidebar danh mục, danh sách công thức mới nhất).
- Tính năng tìm kiếm công thức nâng cao (lọc theo danh mục, độ khó, thời gian nấu, sắp xếp, phân trang).
- Tính năng đổi mật khẩu, quên mật khẩu và đặt lại mật khẩu qua email SMTP.

---

## [0.6.0] - 2026-10-20 (Lab 6)
### Added
- Trang hồ sơ người dùng (Profile) và cập nhật thông tin cá nhân.
- Giao diện cơ bản cho người dùng và luồng xác thực trên Next.js.

---

## [0.5.0] - 2026-10-13 (Lab 5)
### Added
- Xác thực JWT (Access Token 15 phút, Refresh Token Rotation 7 ngày).
- Đăng ký tài khoản, gửi email xác nhận tài khoản.
- Đăng nhập tài khoản cục bộ và Google OAuth 2.0.
- Cơ chế phân quyền RBAC (Admin, Author, User).

---

## [0.4.0] - 2026-10-06 (Lab 4)
### Added
- Quản lý Category: tạo mới, cập nhật, xóa mềm, xem chi tiết và danh sách.
- Quản lý Recipe cơ bản (CRUD món ăn, nguyên liệu, bước làm).

---

## [0.1.0] - 2026-09-22 (Lab 1 & 2)
### Added
- Khởi tạo kiến trúc dự án theo chuẩn Clean Architecture (.NET Minimal API + Next.js App Router).
- Cấu hình Entity Framework Core, PostgreSQL, Docker Compose cơ bản.
