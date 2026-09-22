# Kiến Thức Nền Tảng — Tổng Hợp Tài Liệu Bài Giảng "Phát triển Ứng dụng Web Nâng cao"

**Nguồn tham khảo cho dự án Culinary Blog — Nhóm 11, CTK47B, Đại học Đà Lạt**

**Phiên bản:** 1.0.0
**Ngày tổng hợp:** 22/09/2026
**Người tổng hợp:** Luyen (thành viên nhóm)

---

## 0. Mục đích và phạm vi tài liệu

Tài liệu này tổng hợp toàn bộ kiến thức từ 14 tài liệu trong thư mục `Bài_Giảng`:

- 4 chương giáo trình chính thức: Chương 1 (Kiến trúc Web & RESTful API), Chương 2 (Xác thực, Phân quyền & Bảo mật API), Chương 3 (Thiết kế Dữ liệu & Tối ưu Truy vấn), Chương 4 (Kiến trúc Frontend với Next.js).
- 10 slide deck bổ sung (S01–S10) mở rộng và minh họa lại các chủ đề trên, trong đó **S10 (Caching)** là chủ đề duy nhất không có chương giáo trình riêng.

Mục tiêu: dùng làm **nguồn tra cứu nhanh** khi hiện thực hoá các Feature Requirement (FR) trong `SRS.md`, và làm cơ sở đối chiếu khi review code/PR trong repo `PTUDWNC-2026-Nhom11`. Tài liệu không thay thế SRS.md hay các ADR — khi có mâu thuẫn, **SRS.md và ADR trong repo luôn là nguồn quyết định** (xem Phụ lục D về các điểm dự án cố ý đi lệch giáo trình).

### Ánh xạ nhanh Chương ↔ Vấn đề dự án

| Chương / Slide | Chủ đề | Liên quan trực tiếp đến |
|---|---|---|
| Chương 1 | Clean Architecture, CQRS, Minimal API, REST, envelope | Toàn bộ cấu trúc backend, chuẩn response `{data, meta}` (Chương 5.2/8 SRS) |
| Chương 2 | JWT, Identity, RBAC, Rate Limiting, OAuth | Auth module, FR liên quan đăng nhập/đăng ký/phân quyền tác giả |
| Chương 3 | EF Core, Repository/UoW, N+1, Full-Text Search | Domain/Infrastructure layer, FR tìm kiếm công thức |
| Chương 4 | Next.js App Router, TanStack Query, Auth.js | Toàn bộ frontend `culinary-blog-web` |
| S10 | Caching (IMemoryCache, Redis, Output Cache) | FR-CAT-001 ("phục vụ từ cache khi có") |
| S01–S09 | Bổ sung/minh hoạ lại Chương 1–3 | Xem Phụ lục A |

---

## PHẦN I — KIẾN TRÚC WEB VÀ RESTFUL API (Chương 1)

### 1.1 Kiến trúc phân tầng và Clean Architecture

Clean Architecture tổ chức code thành 4 tầng đồng tâm, với **Dependency Rule**: mọi phụ thuộc chỉ được trỏ vào trong (tầng ngoài phụ thuộc tầng trong, không bao giờ ngược lại).

| Tầng | Vai trò | Ví dụ trong Culinary Blog |
|---|---|---|
| **Domain** | Entities, Value Objects, Domain Exceptions — không phụ thuộc gì cả | `Recipe`, `Category`, `RefreshToken` |
| **Application** | Use cases (CQRS Commands/Queries), interfaces (`IApplicationDbContext`, `IJwtService`) | `CreateRecipeCommand`, `IRecipeRepository` |
| **Infrastructure** | Hiện thực hoá interfaces: EF Core, JWT, Email, Cache | `ApplicationDbContext`, `JwtService`, `RecipeRepository` |
| **Presentation/API** | Minimal API Endpoints, Middleware, DI composition root | `RecipeEndpoints`, `Program.cs` |

Nguyên tắc cốt lõi: Domain và Application **không được biết** Infrastructure hay Presentation tồn tại. Infrastructure implement các interface do Application định nghĩa (Dependency Inversion Principle) — đây là lý do `IApplicationDbContext` được khai báo ở Application nhưng `ApplicationDbContext` (EF Core) implement ở Infrastructure.

### 1.2 CQRS với MediatR

CQRS (Command Query Responsibility Segregation) tách biệt thao tác **ghi** (Command) và **đọc** (Query) thành các object riêng biệt, mỗi object có một Handler xử lý duy nhất.

- **Command**: `CreateRecipeCommand`, `LoginCommand`, `RefreshTokenCommand` — trả về kết quả tối thiểu, có thể thay đổi state.
- **Query**: `GetRecipesQuery`, `GetRecipeDetailQuery` — chỉ đọc, không có side-effect.
- **Handler**: implement `IRequestHandler<TRequest, TResponse>`, chứa toàn bộ logic nghiệp vụ của một use case (Vertical Slice — mỗi feature là một "lát cắt dọc" gồm Command/Query + Handler + Validator nằm cùng thư mục, thay vì tách theo tầng kỹ thuật như Controller/Service/Repository truyền thống).
- **MediatR**: thư viện trung gian định tuyến `ISender.Send(request)` tới đúng Handler, hỗ trợ Pipeline Behaviors (validation, logging, transaction) chạy trước/sau Handler mà không cần sửa Handler.

### 1.3 .NET 10 Minimal APIs

