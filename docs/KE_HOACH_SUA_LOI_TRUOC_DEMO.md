# Kế hoạch sửa lỗi trước demo — Culinary Blog (Nhóm 11)

**Căn cứ:** báo cáo rà soát của Codex trên `develop` (HEAD `6f881b8`) và phần kiểm chứng lại bằng cách đọc code.
**Ký hiệu cột "Xác minh":** ✔ = đã đối chiếu trực tiếp trong code `develop`; ○ = lấy từ báo cáo Codex, **người sửa tự xác nhận khi sửa**. Mình chưa chạy ứng dụng, nên chưa tái hiện được lỗi nào bằng HTTP.

## 1. Kết luận ngắn

- Luồng **tạo → sửa → xuất bản** đang hỏng ở nhiều chỗ cùng lúc: sai phương thức HTTP, sai hình dạng response, sai kiểu enum, sai tên trường token. Sửa một chỗ chưa đủ, phải sửa cả chuỗi.
- Có **một lỗ hổng bảo mật thật** (SEC-01) sửa được trong vài phút.
- **Giao diện đang giả lập thành công bằng dữ liệu mẫu** khi API lỗi (DATA-04), nên demo có thể *trông như chạy được* trong khi không lưu gì. Đây là lý do cả nhóm không thấy lỗi từ trước.
- Test xanh (305 unit + 34 integration) **không chứng minh luồng chạy đúng**, vì integration test dùng EF InMemory và xác thực giả (QA-01): không kiểm được `xmin`, ràng buộc unique của PostgreSQL, JWT thật hay MinIO thật.

## 2. Các mục đã xác minh trong code

| Mã | Mức | Bằng chứng mình thấy |
|---|---|---|
| SEC-01 | P0 | `RecipeIngredientEndpoints.cs:110`, `RecipeStepEndpoints.cs:114`, `RecipeImageEndpoints.cs:127` coi `X-User-Role: Admin` là Admin; `X-User-Id` còn được dùng thay claim |
| SEC-03 | P1 | `GeneralRateLimiting.cs:33` đọc `X-Forwarded-For` trực tiếp |
| API-01 | P1 | `GetRecipeBySlugQueryHandler`: điều kiện `Status == Published` → chủ bài/Admin không xem được Draft/Archived |
| API-02 | P1 | `ParseIfMatchHeader` trả `null` khi thiếu hoặc sai định dạng → bỏ qua so khớp phiên bản |
| API-04 | P1 | `CreateRecipeCommand` và endpoint không có trường `Nutrition` |
| FE-01 | P1 | `getMyRecipes` không đặt `mine=true` |
| FE-03 | P1 | Wizard gửi `difficulty: 'Medium'` (chữ), backend không cấu hình `JsonStringEnumConverter` |
| FE-04/05 | P1 | `fetchApi` trả nguyên `{ data: ... }` (không bóc `data`), nên `created.id` là `undefined` |
| FE-06 | P1 | Frontend gọi `PUT` cho publish/archive/unarchive; backend chỉ nhận `PATCH`/`POST` → 405 |
| FE-07 | P1 | Restore gọi `PUT`; backend chỉ có `POST /admin/recipes/{id}/restore` |
| FE-08 | P1 | Trang thùng rác đọc `res.items`, API trả `{ data, meta }` |
| DATA-04 | P1 | `api-client.ts` bắt mọi lỗi rồi trả mock: danh sách, thùng rác, chi tiết, **tạo, sửa, thêm nguyên liệu** (dòng 103, 144, 221, 236, 256, 274) |
| EXTRA-01 | P2 | (Mình thêm) `GetRecipeBySlugQueryHandler` Include 3 collection mà không có `AsSplitQuery()` |

## 3. Những điều phải chốt trước khi sửa (họp 15 phút)

Nhiều lỗi sinh ra vì hai phía hiểu hợp đồng khác nhau. Chốt một lần, ghi vào `docs/decisions/` và `SRS.md`, rồi mới sửa. Mình đề xuất mặc định:

