# ADR 0003: Sử dụng Redis làm giải pháp Distributed Caching

- Trạng thái: Accepted
- Ngày: 2026-11-02
- Nhóm đề xuất: Nhóm 11

## Bối cảnh

Để đáp ứng yêu cầu phi chức năng về hiệu năng (NFR-PERF) và tính sẵn sàng cao (NFR-REL), hệ thống cần một tầng đệm dữ liệu (Caching) nhằm giảm tải cho cơ sở dữ liệu PostgreSQL đối với các truy vấn đọc thường xuyên:
- Danh sách thể loại (Category list/detail): TTL 30 phút.
- Danh sách công thức công khai (Public recipes list): TTL 1 phút.
- Chi tiết công thức nấu ăn (Recipe details): TTL 5 phút.
- Kết quả tìm kiếm (Search results): TTL 1 phút.

Sử dụng `IMemoryCache` (In-memory) trong ứng dụng phân tán sẽ dẫn đến hiện tượng dữ liệu không đồng bộ giữa các instance API.

## Quyết định

1. **Công nghệ:** Sử dụng **Redis 7** (thông qua container `redis:7-alpine` và thư viện `Microsoft.Extensions.Caching.StackExchangeRedis`).
2. **Kiến trúc trừu tượng:** Triển khai thông qua interface `ICacheService` bao bọc `IDistributedCache`, không phụ thuộc trực tiếp vào triển khai cụ thể của Redis ở tầng Application.
3. **Chính sách Invalidation:** Hỗ trợ thu hồi bộ đệm theo tag/prefix (`recipes`, `recipe:{id}`, `recipe-slug:{slug}`, `categories`, `search`) khi có thay đổi dữ liệu (tạo/sửa/xóa công thức, nguyên liệu, bước làm, ảnh).
4. **Khả năng phục hồi (Resilience):** Khi Redis gặp sự cố, hệ thống ghi log cảnh báo và tự động truy vấn trực tiếp từ PostgreSQL thay vì ném ngoại lệ dừng ứng dụng (NFR-REL-002).

## Hệ quả

- Giảm 80-90% tải truy vấn đọc lên CSDL chính.
- Thời gian phản hồi API cho các truy vấn phổ biến đạt < 50ms.
- Yêu cầu duy trì dịch vụ Redis trong Docker Compose.