Dự án dùng Minimal APIs thay vì Controllers (MVC) — không có class `XController`, endpoint được khai báo trực tiếp bằng `app.MapGet/MapPost/...`.

Các kỹ thuật chính:

- **Route Groups** (`app.MapGroup("/api/v1/recipes")`): nhóm các endpoint cùng resource, áp dụng chung tag, filter, policy.
- **TypedResults**: trả về kiểu cụ thể (`TypedResults.Ok<T>`, `TypedResults.NotFound`) thay vì `IResult` chung chung — hỗ trợ tốt hơn cho OpenAPI generation và unit test.
- **Endpoint Filters** (`IEndpointFilter`): tương đương middleware nhưng scope theo từng endpoint, dùng cho validation, logging riêng biệt.
- **OpenAPI 3.1 native** (`AddOpenApi()`, `MapOpenApi()`) thay thế Swashbuckle/Swagger — kết hợp với **Scalar UI** (`MapScalarApiReference()`) làm giao diện thử API thay cho Swagger UI.

### 1.4 Thiết kế RESTful API

- **Richardson Maturity Model**: 4 cấp độ trưởng thành của REST API, từ Level 0 (RPC qua HTTP, 1 endpoint duy nhất) đến Level 3 (HATEOAS — response chứa link điều hướng). Culinary Blog nhắm tới Level 2 (đúng resource URI + đúng HTTP method/status code), không bắt buộc HATEOAS.
- **Resource-oriented URI**: danh từ số nhiều, không có động từ (`GET /api/v1/recipes`, không phải `/getRecipes`).
- **HTTP Methods**: GET (Safe + Idempotent), PUT (Idempotent), DELETE (Idempotent), POST (không Idempotent — tạo mới mỗi lần gọi), PATCH (partial update).
- **Status codes chuẩn**: 200 OK, 201 Created (kèm header `Location`), 204 No Content (DELETE thành công), 400 Bad Request (validation), 401 Unauthorized (chưa xác thực), 403 Forbidden (đã xác thực nhưng không đủ quyền), 404 Not Found, 409 Conflict (trùng slug/email), 422 Unprocessable Entity, 429 Too Many Requests.
- **RFC 7807 Problem Details**: định dạng lỗi chuẩn hoá `{ type, title, status, detail, instance, errors }` — ASP.NET Core hỗ trợ native qua `AddProblemDetails()`.

### 1.5 Chuẩn Response Envelope `{data, meta}`

Đây là chuẩn được SRS.md công bố tại Chương 5.2/8 và áp dụng cho **toàn bộ API** của dự án:

```
{
  "data": <kết quả chính — object hoặc mảng>,
  "meta": <thông tin phụ trợ — phân trang, v.v. Bỏ qua nếu không cần>
}
```

> **Lưu ý quan trọng cho dự án**: `PagedResult<T>` (`items/totalCount/page/pageSize/totalPages/hasNextPage/hasPreviousPage`) — xuất hiện trong nhiều ví dụ code của Chương 3 và Chương 4 — chỉ là **DTO nội bộ của Application layer**. Presentation layer (Minimal API Endpoint) phải **map** `PagedResult<T>` sang envelope `{data, meta}` trước khi serialize, KHÔNG được trả thẳng `PagedResult<T>` ra HTTP response. Đây chính là lỗi đã được phát hiện và vá tại FR-CAT-001/FR-CAT-002 trong đợt chuẩn hoá SRS.md v1.3.0 (22/09/2026) — hai điểm này trước đó mô tả "Kết quả mong đợi" theo format cũ của `PagedResult<T>`, không khớp chuẩn envelope đã công bố.

---

## PHẦN II — XÁC THỰC, PHÂN QUYỀN VÀ BẢO MẬT API (Chương 2)

### 2.1 OWASP Top 10 — roadmap trong giáo trình

| OWASP 2021 | Rủi ro | Chương xử lý |
|---|---|---|
| A01 | Broken Access Control | Chương 2 — Resource-Based Authorization |
| A02 | Cryptographic Failures | Chương 2 — PBKDF2, JWT signing |
| A03 | Injection | Chương 3 — EF Core parameterized queries |
| A04 | Insecure Design | Chương 1 — Clean Architecture |
| A05 | Security Misconfiguration | Chương 2 — CORS/HTTPS; Chương 6 — Docker |
| A07 | Identification & Authentication Failures | Chương 2 (trọng tâm) |
| A10 | SSRF | Chương 4 — ghi chú Next.js fetch |

### 2.2 Băm mật khẩu: vì sao không dùng MD5/SHA

| Phương pháp | Tốc độ hash | An toàn |
|---|---|---|
| Plaintext | N/A | ❌ |
| MD5/SHA-1 | ~10 tỷ/giây (GPU) | ❌ — Rainbow Table, không salt |
| SHA-256 + Salt | ~1 tỷ/giây (GPU) | ⚠️ Yếu — vẫn brute-force khả thi |
| **PBKDF2 (600k vòng)** | ~1.000/giây (CPU) | ✅ — ASP.NET Core Identity dùng mặc định |
| BCrypt (cost=12) | ~250/giây | ✅ |
| Argon2id | ~100/giây | ✅✅ Tốt nhất — memory-hard, chống GPU |

ASP.NET Core Identity mặc định dùng **PBKDF2-HMAC-SHA512**, 600.000 vòng lặp (khuyến nghị NIST 2023), cấu hình qua `PasswordHasherOptions.IterationCount`. Nguyên tắc "adaptive cost factor": tăng gấp đôi iterations → tăng gấp đôi thời gian crack, nhưng chỉ chậm thêm vài mili-giây khi login thật.

