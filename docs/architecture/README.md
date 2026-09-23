# Kiến trúc dự án

## Runtime development

```text
Next.js (localhost:3000)
        |
        | HTTP/JSON
        v
.NET API (localhost:5000)
        |
        | Npgsql
        v
PostgreSQL 16 (Docker, localhost:5432)
```

PostgreSQL và công cụ quản trị pgAdmin được container hóa. Frontend và API chạy trực tiếp để có hot reload nhanh và dễ debug. pgAdmin truy cập tại `http://localhost:5050` và kết nối PostgreSQL bằng hostname nội bộ `postgres`.

## Module ownership

1. Identity & Profile.
2. Taxonomy & Discovery.
3. Recipe Lifecycle.
4. Recipe Composition & Media.

Mỗi module chịu trách nhiệm xuyên suốt từ giao diện đến test. Thành phần dùng chung cần pull request và ít nhất một reviewer ngoài module.

## Quyết định nền tảng

- Modular monolith, không dùng microservices trong phiên bản môn học.
- PostgreSQL 16 là nguồn dữ liệu chính.
- Entity Framework Core theo Code First.
- API version qua prefix `/api/v1`.
- Cấu hình local có giá trị development mặc định; secret thật không được commit.
- Migration được tạo sau khi chốt entity nền tảng, tránh migration rỗng hoặc schema giả định quá sớm.
- Mâu thuẫn phát hiện trong SRS và quyết định xử lý được ghi lại tại [`docs/decisions/`](../decisions/README.md) — đọc trước khi implement bất kỳ module nào.