| # | Vấn đề | Đề xuất | Lý do |
|---|---|---|---|
| D1 | Hình dạng response | Mọi response có body dùng `{ data, meta }` (kể cả Auth/profile, API-19); **bóc `data` một chỗ duy nhất** trong `fetchApi`/`apiRequest` | Sửa FE-04/05/08 cùng lúc, bỏ các đoạn thích nghi rải rác |
| D2 | Enum độ khó/trạng thái | **Số** theo backend: `Easy=1, Medium=2, Hard=3, Expert=4`; trạng thái `Draft=0, Published=1, Archived=2`. FE có **một** module ánh xạ (`lib/enums.ts`) | Không đụng cấu hình JSON của backend, rủi ro thấp (FE-03, FE-18) |
| D3 | Token phiên bản | Trường tên **`concurrencyToken`** (chuỗi) có trong mọi DTO tóm tắt/chi tiết/mutation, đồng thời trả header `ETag`; FE đọc `concurrencyToken` | Hiện FE chờ `xmin` mà API không trả (FE-09) |
| D4 | `If-Match` | **Bắt buộc** cho `PUT /recipes/{id}` và các thao tác vòng đời: thiếu header → **428**, sai định dạng → **400** (API-02, API-03) | Hết khe hở "null thì bỏ qua" |
| D5 | Restore | Khôi phục về **Draft**; sửa mô tả FR-RCP-007 trong SRS cho khớp (DOC-07) | An toàn, khớp UI và quyết định Unarchive → Draft |
| D6 | Giới hạn validation | Tiêu đề 3–200, mô tả 10–2000 (bỏ giới hạn 500), tên Category 2–100; cập nhật FR-CAT-003 (DOC-03, DOC-04, API-15, FE-19) | Một nguồn sự thật |
| D7 | `RecipeStep` | Giữ `title` (bắt buộc), `description`, `durationMinutes`, `parentStepId`; sửa SRS theo code (DOC-05, FE-17) | Đã chốt từ quyết định E5 |
| D8 | Publish/Unpublish lặp lại | Trả **200** nếu đã ở trạng thái đích (API-16) | Đúng idempotent |
| D9 | Dữ liệu thật | Chốt số lượng công thức, nguồn ảnh, nguồn số liệu dinh dưỡng (xem mục 6) | DATA-01…03 |
| D10 | Mock fallback | **Bỏ hẳn** hoặc chỉ bật khi `NEXT_PUBLIC_USE_MOCK=true` (mặc định tắt) | Không để giao diện giả lập thành công |

## 4. Thứ tự thực hiện

**Đợt 1 — làm ngay, theo thứ tự** (không làm gì khác cho tới khi xong, vì mọi việc sau đều phụ thuộc):
1. SEC-01 (xóa `X-User-Role`, `X-User-Id` ở 3 file).
2. D10: tắt mock fallback, để lỗi API hiện ra thật.
3. D1 + D2 + D3: sửa `fetchApi`, `lib/enums.ts`, DTO có `concurrencyToken`.
4. FE-06, FE-07: sửa phương thức HTTP.
5. API-01, API-04, API-02/16: quyền xem Draft cho chủ bài/Admin, Nutrition, `If-Match`.
6. Chạy kịch bản demo ở mục 7 trên **PostgreSQL thật**; chỉ khi chạy hết mới sang đợt 2.

**Đợt 2 — hoàn thiện luồng chính:** API-05/06/08, FE-01/02/10/11/12, FE-13…17, FE-22, MEDIA-01, EXTRA-01.
**Đợt 3 — dữ liệu thật** (DATA-01…03) và hạ tầng chạy máy sạch (OPS-01/02/03).
**Đợt 4 — tài liệu, kiểm thử, phần còn lại** (DOC-*, QA-*, OPS-04…11, API-07…21, MEDIA-02…05).

Báo cáo Chương 6 (demo) **chỉ chụp ảnh sau khi hết đợt 1**, tránh chụp dữ liệu giả.

## 5. Phân công 4 người

Theo module của từng người (đầu mối theo báo cáo Codex), có cân đối khối lượng.

### Tạ Nhật Nguyên — luồng vòng đời + nền hợp đồng (làm đầu tiên)