### 2.3 JWT — cấu trúc và triển khai

JWT gồm 3 phần `Header.Payload.Signature`, Base64Url-encoded (KHÔNG mã hoá — ai cũng đọc được Payload, chỉ Signature được ký để chống giả mạo). **Không lưu dữ liệu nhạy cảm vào Payload.**

**Claims chuẩn (RFC 7519)**: `sub` (user id), `iss` (issuer), `aud` (audience), `exp`/`iat` (thời hạn/thời điểm tạo), `jti` (id duy nhất của token). Culinary Blog thêm custom claims `email`, `role`.

**HS256 vs RS256**:

| Tiêu chí | HS256 (Symmetric) | RS256 (Asymmetric) |
|---|---|---|
| Key | 1 secret key dùng chung | private (ký) + public (verify) |
| Phù hợp | Monolith, single service | Microservices, nhiều verifier |
| Culinary Blog | ✅ Dùng HS256 | — |

**Access Token**: ngắn hạn (15 phút, best practice). **Refresh Token**: dài hạn (7 ngày), random 64 bytes, lưu server-side trong bảng `RefreshTokens`.

### 2.4 Refresh Token Rotation

Cơ chế: mỗi lần dùng Refresh Token để lấy Access Token mới, server phát hành cặp token MỚI và đánh dấu token cũ `IsUsed = true`. Nếu kẻ tấn công đánh cắp Refresh Token và dùng trước user thật, khi user thật dùng lại token đã bị đánh dấu `IsUsed` → hệ thống phát hiện bất thường, **thu hồi (Revoke) toàn bộ Refresh Token của user** và buộc đăng nhập lại.

`RefreshToken.IsValid()` = `!IsRevoked && !IsUsed && ExpiresAt > UtcNow`.

Chống **"alg:none" attack**: khi validate token đã hết hạn để lấy claims (dùng cho refresh flow), phải explicitly kiểm tra `jwtToken.Header.Alg == HmacSha256` sau khi `ValidateToken`, vì `ValidateLifetime = false` mở ra khả năng kẻ tấn công đổi algorithm để bypass signature check.

### 2.5 Mô hình phân quyền (Authorization)

Authentication trả lời "Bạn là ai?" — Authorization trả lời "Bạn được phép làm gì?" — hai lớp **tách biệt**, Authorization luôn chạy SAU Authentication.

1. **RBAC (Role-Based)**: gán quyền theo Role trong JWT claims, không cần query DB mỗi request. Culinary Blog có 3 role: `Admin`, `Author`, `Reader`. Cấu hình qua `AddAuthorizationBuilder().AddPolicy("AdminOnly", p => p.RequireRole("Admin"))`.
2. **Policy-Based**: điều kiện phức tạp hơn Role đơn thuần (VD: `VerifiedAuthorRequirement` — phải có email đã xác nhận). Gồm `IAuthorizationRequirement` + `AuthorizationHandler<TRequirement>`.
3. **Resource-Based (Owner Authorization)**: kiểm tra quyền dựa trên cả identity user VÀ thuộc tính resource cụ thể — không làm được với Role/Policy đơn thuần vì cần biết recipe đang thao tác thuộc về ai. Culinary Blog: chỉ tác giả (`AuthorId == user.Id`) mới sửa/xoá được recipe của mình; Admin luôn có toàn quyền. Dùng `IAuthorizationService.AuthorizeAsync(user, resource, operation)` trong Endpoint, KHÔNG dùng attribute.

### 2.6 Rate Limiting và CORS

**Rate Limiting** (native từ .NET 7, `AddRateLimiter`):

| Thuật toán | Cơ chế | Phù hợp |
|---|---|---|
| Fixed Window | X request/cửa sổ cố định | Login endpoint (5 req/phút) |
| Sliding Window | X request/cửa sổ trượt | Công bằng hơn, chống burst |
| Token Bucket | Token tích luỹ theo thời gian | API chung (200 req/phút) |
| Concurrency | Giới hạn request đồng thời | Endpoint CPU-intensive |

Vượt hạn mức → trả **429 Too Many Requests** kèm header `Retry-After`.

**CORS**: chỉ áp dụng cho browser, KHÔNG bảo vệ khỏi Postman/curl/server-to-server — CORS không phải biện pháp bảo mật API (đó là việc của Authentication/Authorization). Cấu hình riêng cho Development (`localhost:3000`) và Production; `AllowCredentials()` bắt buộc nếu dùng httpOnly cookie. Thứ tự middleware quan trọng: `UseCors()` → `UseRateLimiter()` → `UseAuthentication()` → `UseAuthorization()`.

### 2.7 OAuth 2.0 + PKCE + Google Social Login

**Authorization Code Flow với PKCE** (khuyến nghị cho web/mobile apps, chống "authorization code interception"): User → Google Authorization Endpoint (kèm `code_challenge`) → Google xác thực → redirect về callback với `code` → **server-side** đổi `code` + `code_verifier` lấy ID Token/Access Token (client không thấy bước này) → server tạo/cập nhật `ApplicationUser`, phát hành JWT nội bộ.

Implement bằng `AddGoogle()` (ASP.NET Core External Authentication) + `UserManager.FindByLoginAsync/AddLoginAsync`. Case xử lý: email đã đăng ký thủ công trước đó đăng nhập Google cùng email → cần quyết định merge account hay báo lỗi (bài tập mở, chưa có câu trả lời chuẩn trong giáo trình).

