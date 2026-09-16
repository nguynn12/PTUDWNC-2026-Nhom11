# Culinary Blog

Monorepo dùng chung cho dự án Blog Ẩm thực. PostgreSQL chạy trong Docker; API và frontend chạy trực tiếp trên máy để hỗ trợ hot reload.

## Yêu cầu

- Docker Desktop với Docker Compose
- .NET SDK 10.0.100 trở lên trong dòng 10.0.x
- Node.js 22 LTS và npm 10+

## Khởi động nhanh

1. Sao chép `.env.example` thành `.env` nếu cần đổi cấu hình PostgreSQL. Nếu không tạo `.env`, Docker Compose dùng giá trị development mặc định.
2. Khởi động database:

   ```bash
   docker compose up -d
   docker compose ps
   ```

3. Chạy API:

   ```bash
   cd backend
   dotnet restore
   dotnet run --project src/CulinaryBlog.API
   ```

4. Chạy frontend trong terminal khác:

   ```bash
   cd frontend
   npm install
   npm run dev
   ```

5. Kiểm tra:

   - Frontend: http://localhost:3000
   - API health: http://localhost:5000/health
   - Database health: http://localhost:5000/health/database
   - pgAdmin: http://localhost:5050

## pgAdmin 4

pgAdmin chạy cùng PostgreSQL trong Docker và đã được nạp sẵn kết nối `Culinary Blog PostgreSQL`.

- Đăng nhập pgAdmin: `admin@culinaryblog.com` / `culinary_blog_admin`.
- Khi pgAdmin hỏi mật khẩu database: nhập `culinary_blog_dev`.
- Host trong pgAdmin là `postgres`, không phải `localhost`, vì hai container giao tiếp qua mạng Docker Compose.

Có thể thay các giá trị trên trong file `.env` trước lần khởi động đầu tiên.

## Lệnh thường dùng

```bash
docker compose up -d             # Khởi động PostgreSQL
docker compose logs -f postgres # Xem log database
docker compose stop             # Dừng nhưng giữ dữ liệu
docker compose down             # Dừng và xóa container/network, giữ volume
dotnet test backend/CulinaryBlog.slnx
npm --prefix frontend run typecheck
```

Không dùng `docker compose down -v` trừ khi chủ động muốn xóa toàn bộ dữ liệu development.

## Quy ước làm việc

- Mỗi tính năng được phát triển theo lát cắt dọc: UI, API, application, domain/data và tests.
- Branch đề xuất: `feature/<module>-<short-name>`, `fix/<short-name>`.
- Không commit `.env`, secret, `bin`, `obj`, `.next` hoặc `node_modules`.
- Migration phải có tên mô tả và được review để tránh xung đột schema.
- API lỗi theo RFC 7807; thời gian lưu trong database dùng UTC.

Xem thêm [backend/README.md](backend/README.md), [frontend/README.md](frontend/README.md) và [docs/architecture/README.md](docs/architecture/README.md).
