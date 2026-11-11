# ADR 0002: Sử dụng MinIO S3-Compatible làm Object Storage cho hình ảnh và avatar

- Trạng thái: Accepted
- Ngày: 2026-11-02
- Người đề xuất: Nguyễn Phú Quý (2312731)

## Bối cảnh

Hệ thống CulinaryBlog yêu cầu lưu trữ và phục vụ hình ảnh chất lượng cao cho công thức nấu ăn và ảnh đại diện người dùng (FR-FILE-001, FR-FILE-002, FR-RCP-008). 
Ban đầu, hệ thống lưu tệp tin trực tiếp trên ổ đĩa local (`wwwroot/uploads`). Tuy nhiên, phương pháp lưu trữ cục bộ bộc lộ nhiều hạn chế:
1. Khó mở rộng theo quy mô phân tán (Horizontal Scaling) khi nhiều instance API chạy đồng thời.
2. Không đồng bộ với kiến trúc điện toán đám mây (AWS S3, Cloudflare R2, MinIO).
3. Thiếu cơ chế phân quyền truy cập và chính sách bucket public-read chuẩn hóa.

## Quyết định

1. **Công nghệ:** Sử dụng **MinIO** (bản phát hành tương thích S3 API của Amazon) thông qua thư viện chính thức `AWSSDK.S3` của AWS.
2. **Cấu trúc lưu trữ:**
   - Bucket mặc định: `culinary-blog` với chính sách tải công khai (`public-read`).
   - Đặt tên file ngẫu nhiên chống path traversal: `{folder}/{Guid.NewGuid()}{extension}`.
   - Thư mục lưu trữ: `recipes/{recipeId}/` cho ảnh món ăn, `avatars/{userId}/` cho ảnh đại diện.
3. **Xử lý ảnh đa kích thước (FR-JOB-002):**
   - Sử dụng thư viện `SixLabors.ImageSharp` để sinh tự động phiên bản Thumbnail (`300x300` px) và Medium (`800x600` px) định dạng WebP tối ưu.
   - Ghi nhận đầy đủ đường dẫn `OriginalUrl`, `ThumbnailUrl`, `MediumUrl` vào CSDL.
4. **Độ tin cậy & Idempotency:**
   - Hàm `DeleteAsync` đảm bảo tính chất **idempotent**: nếu tệp tin không tồn tại trên MinIO thì không ném exception (bắt mã lỗi `NoSuchKey` hoặc `404 Not Found`).
   - Cung cấp cơ chế fallback an toàn: khi MinIO chưa khởi chạy hoặc bị tắt trong môi trường kiểm thử, hệ thống tự động lưu vào thư mục cục bộ (`wwwroot/uploads`) mà không làm gián đoạn luồng nghiệp vụ.

## Hệ quả

- **Tích cực:**
  - Hoán đổi linh hoạt giữa MinIO (local/on-premise) và AWS S3 (production) mà không cần sửa đổi tầng Application (tuân thủ Clean Architecture).
  - Tối ưu hóa băng thông và trải nghiệm người dùng nhờ các phiên bản ảnh WebP thu nhỏ chuẩn kích thước.
  - Ngăn ngừa hoàn toàn lỗ hổng Path Traversal và giả mạo tệp tin độc hại.
- **Lưu ý:**
  - Cần khởi chạy container `minio` và `minio-init` trong `docker-compose.yml` khi phát triển và triển khai hệ thống.
