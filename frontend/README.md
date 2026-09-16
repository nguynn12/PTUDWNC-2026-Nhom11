# Frontend

Frontend dùng Next.js App Router và TypeScript strict mode.

## Tổ chức code

- `src/app`: routes, layouts và route-level loading/error UI.
- `src/features`: module nghiệp vụ theo lát cắt dọc (`auth`, `categories`, `recipes`, `search`).
- `src/components`: UI dùng chung, không chứa business rules.
- `src/lib`: API client và utility dùng chung.

Khi thêm một module, ưu tiên đặt component, schema, service và type riêng trong `src/features/<module>` thay vì gom tất cả theo loại file ở cấp toàn dự án.