### 2.8 ASP.NET Core Identity

`ApplicationUser : IdentityUser` mở rộng thêm `FullName, AvatarUrl, Bio`. Cấu hình password policy (tối thiểu 8 ký tự, có chữ hoa/số), Lockout (5 lần sai → khoá 15 phút), `RequireUniqueEmail`. Middleware pipeline đúng thứ tự: `UseHttpsRedirection → UseCors → UseRateLimiter → UseAuthentication → UseAuthorization → Map*Endpoints`.

---

## PHẦN III — THIẾT KẾ DỮ LIỆU VÀ TỐI ƯU TRUY VẤN (Chương 3)

### 3.1 ORM: Active Record vs Data Mapper

| Tiêu chí | Active Record (Rails) | Data Mapper (EF Core) |
|---|---|---|
| Tổ chức | Entity chứa cả data lẫn DB logic | Entity chỉ chứa data; DbContext chứa DB logic |
| Coupling | Entity phụ thuộc ORM | Entity độc lập (POCO) |
| Test | Khó mock | Dễ mock qua interface |
| Culinary Blog | Không dùng | ✅ EF Core |

EF Core là Data Mapper: Entity là POCO thuần, không có logic DB. **Change Tracking**: khi query entity, EF Core tự theo dõi thay đổi; `SaveChangesAsync()` so sánh với snapshot ban đầu để sinh SQL UPDATE tối thiểu — đây là lý do `AsNoTracking()` quan trọng cho query chỉ đọc.

### 3.2 Repository Pattern và Unit of Work — LƯU Ý QUAN TRỌNG CHO DỰ ÁN

Chương 3 dạy và minh hoạ đầy đủ **Repository Pattern + Unit of Work** (`IRepository<T>`, `IRecipeRepository`, `IUnitOfWork`) như một tầng abstraction giữa Application và EF Core, với lý do: testability (dễ mock hơn `DbContext`) và Dependency Inversion.

> ⚠️ **Khác biệt có chủ đích với dự án thực tế**: theo README.md và cấu trúc Application layer hiện tại của `PTUDWNC-2026-Nhom11`, quy ước của dự án là **CQRS Handler dùng trực tiếp `IApplicationDbContext`**, KHÔNG dùng Repository/Unit of Work. Đây không phải là thiếu sót — Chương 3 giới thiệu Repository/UoW như một pattern thay thế để sinh viên biết, nhưng nhóm đã chọn hướng "Handler → IApplicationDbContext trực tiếp" (đơn giản hơn, giảm một tầng abstraction không cần thiết khi đã có MediatR Handler làm ranh giới use case). Khi đọc code mẫu trong Chương 3 có `IUnitOfWork.Recipes.AddAsync(...)`, cần **quy đổi** sang `_context.Recipes.Add(...)` + `_context.SaveChangesAsync()` cho khớp coding convention thực tế của repo. Đừng thêm Repository/UoW vào dự án trừ khi có quyết định kiến trúc (ADR) mới thay đổi quy ước này.

### 3.3 Thiết kế Domain Entities và ERD

Cấu trúc chính: `Category (1) → (N) Recipe → (1:N) RecipeStep`, `(1:N) RecipeIngredient`, `(1:N) RecipeImage`. `Recipe` có Owned Entity `RecipeNutrition` (Calories/Protein/Carbs/Fat — lưu chung bảng `Recipes`, không JOIN, bất biến/immutable dạng `record`).

- **Owned Entity Type** (`OwnsOne`): dùng cho Value Object không có định danh riêng (VD `RecipeNutrition`), lưu chung bảng entity chủ, tối ưu hơn bảng riêng vì không cần JOIN.
- **Value Conversion** (`HasConversion<string>()`): lưu enum (`DifficultyLevel`, `RecipeStatus`) dưới dạng string thay vì int — dễ đọc trong DB, an toàn khi migration (thêm/xoá giá trị enum không làm lệch số).
- **Concurrency Token**: `[Timestamp] byte[] RowVersion` + `IsRowVersion().IsConcurrencyToken()` — EF Core tự thêm `WHERE RowVersion = @current` vào UPDATE, ném `DbUpdateConcurrencyException` nếu bị ai đó sửa trước (optimistic concurrency; PostgreSQL có thể dùng `xmin` hệ thống thay thế).
- **Delete Behavior**: `Cascade` (xoá Recipe → xoá Steps/Ingredients), `Restrict` (không xoá Category nếu còn Recipe), `SetNull` (xoá User → giữ Recipe, AuthorId = null).

### 3.4 Fluent API và IEntityTypeConfiguration

Mỗi entity có 1 file cấu hình riêng implement `IEntityTypeConfiguration<T>`, nạp tự động qua `ApplyConfigurationsFromAssembly()` trong `OnModelCreating`. Tách biệt khỏi Domain Entity — Domain không có annotation ORM (tuân thủ Clean Architecture).

### 3.5 Tối ưu truy vấn

**N+1 Query Problem** — vấn đề phổ biến nhất với ORM: query N entities rồi truy cập navigation property trong vòng lặp → 1 query ban đầu + N query bổ sung = N+1 tổng cộng. Khắc phục bằng **Eager Loading** (`.Include(r => r.Category).Include(r => r.Author)`) → chỉ 1 query JOIN duy nhất.

**Cartesian Explosion**: Include nhiều collection navigation (1-N) cùng lúc → JOIN tạo Cartesian product (VD 10 Steps × 15 Ingredients = 150 rows cho 25 bản ghi thực). Giải quyết bằng **`AsSplitQuery()`** — chạy nhiều SQL riêng biệt thay vì 1 JOIN khổng lồ.