| # | Việc | Mã | Xác minh | Cỡ |
|---|---|---|---|---|
| 1 | Bóc `data` ở `fetchApi`; module `lib/enums.ts`; `concurrencyToken` trong DTO và `ETag`; tắt mock fallback (D1, D2, D3, D10) | FE-04/05/08/09/18, DATA-04 | ✔ | Vừa |
| 2 | Sửa phương thức: publish/unpublish/archive/unarchive → `PATCH`, restore → `POST` | FE-06, FE-07 | ✔ | Nhỏ |
| 3 | `GET` "công thức của tôi" gửi `mine=true`; trang sửa dùng đúng định danh (id hay slug) | FE-01, FE-02 | ✔ / ○ | Vừa |
| 4 | Nhận và lưu `Nutrition` trong Create/Update Recipe | API-04, FE-15 | ✔ | Vừa |
| 5 | `If-Match` bắt buộc (428/400); Publish/Unpublish idempotent | API-02, API-16 | ✔ | Vừa |
| 6 | Nút Unpublish/Unarchive trên danh sách; phân trang đầy đủ; thống kê Dashboard theo tổng thật | FE-10/11/12 | ○ | Vừa |
| 7 | Sửa `SystemAuditLoggingMiddleware` (ghi đúng mã HTTP cuối) và `ResourceId` của audit | OPS-05, OPS-06 | ○ | Nhỏ |
| 8 | Dọn `RecipePurgeService`: xóa đủ ảnh thu nhỏ/medium | MEDIA-02 | ○ | Nhỏ |
| 9 | **Sửa lại báo cáo Lab 8 cho đúng thực tế** (DOC-10, DOC-11) | DOC-10/11 | ○ | Nhỏ |
| 10 | Chủ trì họp chốt mục 3, cập nhật `SRS.md`/`docs/decisions` | DOC-01…09 | ○ | Vừa |

### Nguyễn Phú Quý — API con (nguyên liệu, bước, ảnh) + hiển thị chi tiết

| # | Việc | Mã | Xác minh | Cỡ |
|---|---|---|---|---|
| 1 | Xóa fallback `X-User-Role`, `X-User-Id` | SEC-01 | ✔ | Nhỏ |
| 2 | GET nguyên liệu/bước/ảnh kiểm tra quyền xem Draft/Archived | SEC-02 | ○ | Nhỏ |
| 3 | API con đọc và kiểm tra `If-Match` của Recipe cha | API-03 | ○ | Vừa |
| 4 | Sửa reorder bước (cập nhật hai giai đoạn hoặc dịch số tạm) để không đụng unique index; map `BadRequestException` → 400; trả lỗi từng trường | API-05/06/20 | ○ | Vừa |
| 5 | Hủy cache Recipe/Search khi sửa nguyên liệu/bước/ảnh | API-08 | ○ | Nhỏ |
| 6 | Server cấp `StepNumber`, bỏ tin số client; unique index một ảnh primary/Recipe | API-17, API-18 | ○ | Vừa |
| 7 | Khớp tên trường trang chi tiết: `originalUrl/mediumUrl/thumbnailUrl`, `prepTimeMinutes/cookTimeMinutes`, `protein/carbohydrates/fat/fiber`, `category` lồng, `durationMinutes` | FE-13/14/15/16/17 | ○ | Vừa |
| 8 | Trang chi tiết phân biệt lỗi mạng/500 với 404; chuyển hướng slug cũ ở phía trang | FE-20, FE-21 | ○ | Vừa |
| 9 | Upload nhiều ảnh: ghi nhận phần đã thành công, không sinh ảnh trùng khi tải lại | FE-22 | ○ | Vừa |
| 10 | MinIO lỗi trả 503 (bỏ tự lưu local); xóa ảnh có retry bền vững; resize bằng job nền | MEDIA-03/04/05 | ○ | Lớn |
| 11 | Thêm `AsSplitQuery()` cho truy vấn chi tiết | EXTRA-01 | ✔ | Nhỏ |

### Trần Quốc Quân — Discovery, cache, sitemap

