-- ============================================================================
-- Đánh dấu migration EF Core "InitialCreate" (20260922103000_InitialCreate) là ĐÃ
-- ÁP DỤNG, KHÔNG chạy lại DDL — vì schema tương ứng ĐÃ được tạo trước đó bằng
-- 001_init_identity_schema.sql (chạy thủ công lúc dotnet-ef chưa cài được).
--
-- Migration InitialCreate (backend/src/CulinaryBlog.Infrastructure/Migrations/)
-- được viết tay để khớp 1:1 với schema đã chạy ở 001. Script này chỉ insert 1
-- dòng vào bảng lịch sử migration nội bộ của EF Core (__EFMigrationsHistory) để
-- từ nay `dotnet ef database update` / `dotnet ef migrations list` nhận diện
-- đúng migration đã áp dụng, không cố tạo lại các bảng đã tồn tại (sẽ lỗi
-- "relation already exists" nếu bỏ qua bước này rồi chạy `database update`).
--
-- CÁCH CHẠY: giống 001 — mở trong pgAdmin Query Tool trên server "Culinary Blog
-- PostgreSQL", database culinary_blog_dev, rồi Execute (F5). Chạy SAU 001.
-- ============================================================================

BEGIN;

CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId"    varchar(150) NOT NULL,
    "ProductVersion" varchar(32)  NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260922103000_InitialCreate', '10.0.3')
ON CONFLICT ("MigrationId") DO NOTHING;

COMMIT;
