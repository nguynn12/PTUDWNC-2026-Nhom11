# ADR 0001: Chỉ container hóa PostgreSQL trong development

- Trạng thái: Accepted
- Ngày: 2026-09-16

## Bối cảnh

Nhóm cần môi trường database nhất quán nhưng vẫn muốn hot reload và debug frontend/backend thuận tiện.

## Quyết định

PostgreSQL 16 chạy bằng Docker Compose. Next.js và .NET API chạy trực tiếp trên máy phát triển. Dockerfile cho ứng dụng chưa thuộc phạm vi Sprint 0.

## Hệ quả

- Mọi thành viên dùng cùng phiên bản PostgreSQL và cùng cấu hình database mặc định.
- Thành viên phải cài .NET SDK và Node.js trên máy.
- Khi chuẩn bị triển khai, nhóm sẽ bổ sung quy trình đóng gói ứng dụng riêng.

