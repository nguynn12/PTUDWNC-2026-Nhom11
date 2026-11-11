# ADR 0004: Sử dụng Hangfire cho tác vụ chạy ngầm và định kỳ

- Trạng thái: Accepted
- Ngày: 2026-11-02
- Nhóm đề xuất: Nhóm 11

## Bối cảnh

Hệ thống CulinaryBlog có nhiều tác vụ bất đồng bộ và định kỳ cần xử lý độc lập khỏi chu kỳ request-response của người dùng:
- FR-JOB-001: Gửi email chào mừng kích hoạt tài khoản.
- FR-JOB-002: Xử lý thu nhỏ và tối ưu hóa hình ảnh.
- FR-JOB-003: Định kỳ sinh `sitemap.xml` phục vụ SEO lúc 02:00 UTC.
- FR-JOB-004: Xóa vĩnh viễn (Purge) các công thức đã xóa mềm quá 30 ngày lúc 03:00 UTC.

Tác vụ cần có cơ chế lưu trữ bền vững (persistent storage), tự động retry với exponential backoff khi thất bại, và giao diện giám sát trực quan.

## Quyết định

1. **Công nghệ:** Sử dụng **Hangfire** kết hợp với lưu trữ trên PostgreSQL (`Hangfire.PostgreSql`).
2. **Quản lý thất bại (Retry Strategy):** Cấu hình `AutomaticRetry` 3 lần với khoảng thời gian dãn cách: 1 phút, 5 phút, 30 phút.
3. **Bảo mật:** Bật Hangfire Dashboard tại `/hangfire` với bộ lọc ủy quyền (Authorization Filter) chỉ cho phép tài khoản có vai trò `Admin`.
4. **Idempotency:** Mọi job nền được thiết kế để đảm bảo tính bất biến khi chạy lại nhiều lần mà không gây sai lệch dữ liệu.

## Hệ quả

- Đảm bảo độ tin cậy của các luồng nghiệp vụ dài và định kỳ.
- Không làm nghẽn luồng xử lý HTTP request chính của người dùng.
- Có giao diện Web trực quan để quản trị viên theo dõi trạng thái jobs và kích hoạt thủ công khi cần.
