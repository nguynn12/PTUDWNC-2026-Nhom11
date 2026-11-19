# Backend

Backend dùng .NET 10 Minimal API và Clean Architecture.

## Dependency rule

```text
API -> Application
API -> Infrastructure
Infrastructure -> Application -> Domain
```

`Domain` không tham chiếu project khác. `Application` không tham chiếu `Infrastructure` hoặc `API`.

## Database

`CulinaryBlogDbContext` nằm trong Infrastructure. Khi bắt đầu thêm entity, tạo migration từ thư mục `backend`:

```bash
dotnet ef migrations add InitialCreate \
  --project src/CulinaryBlog.Infrastructure \
  --startup-project src/CulinaryBlog.API \
  --output-dir Migrations

dotnet ef database update \
  --project src/CulinaryBlog.Infrastructure \
  --startup-project src/CulinaryBlog.API
```

Các migration hiện có nằm tại `src/CulinaryBlog.Infrastructure/Migrations`. Chỉ thêm migration khi thay đổi model; dùng `database update` để áp dụng schema hiện tại.


## TV3 — vận hành và nghiệm thu (15/11/2026)

- SRS hiện tại quy định title 3–200 ký tự, description 10–2000; Publish cần ít nhất một nguyên liệu và một bước. Ảnh không phải điều kiện bắt buộc Publish.
- `/hangfire` yêu cầu JWT Bearer của Admin ở mọi môi trường. Anonymous bị 401; Author bị 403. Truy cập từ localhost không được bỏ qua xác thực. Khi nghiệm thu dashboard qua HTTP client phải gửi token Admin.
- `purge-expired-recipes-30days` dùng chung `IRecipePurgeService` với purge tay. Cron `0 3 * * *`, timezone Hangfire hiện tại là **UTC**. Retry 60/300/1800 giây; lỗi storage giữ metadata để thử lại, không báo hoàn thành giả.
- Job của nhóm có thể enqueue qua `IBackgroundJobClient`; ví dụ `jobs.Enqueue<PurgeExpiredRecipesJob>(job => job.ExecuteAsync(CancellationToken.None))`. Chỉ thử trên database/bucket riêng; job recurring đã được đăng ký trong Program.
- `/health` và `/health/ready` đọc Redis cùng thứ tự ưu tiên với ứng dụng: `ConnectionStrings:Redis`, rồi `Redis:ConnectionString`, rồi mặc định. MinIO dùng `Minio:Endpoint` nội bộ, không dùng `PublicUrl`. Redis/MinIO đang kiểm tra khả năng kết nối TCP; điều này không thay thế kiểm tra mật khẩu, quyền bucket hoặc thao tác đọc/ghi.

### Sao lưu và khôi phục thật

Chạy từ thư mục gốc. Mặc định khớp Docker Compose: database `culinary_blog_dev`, user `culinary_blog`, container `culinary-blog-postgres`. Có thể truyền tham số hoặc đặt `POSTGRES_DB`/`POSTGRES_USER`.

```powershell
.\backend\scripts\backup.ps1 -BackupDir 'C:\culinary-backups' -RetentionDays 30
```

Script dùng `pg_dump -F c`, kiểm tra archive bằng `pg_restore --list` và header `PGDMP`, rồi mới đổi tên `.partial` thành `.dump`. Docker dùng `docker cp`, không chuyển dữ liệu nhị phân qua pipeline văn bản. Lỗi ném exception/mã thoát khác 0, không tạo JSON giả và không dọn backup cũ. Mật khẩu `PGPASSWORD` được trả về giá trị trước khi chạy script.

Để tự động chạy 03:00 hằng ngày: tạo task trong Windows Task Scheduler, trigger Daily 03:00 theo múi giờ máy, program `powershell.exe`, arguments `-NoProfile -NonInteractive -File "<thu-muc-du-an>\backend\scripts\backup.ps1" -BackupDir "C:\culinary-backups"`. Tài khoản chạy task cần quyền ghi thư mục và kết nối PostgreSQL hoặc Docker đang chạy. Cài `pg_dump`/`pg_restore` nếu task không dùng Docker. Lượt nghiệm thu chỉ kiểm tra script; không tự đăng ký task trên máy người dùng.

