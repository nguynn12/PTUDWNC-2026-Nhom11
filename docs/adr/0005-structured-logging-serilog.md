# ADR 0005: Ghi nhật ký có cấu trúc với Serilog và Seq

- Trạng thái: Accepted
- Ngày: 2026-11-02
- Nhóm đề xuất: Nhóm 11

## Bối cảnh

Hệ thống cần tuân thủ các yêu cầu giám sát và kiểm toán (FR-OBS-002, NFR-SEC):
- Truy vết yêu cầu xuyên suốt giữa client và server thông qua `X-Correlation-ID`.
- Giám sát thời gian thực thi của các Mediator Command/Query và cảnh báo khi request vượt quá 500ms.
- Nhật ký nghiệp vụ ghi nhận: `UserId`, `ResourceId`, `Action`, `Timestamp`, `Result`.
- Tuyệt đối không lưu vết các dữ liệu nhạy cảm (mật khẩu, JWT token, mã xác thực) trong log.

## Quyết định

1. **Công nghệ:** Sử dụng thư viện **Serilog** với các Sink: Console (JSON format), File (Rolling theo ngày), và Seq (tập trung hóa log trên môi trường Development/Staging).
2. **Correlation ID Middleware:** Tự động trích xuất hoặc sinh mã `X-Correlation-ID` cho mỗi HTTP request và đính kèm vào `LogContext`.
3. **MediatR Logging Behavior:** Triển khai `LoggingBehavior<TRequest, TResponse>` đo lường thời gian thực thi, log cảnh báo (Warning) nếu thực thi > 500ms.
4. **An toàn thông tin:** Masking hoặc loại trừ hoàn toàn các thuộc tính nhạy cảm trước khi ghi nhận vào sink log.

## Hệ quả

- Dễ dàng tra cứu nguyên nhân lỗi và hành vi hệ thống trên giao diện Seq.
- Đảm bảo tuân thủ tiêu chuẩn an toàn thông tin OWASP và SRS.