| # | Việc | Mã | Xác minh | Cỡ |
|---|---|---|---|---|
| 1 | Chi tiết cho chủ bài/Admin xem Draft/Archived (kèm kiểm tra quyền) | API-01 | ✔ | Vừa |
| 2 | Không ghi cache công khai khi `mine=true` hoặc kết quả riêng; hủy cache Category khi đổi trạng thái Recipe; làm mới cache Recipe khi đổi tên danh mục | API-07/09/10 | ○ | Vừa |
| 3 | Sắp xếp: bổ sung `prepTime`, `servings`; xử lý hoặc bỏ `newest/quickest/createdAt` cho khớp | API-11 | ○ | Nhỏ |
| 4 | Bộ lọc enum ngoài phạm vi → 422 | API-12 | ○ | Nhỏ |
| 5 | Chi tiết Category trả công thức phân trang; bộ lọc `authorId` cho Admin | API-13/14 | ○ | Vừa |
| 6 | Validation Recipe theo D6 (tiêu đề 3–200, mô tả 10–2000) cả BE và FE | API-15, FE-19 | ○ | Nhỏ |
| 7 | Unique tên Category theo tên chuẩn hóa | API-21 | ○ | Nhỏ |
| 8 | Giới hạn tốc độ không tin `X-Forwarded-For` trực tiếp (dùng `UseForwardedHeaders` với proxy tin cậy); sửa header rate limit | SEC-03, OPS-10 | ✔ / ○ | Vừa |
| 9 | Gom một nguồn sitemap; phân trang đủ để không bỏ sót bài | OPS-09 | ○ | Vừa |

### Liêng Hót Ha Luyến — Auth, hạ tầng, vận hành

| # | Việc | Mã | Xác minh | Cỡ |
|---|---|---|---|---|
| 1 | Avatar `IFormFile`: cấu hình antiforgery hoặc `DisableAntiforgery` cho endpoint | MEDIA-01 | ○ | Nhỏ |
| 2 | Thống nhất response Auth/profile sang `{ data }` theo D1 và cập nhật FE | API-19 | ○ | Vừa |
| 3 | Docker từ máy sạch: tự migrate/seed ở môi trường chạy compose; Redis/Seq trỏ tên dịch vụ trong compose thay vì `localhost` | OPS-01, OPS-02 | ○ | Vừa |
| 4 | Health check đọc đúng cấu hình đang dùng (không hard-code `localhost`) | OPS-03 | ○ | Nhỏ |
| 5 | CSP cho Scalar/Hangfire; ghi rõ ngoại lệ loopback ở Development | OPS-04, SEC-04 | ○ | Nhỏ |
| 6 | Welcome/xác nhận email chạy qua hàng đợi có retry; gửi lại email trả phản hồi chung | OPS-08 | ○ | Vừa |
| 7 | Ảnh: kiểm tra giải mã cho cả ảnh nhỏ và AVIF | SEC-05 | ○ | Nhỏ |
| 8 | Metadata/canonical theo địa chỉ triển khai; đồng bộ Node 22 trong Dockerfile | OPS-11, DOC-13 | ○ | Nhỏ |
| 9 | CI: thu coverage thật, chạy Playwright; ghi rõ NuGet Audit đang tắt | QA-03/04/06 | ○ | Vừa |
| 10 | OpenTelemetry (nếu còn thời gian; nếu không thì ghi "chưa làm" trong báo cáo) | OPS-07 | ○ | Lớn |

## 6. Dữ liệu thật (DATA-01…03) — chia chung cả nhóm

Dữ liệu sinh ngẫu nhiên đạt yêu cầu Lab 2 nhưng không dùng được cho demo và nộp cuối. Việc này cần con người, không giao hết cho AI vì AI dễ viết công thức sai.

| Bước | Nội dung |
|---|---|
| 1 | Chốt số lượng (D9). Chất lượng quan trọng hơn số lượng: 40 công thức thật tốt hơn 100 công thức ghép mẫu. Hỏi lại giảng viên nếu yêu cầu số lượng tối thiểu khi nộp |
| 2 | Mỗi người nhập tay hoặc soạn 1 tệp JSON gồm: tiêu đề, mô tả, thời gian, khẩu phần, độ khó, nguyên liệu (số lượng + đơn vị), các bước thật của món đó, **số liệu dinh dưỡng lấy từ nguồn tra cứu có thật** (ví dụ bảng thành phần thực phẩm Việt Nam) hoặc **để trống** thay vì số ngẫu nhiên |
| 3 | **Ảnh:** dùng ảnh tự chụp, hoặc ảnh có giấy phép cho phép dùng (ghi nguồn). Không dùng ảnh ngẫu nhiên không liên quan món |
| 4 | Một người viết bộ nạp (importer) đọc các tệp JSON, tải ảnh lên MinIO, tạo dữ liệu qua đúng các Command (không ghi thẳng DB) để kiểm cả luồng |
| 5 | Mỗi người rà chéo công thức của người khác (tên món, nguyên liệu, bước có khớp không) |

## 7. Kịch bản kiểm thử luồng demo (chạy trên PostgreSQL + MinIO thật, mock đã tắt)