**AsNoTracking() + Projection**: `AsNoTracking()` tắt Change Tracking cho query chỉ đọc (tiết kiệm ~30% bộ nhớ). `ProjectToType<TDto>()` (Mapster) chỉ SELECT đúng cột cần trong DTO, không SELECT toàn bộ entity.

Quy tắc kinh nghiệm: bật SQL logging trong Development (`Microsoft.EntityFrameworkCore.Database.Command: Information`); một request API bình thường không nên vượt quá 5–10 SQL queries — nếu vượt, cần review.

### 3.6 Chỉ mục (Index) và Full-Text Search

| Index Type | Cấu trúc | Phù hợp |
|---|---|---|
| **B-tree** (mặc định) | Balanced tree | `=, <, >, BETWEEN, LIKE xxx%` — Slug, CategoryId, CreatedAt |
| Hash | Hash table | Chỉ `=`, ít dùng trong PostgreSQL |
| **GIN** | Inverted index | Array, JSONB, Full-Text Search — **bắt buộc cho SearchVector** |
| GiST | Generalized tree | Dữ liệu hình học, PostGIS |
| BRIN | Block range | Cột tăng dần, bảng cực lớn |

`LIKE '%từ khoá%'` không dùng được B-tree (wildcard đầu) → sequential scan toàn bảng, chậm và không hiểu ngữ nghĩa. **PostgreSQL Full-Text Search**: `to_tsvector('simple', unaccent(text))` tạo tsvector (danh sách lexeme + vị trí); `plainto_tsquery()`/`websearch_to_tsquery()` tạo tsquery từ input user; toán tử `@@` để match; `ts_rank()` để sắp xếp theo độ liên quan. Với GIN index, tốc độ ~5ms so với ~500ms của LIKE trên 100k rows.

**Tiếng Việt**: PostgreSQL không có stemmer riêng cho tiếng Việt → dùng config `'simple'` (không stemming sai) + extension `unaccent` (chuẩn hoá dấu, "phở" → "pho" để tìm không dấu vẫn ra kết quả có dấu). `SearchVector` nên là **computed column** (`HasComputedColumnSql(..., stored: true)`) để PostgreSQL tự cập nhật khi Title/Description đổi.

### 3.7 Database Seeding với Bogus

`HasData()` của EF Core dùng cho dữ liệu tĩnh, cố định (Categories, Roles) — GUID cố định để idempotent. Thư viện **Bogus** sinh dữ liệu phong phú, thực tế (tên, email, lorem tiếng Việt) cho khối lượng lớn (VD 50 recipes với Steps/Ingredients ngẫu nhiên nhưng hợp lý) — dùng `UseSeed(SEED)` để đảm bảo kết quả **reproducible** giữa các lần chạy (quan trọng cho môi trường test).

---

## PHẦN IV — CACHING (S10 — không có chương giáo trình riêng)

> Đây là chủ đề duy nhất trong 14 tài liệu không có "Chương" chính thức, chỉ có slide bổ sung S10. Vì FR-CAT-001 của SRS.md có nhắc "kết quả được serve từ cache khi có", phần này được tổng hợp đầy đủ.

### 4.1 Vì sao cần caching — đánh đổi cốt lõi

Caching là lớp lưu trữ trung gian tốc độ cao, tránh lặp lại thao tác tốn kém (query DB, gọi API bên thứ ba). Ba lợi ích: giảm latency, giảm tải backend/DB, tăng throughput. Đánh đổi bản chất: **Performance vs. Consistency** — không có cấu hình đúng cho mọi trường hợp; câu hỏi cần trả lời trước khi cache: *"Dữ liệu này chấp nhận độ trễ cập nhật bao lâu?"*

Thuật ngữ nền tảng: **Cache hit/miss**, **Hit ratio** (hits/(hits+misses) — chỉ số quan trọng nhất), **TTL**, **Eviction**, **Stale data**.

### 4.2 Các tầng cache trong hệ thống web

```
Browser Cache → CDN/Edge Cache → Reverse Proxy (Nginx/Varnish) → Application (ASP.NET Core) → Distributed Cache (Redis) → Database
```

Nguyên tắc: càng gần user càng nhanh nhưng càng khó nhất quán; càng gần DB càng dễ nhất quán nhưng lợi ích tốc độ càng thấp. Request dừng lại ở tầng đầu tiên gặp cache hit.

Culinary Blog tập trung vào 2 tầng: **Application-level (IMemoryCache)** và **Distributed cache (Redis)**.

### 4.3 IMemoryCache và Output Cache

**IMemoryCache**: lưu trong RAM của tiến trình, cực nhanh, không chi phí mạng — nhưng chỉ tồn tại 1 instance, mất khi restart, không phù hợp khi scale nhiều server. Dùng cho dữ liệu tĩnh/ít đổi hoặc hệ thống 1 instance. API: `cache.GetOrCreateAsync(key, async entry => {...})`.

**Output Cache** (native .NET): lưu toàn bộ HTTP response của 1 endpoint, hỗ trợ **invalidate theo Tag** chủ động (`.CacheOutput("RecipeList")`, `EvictByTagAsync("recipes")`), không cần chờ hết TTL. Phù hợp cho GET endpoint công khai, đọc nhiều.