Database đích của restore phải tồn tại và được chỉ định rõ. **Restore thay dữ liệu của database đích**; kiểm thử bằng một database riêng, tuyệt đối không dùng database demo:

```powershell
docker exec culinary-blog-postgres createdb -U culinary_blog culinary_restore_check
.\backend\scripts\restore.ps1 -BackupFilePath 'C:\culinary-backups\backup_<ten>.dump' -Database culinary_restore_check
```

Restore kiểm tra archive trước khi kết nối, chạy `--single-transaction --exit-on-error --clean --if-exists`, rollback khi lỗi và chỉ báo thành công khi exit code bằng 0. Việc xóa database kiểm thử thực hiện sau khi đã kiểm tra đúng tên đích.

### Kiểm thử có sẵn

```powershell
dotnet test backend/CulinaryBlog.slnx --no-restore -m:1 -nodeReuse:false
npm --prefix frontend run typecheck
npm --prefix frontend run build
```

E2E được bổ sung ngay trong `frontend/e2e/search.spec.ts`, dùng `@playwright/test` đã khai báo trong package.json. Khởi động frontend với `BACKEND_API_URL` trỏ về API/database kiểm thử; dùng Author đã xác nhận email ở môi trường đó:

```powershell
$env:PLAYWRIGHT_BASE_URL = 'http://127.0.0.1:3000'
$env:PLAYWRIGHT_API_URL = 'http://127.0.0.1:5059'
$env:PLAYWRIGHT_CHANNEL = 'msedge'
$env:E2E_AUTHOR_EMAIL = 'author-kiem-thu@example.com'
$env:E2E_AUTHOR_PASSWORD = '<mat-khau-tai-khoan-kiem-thu>'
npm --prefix frontend run test:e2e
```

Không đặt credentials của demo/production vào test. Nếu không truyền credentials, nhóm lifecycle sẽ skip và không được tính là đã nghiệm thu. Kiểm tra bàn phím trong bộ E2E là smoke test focus, không phải chứng nhận đầy đủ WCAG. Kết quả nghiệm thu TV3 được ghi trong `docs/Lab08_2312704_TaNhatNguyen.md`.

### Seq — đã sửa và nghiệm thu ngày 15/11/2026

Seq 2026.1 yêu cầu thông tin Admin khi khởi tạo; thiếu mật khẩu khiến container kết thúc với exit code 1 và bị Docker khởi động lại liên tục. Compose nay đọc `SEQ_ADMIN_PASSWORD_HASH` từ `.env` và dùng `SEQ_FIRSTRUN_ADMINPASSWORDHASH`, có health check `/health`; API Docker dùng `http://seq:5341` và chờ Seq healthy. Khởi tạo hash cho máy mới theo README gốc; không bỏ qua xác thực hoặc xóa volume.

Máy hiện tại: user Seq `admin`; mật khẩu đã sinh được lưu ở biến `SEQ_ADMIN_PASSWORD` trong **file `.env` có sẵn**, được Git bỏ qua. Không đưa `.env` vào commit. Hash chỉ dùng khi khởi tạo volume lần đầu; đăng nhập sau đó dùng mật khẩu, không dùng hash.

Đã kiểm chứng trên dịch vụ thật:

- Seq Running/Healthy, RestartCount=0 qua nhiều lần kiểm tra; đăng nhập Admin thành công.
- Đọc `/api/events` không đăng nhập trả 401; phiên Admin đọc được log.
- Request API `GET /api/v1/recipes/search?q=pho` trả 200; Seq tìm được 7 event với CorrelationId `9aae4843-cfdc-4345-beb0-921d847cd7f2`, gồm HTTP StatusCode=200 và MediatR SearchRecipesQuery.
- `CreateRecipeCommand` chạy trên database riêng trả 201; Seq có audit Success, UserId, ResourceId, Timestamp với CorrelationId `a19f8118-b9cf-4cf8-a529-9ea0f4247417`.
- Ingestion từ mạng Docker đến `http://seq:5341/ingest/clef` trả 201.
- Backend vẫn 389/389 test đạt (344 unit + 45 integration). Dữ liệu demo không đổi.

Tham khảo cấu hình chính thức: https://datalust.co/docs/docker-deployment-overview và https://datalust.co/docs/using-the-http-api .