Chạy xong đợt 1 thì chạy kịch bản này; mỗi bước phải ra đúng kết quả mong đợi.

| # | Thao tác | Kết quả mong đợi |
|---|---|---|
| 1 | Đăng nhập Author A | Có token; vào được Dashboard |
| 2 | Gửi `X-User-Role: Admin` từ Author A vào API nguyên liệu của bài **Author B** | **403** (không còn vượt quyền) |
| 3 | Tạo công thức (kèm dinh dưỡng) | Trạng thái Draft; dữ liệu dinh dưỡng **được lưu** (tải lại vẫn còn); giao diện hiện mã bài thật |
| 4 | Thêm 3 nguyên liệu, 3 bước, upload 2 ảnh | Hiện ngay trong danh sách; ảnh nằm trên MinIO |
| 5 | Đổi chỗ bước 2 và 3 | 200, không 500 |
| 6 | Sửa tiêu đề kèm `If-Match` đúng | 200, token mới |
| 7 | Mở 2 tab cùng bài, lưu ở tab 1 rồi tab 2 | Tab 2 nhận **409**, hiện hộp thoại xung đột |
| 8 | Gọi sửa **không** có `If-Match` | **428** |
| 9 | Mở "Công thức của tôi" | Có cả Draft/Archived của mình, không hiện bài người khác |
| 10 | Bấm "Sửa" ở bài Draft | Tải được dữ liệu bài |
| 11 | Xuất bản | 200 (`PATCH`); bài hiện ở trang chủ, danh mục và tìm kiếm |
| 12 | Xuất bản lần 2 | 200, không lỗi |
| 13 | Lưu trữ → bỏ lưu trữ → xóa | Mỗi bước đúng trạng thái; xóa vào thùng rác |
| 14 | Admin mở thùng rác | Thấy bài vừa xóa; khôi phục (`POST`) được; xóa vĩnh viễn được và file ảnh biến mất khỏi MinIO |
| 15 | Tắt backend rồi mở các trang | Hiện lỗi thật, **không** hiện dữ liệu mẫu |

## 8. Prompt giao cho AI coder (dán kèm phần việc của từng người)

```
Bạn đang sửa lỗi trong repo PTUDWNC-2026-Nhom11 (nhánh develop). Chỉ làm đúng danh sách việc tôi đưa
bên dưới. KHÔNG thêm tính năng, KHÔNG đổi tên ngoài danh sách, KHÔNG sửa file không liên quan.
Nếu điều kiện không rõ, DỪNG và hỏi, đừng đoán.

Hợp đồng đã chốt (docs/decisions/): response có body luôn là { data, meta }; enum dạng số
(Difficulty Easy=1..Expert=4, Status Draft=0/Published=1/Archived=2); token phiên bản tên
concurrencyToken và có header ETag; If-Match bắt buộc (thiếu -> 428, sai định dạng -> 400);
Restore -> Draft; Publish/Unpublish lặp lại -> 200.

Với MỖI mã lỗi trong danh sách của tôi:
1. Mở đúng file/dòng được nêu, XÁC NHẬN lỗi còn tồn tại (không đúng thì báo lại, đừng sửa).
2. Sửa tối thiểu.
3. Viết hoặc cập nhật test chạy trên PostgreSQL thật (Testcontainers hoặc DB test), không chỉ InMemory.
4. Chạy dotnet build, dotnet test, và nếu đụng frontend: npm run typecheck, npm run lint, npm run build.

Khi xong, in ra: danh sách file đã sửa; với từng mã lỗi: "đã xác nhận / đã sửa / không tái hiện được";
kết quả build và test. Không báo "xong" nếu chưa chạy các lệnh trên.
```

## 9. Nguyên tắc từ giờ

- Mỗi PR chỉ sửa một nhóm mã lỗi, ghi mã lỗi trong tiêu đề, và **kèm kết quả chạy kịch bản mục 7 liên quan**.
- "Test xanh" không còn là bằng chứng hoàn thành. Chỉ ghi "hoàn thành" trong báo cáo khi đã chạy được trên giao diện thật.
- Trong báo cáo, các mục chưa sửa kịp phải nằm ở mục 7.4 và 7.6 với nguyên nhân, không để mục "100% hoàn thành" như báo cáo Lab 8 hiện tại.