| Tiêu chí | IMemoryCache | Output Cache |
|---|---|---|
| Lưu | Object tuỳ ý | Toàn bộ HTTP response |
| Vị trí | Trong code nghiệp vụ | Middleware |
| Invalidation | Thủ công theo key | TTL hoặc Tag |

### 4.4 Redis Distributed Cache và 5 Caching Pattern

Khi scale nhiều instance, IMemoryCache của mỗi instance độc lập → dữ liệu không đồng bộ. Redis giải quyết bằng cache dùng chung toàn cụm (`AddStackExchangeRedisCache`).

| Pattern | Cơ chế | Khi nào dùng |
|---|---|---|
| **Cache-aside** (Lazy Loading) | App tự đọc cache → miss thì query DB → ghi ngược vào cache | **Mặc định** cho ứng dụng đọc nhiều (khuyến nghị bắt đầu) |
| Read-through | Cache/provider tự lấy dữ liệu từ DB khi miss | Hệ thống lớn, nhiều team dùng chung cache layer |
| Write-through | Ghi đồng thời cache + DB trước khi trả kết quả | Cần chính xác ngay sau khi ghi |
| Write-behind | Ghi cache trước, trả ngay, đồng bộ DB bất đồng bộ sau | Ghi log, đếm view — chấp nhận mất mát nhỏ |
| Refresh-ahead | Cache tự làm mới TRƯỚC khi hết TTL | Dữ liệu hot, truy cập rất thường xuyên |

Code mẫu Cache-aside: đọc `cache.GetStringAsync(key)` → nếu null, query DB → `cache.SetStringAsync(key, json, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) })`.

### 4.5 Cache Invalidation — "bài toán khó nhất"

*"Chỉ có 2 việc khó trong Khoa học Máy tính: cache invalidation và đặt tên biến." — Phil Karlton*

| Chiến lược | Ưu điểm | Nhược điểm | Khi dùng |
|---|---|---|---|
| **TTL** | Cực đơn giản | Có "cửa sổ" dữ liệu cũ | Dữ liệu chấp nhận độ trễ |
| **Tag-based** | Chủ động, chính xác theo nhóm | Cần hạ tầng hỗ trợ tag | Listing bị ảnh hưởng bởi nhiều thao tác ghi |
| **Event-driven** (Redis Pub/Sub) | Đồng bộ gần tức thời toàn cụm | Phức tạp, cần message broker | Nhiều instance, cần nhất quán cao |
| **Versioning** | Không cần xoá chủ động, tránh race-condition | Tốn bộ nhớ tạm | Static assets, config ít đổi |

Khuyến nghị: kết hợp — TTL làm lưới an toàn cuối cùng + Tag-based/Event-driven để invalidate chủ động ngay khi có ghi.

### 4.6 Thiết kế Cache Key và Serialization

Quy ước: `{namespace}:{entity}:{id}:{sub-resource}`, VD `culinaryblog:recipe:42:comments`, `culinaryblog:recipes:popular:page:1:category:dessert`. Với endpoint có tham số (trang, filter, sort), **toàn bộ tổ hợp tham số** phải nằm trong key, nếu không hai request khác nhau vô tình dùng chung cache entry. Khi đổi schema class, nên đưa version vào key (dữ liệu cũ deserialize sai nếu không).

Serialization: JSON (`System.Text.Json`) là mặc định — dễ debug trên Redis CLI; chỉ chuyển sang Binary (MessagePack/Protobuf) khi đã đo được kích thước/tốc độ là điểm nghẽn thực sự.

### 4.7 HTTP Caching

`Cache-Control: public, max-age=3600` (mọi tầng được cache) / `private, no-store` (chỉ browser / không lưu) / `no-cache` (phải revalidate). **Conditional GET**: browser gửi `If-None-Match` (ETag) → server so khớp → trả **304 Not Modified** nếu không đổi, tiết kiệm băng thông. Phù hợp cho static assets, API công khai ít đổi — khác Server-side Cache ở chỗ server khó chủ động ép client xoá cache đã lưu.

### 4.8 Sự cố thực tế cần phòng tránh

| Vấn đề | Nguyên nhân | Giải pháp |
|---|---|---|
| **Cache Stampede** (Thundering Herd) | Key nóng hết hạn đồng loạt → nhiều request cùng miss, dội xuống DB | Locking/mutex, request coalescing, refresh-ahead, jitter TTL |
| **Cache Penetration** | Truy vấn liên tục key không tồn tại dữ liệu → luôn chạm DB | Cache cả kết quả "not found" (TTL ngắn), Bloom Filter |
| Stale data | TTL dài / thiếu invalidation chủ động | Tag-based / event-driven invalidation |
| Cache inconsistency | Race condition giữa ghi DB và ghi cache | Xoá cache sau khi commit, TTL ngắn, distributed lock |

### 4.9 Case Study CulinaryBlog — gợi ý áp dụng

| Tính năng | Loại cache / Pattern | TTL / Invalidation |
|---|---|---|
| Danh sách công thức (trang chủ) | Output Cache, tag `"recipes"` | TTL 2 phút + evict theo tag khi có recipe mới |
| Chi tiết 1 công thức | Redis, Cache-aside | TTL 10 phút, key `recipe:{id}` |
| Danh sách công thức phổ biến | IMemoryCache, Cache-aside | Sliding expiration 5 phút (per-instance) |
| Trang chi tiết (SEO metadata) | HTTP Caching | `public, max-age=600` |
| Bình luận theo công thức | Redis + event-driven invalidation | TTL 3 phút làm lưới an toàn |

