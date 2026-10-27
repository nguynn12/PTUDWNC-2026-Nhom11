# Decisions — SRS Conflict Resolution Log

Thư mục này chứa toàn bộ quyết định kiến trúc phát sinh từ việc rà soát mâu thuẫn trong `SRS.md` (v1.2.0), trước khi nhóm bắt đầu code nghiệp vụ (sau Sprint 0 skeleton). Đây là **nguồn quyết định bổ sung**, đọc **cùng với** `SRS.md` và `docs/architecture/README.md`, không thay thế 2 tài liệu đó.

## Thứ tự ưu tiên khi có xung đột thông tin

1. **`RESOLVED-CONFLICTS.md`** (file này, thư mục hiện tại) — nếu 1 quy tắc trong `SRS.md` gốc bị đánh dấu mâu thuẫn và đã có quyết định ở đây, **dùng quyết định ở đây**, không dùng câu chữ gốc trong `SRS.md`.
2. **`SRS.md`** (thư mục gốc / `docs/srs/`) — dùng cho mọi phần không bị liệt kê trong `RESOLVED-CONFLICTS.md`.
3. **`OPEN-QUESTIONS.md`** — danh sách các điểm **chưa có quyết định**. Nếu một tác vụ code chạm vào 1 trong các mục này, **dừng lại, không tự suy đoán/tự chọn phương án** — hỏi lại người phụ trách (xem cột "Người chốt" trong file, hoặc hỏi bất kỳ thành viên nào nếu cột đó còn trống) trước khi viết code liên quan.

## Danh sách file

| File | Nội dung | Số mục |
|---|---|---|
| `RESOLVED-CONFLICTS.md` | 27 mâu thuẫn giữa các phần của SRS đã được phân tích và chốt quyết định kiến trúc (ADR), cộng D7 chốt từ OPEN-QUESTIONS #1 | 28 |
| `OPEN-QUESTIONS.md` | Các điểm SRS v1.2.0 vẫn còn mơ hồ/tự mâu thuẫn, cần người quyết trước khi code phần liên quan | 3 còn mở (mục 1 đã chốt) |

## Quy tắc cho AI coding agent đọc thư mục này

- Không implement bất kỳ phần nào khớp với 1 mục trong `OPEN-QUESTIONS.md` mà không có xác nhận của người dùng trong phiên làm việc hiện tại, kể cả khi có vẻ "hiển nhiên nên chọn phương án nào".
- Khi 1 file trong `RESOLVED-CONFLICTS.md` có mục "Tác động kỹ thuật" ghi rõ migration SQL / tên field / route API — coi đó là spec bắt buộc tuân theo, không tự đổi tên khác dù thấy hợp lý hơn.
- Nếu phát hiện thêm 1 mâu thuẫn mới trong SRS chưa được liệt kê ở đây, dừng lại và báo cho người dùng thay vì tự chọn cách xử lý.