**Liên hệ FR-CAT-001**: `GET /api/v1/categories` trả `{data: CategoryDto[]}` không phân trang, "được serve từ cache khi có" — danh mục là dữ liệu tĩnh, ít đổi → ứng với mô hình **IMemoryCache hoặc Output Cache** (không cần Redis) là hợp lý nhất trong 6 tầng cache nêu trên, vì đây là danh sách nhỏ, đọc rất nhiều, ghi rất hiếm.

**Best Practices**: luôn đặt TTL (không "vĩnh viễn"); ưu tiên Cache-aside làm mặc định; kết hợp TTL + invalidation chủ động; đưa đủ tham số vào cache key; theo dõi hit ratio thường xuyên. **Anti-pattern**: cache dữ liệu đổi liên tục mà không invalidate; cache dữ liệu nhạy cảm theo user ở tầng cache dùng chung; TTL quá dài "cho chắc" hoặc quá ngắn "cho an toàn" (mất tác dụng); bỏ invalidation "để tính sau".

---

## PHẦN V — KIẾN TRÚC FRONTEND VỚI NEXT.JS (Chương 4)

### 5.1 App Router và Server/Client Components

Next.js App Router: file-based routing trong thư mục `/app`, hỗ trợ **Route Groups** `(tenNhom)` để tổ chức route mà không ảnh hưởng URL (VD `(public)`, `(auth)`, `(dashboard)`).

| | Server Component (mặc định) | Client Component (`'use client'`) |
|---|---|---|
| Chạy | Server-only | Browser |
| Bundle size | 0 (không gửi về client) | Có, tính vào bundle |
| Fetch data | Trực tiếp, `async function` | Qua hook (TanStack Query) |
| Dùng cho | `RecipeDetail`, `RecipesPage`, layouts | `CommentSection`, `CreateRecipeForm`, `UserMenu` (cần hooks/events/browser API) |

### 5.2 4 chiến lược Rendering

| Chiến lược | Đặc điểm | Dùng cho |
|---|---|---|
| **CSR** | Render hoàn toàn phía client | SPA truyền thống (ít dùng trong App Router) |
| **SSR** | Render mỗi request — luôn fresh, cần user context | `recipes/[slug]/page.tsx` |
| **SSG** | Render lúc build, static hoàn toàn | Trang ít đổi |
| **ISR** | SSG + revalidate mỗi N giây (`revalidate: 3600`) | `recipes/page.tsx` (danh sách, revalidate 1 giờ) |

### 5.3 TanStack Query v5

Quản lý **Server State** (cache, stale-while-revalidate, refetch, deduplication) — khác với Client State (Zustand), URL State, Form State (React Hook Form).

- **Query Keys**: mảng (không phải string) để tạo hierarchy chính xác cho invalidation, VD `recipeKeys.detail(slug)`.
- **useQuery/useMutation/useInfiniteQuery**: `useInfiniteComments()` dùng cho phân trang vô hạn (Load More).
- **Optimistic Update**: cập nhật UI ngay trước khi server xác nhận (VD like recipe → icon đỏ ngay), rollback nếu lỗi — implement qua callback `onMutate/onError/onSettled`.

### 5.4 Auth.js v5

Session management với Credentials Provider + Google Provider, lưu **access token trong Session JWT (httpOnly cookie)** thay vì localStorage — an toàn hơn trước XSS (script không đọc được httpOnly cookie). `auth()` dùng ở Server Component, `useSession()` ở Client Component. `middleware.ts` bảo vệ route (`/dashboard`, `/recipes/new`) trước khi vào page.

### 5.5 React Hook Form + Zod

Zod định nghĩa schema validation runtime; `z.infer<typeof schema>` sinh TypeScript type tự động từ schema — đảm bảo runtime validation và compile-time type luôn nhất quán (single source of truth), không cần viết interface riêng. `useFieldArray` cho dynamic form fields (thêm/xoá steps, ingredients trong form tạo recipe).

### 5.6 Zustand — Client/UI State

State nhẹ, không cần Provider wrapping, dùng cho state thuần UI (sidebar, modal, view mode) — KHÔNG dùng cho Server State (đó là việc của TanStack Query). Middleware `persist` giữ state qua page reload (localStorage), `partialize` chọn lọc phần nào cần persist (VD chỉ persist `recipesViewMode`, không persist `activeModal`/`sidebarOpen`).

### 5.7 Next.js Image Optimization

Component `<Image>` tự động: chuyển WebP/AVIF, lazy load (trừ `priority`), resize theo `sizes` prop, sinh `blur` placeholder, ngăn Cascading Layout Shift (CLS). Ảnh remote (API server, MinIO, Google avatar) phải whitelist qua `images.remotePatterns` trong `next.config.ts`. `priority={true}` dùng cho ảnh LCP (ảnh đầu tiên nhìn thấy) để preload.

---

## PHỤ LỤC A — Phạm vi các slide bổ sung (S01–S09)

Các slide này minh hoạ lại và mở rộng nội dung 4 chương chính bằng ví dụ ngắn gọn hơn, không có chủ đề nào hoàn toàn mới ngoài phạm vi Phần I–III–V ở trên (ngoại trừ S10 — Caching, đã tổng hợp đầy đủ ở Phần IV):

| Slide | Chủ đề | Tương ứng chương |
|---|---|---|
| S01 | Nhập môn — tổng quan môn học, quy trình phát triển | Giới thiệu chung |
| S02 | Web Application Architectures (Monolith, layered, microservices) | Chương 1 |
| S03 | RESTful API và API Design | Chương 1 |
| S04 | ASP.NET 10 Minimal WebAPI | Chương 1 |
| S05 | ORM/EF Core cơ bản | Chương 3 |
| S06 | EF Core Advanced | Chương 3 |
| S07 | JWT Authentication | Chương 2 |
| S08 | Repository & Unit of Work | Chương 3 (xem lưu ý Phần III §3.2) |
| S09 | Query Optimization | Chương 3 |
| S10 | Caching Intro | Phần IV (tổng hợp đầy đủ, không trùng lặp chương nào) |

---

## PHỤ LỤC B — Bảng thuật ngữ Anh–Việt then chốt

| Thuật ngữ | Nghĩa |
|---|---|
| Dependency Rule | Nguyên tắc phụ thuộc chỉ trỏ vào trong (Clean Architecture) |
| Vertical Slice | Tổ chức code theo tính năng (feature), không theo tầng kỹ thuật |
| Eager/Explicit Loading | Tải trước / tải theo yêu cầu navigation property |
| Change Tracking | Cơ chế EF Core theo dõi thay đổi entity để sinh SQL UPDATE |
| Optimistic Concurrency | Kiểm soát đồng thời lạc quan — phát hiện xung đột khi ghi, không khoá trước |
| Token Rotation | Xoay vòng token — phát hành mới + vô hiệu hoá cũ mỗi lần refresh |
| Stale-while-revalidate | Trả dữ liệu cũ ngay trong lúc âm thầm lấy dữ liệu mới (TanStack Query) |
| Hit ratio | Tỉ lệ cache hit / tổng truy vấn cache |
| Idempotent | Gọi nhiều lần cho cùng kết quả (PUT, DELETE, GET) |

---

## PHỤ LỤC C — Nguồn tài liệu tham khảo mở rộng (tổng hợp)

**RFC / Tiêu chuẩn**: RFC 7519 (JWT), RFC 6749 (OAuth 2.0), RFC 7636 (PKCE), RFC 7807 (Problem Details), RFC 6585 (429 Too Many Requests).

**Tài liệu chính thức**: Microsoft Learn (ASP.NET Core Identity, JWT Bearer, Resource-based Authorization, Rate Limiting, EF Core Owned Entities/Value Conversions/Split Queries), PostgreSQL Docs (Full Text Search, Index Types), Next.js Docs (App Router, Data Fetching), TanStack Query v5 Docs, Auth.js v5 Docs, React Hook Form Docs, Zod Docs, Zustand Docs.

**Bài viết chuyên sâu**: OWASP Cheat Sheets (Authentication, JWT), PortSwigger Web Security Academy (JWT Attacks), Auth0 Blog (Refresh Token Rotation), NIST SP 800-63B, Martin Fowler — *Patterns of Enterprise Application Architecture* (Repository/UoW/Data Mapper), Jimmy Bogard — "No Repository Pattern Needed" (quan điểm phản biện — liên quan trực tiếp đến quyết định của dự án ở Phụ lục D), TkDodo — *Practical React Query*.

---

## PHỤ LỤC D — Ghi chú: điểm dự án Culinary Blog cố ý đi lệch giáo trình

Tài liệu bài giảng trình bày một cách tiếp cận "chuẩn giáo trình" để dạy khái niệm; dự án thực tế có thể chọn phương án khác vì lý do phù hợp với quy mô nhóm/đồ án. Ghi chú lại các điểm đã biết tính đến v1.3.0 của SRS.md, để tránh nhầm code mẫu trong bài giảng với quy ước thực tế của repo:

1. **Repository/Unit of Work** (Chương 3 §3.2, S08): giáo trình dạy đầy đủ pattern này; dự án dùng `IApplicationDbContext` trực tiếp trong CQRS Handler. Xem Phần III §3.2.
2. **Response Envelope**: các ví dụ Chương 3/4 dùng `PaginatedResult<T>`/`PagedResult<T>` phẳng làm response trả thẳng ra API; chuẩn thực tế của dự án là bọc trong `{data, meta}` theo Chương 5.2/8 của SRS.md. Xem Phần I §1.5.
3. **Sprint-0 runtime**: theo ADR-0001 của repo, chỉ containerize PostgreSQL (không containerize Redis/backend/frontend) ở giai đoạn hiện tại — các ví dụ cache Redis ở Phần IV là kiến thức chuẩn bị cho giai đoạn sau, chưa chắc đã cần thiết lập ngay.
4. **Tech stack version**: SRS.md/README tham chiếu Next.js 16.1.6 / React 19.2.8 thực tế trong `package.json`, một số ví dụ code trong Chương 4 và tài liệu tham khảo mở rộng dùng ngữ cảnh "Next.js 15" — không ảnh hưởng đến API (App Router ổn định giữa các minor version) nhưng cần lưu ý khi tra cứu docs chính thức.

*(Danh sách này nên được cập nhật bởi bất kỳ thành viên nào phát hiện thêm điểm khác biệt giữa giáo trình và code thực tế của repo, để tránh nhầm lẫn khi review PR.)*

---

*Hết tài liệu tổng hợp. Nguồn: 14 file trong thư mục `Bài_Giảng` (Chương 1–4 chính thức + S01–S10 bổ sung), đối chiếu với `SRS.md` v1.3.0, `Phan_Tich_Mau_Thuan_SRS_Culinary_Blog_v1.0.md`, và mã nguồn hiện tại của `PTUDWNC-2026-Nhom11`.*
