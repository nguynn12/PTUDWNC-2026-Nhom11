# Resolved Conflicts — SRS v1.2.0 (Culinary Blog)

**Trạng thái:** Đã chốt, có hiệu lực áp dụng ngay. Ghi đè lên phần tương ứng trong `SRS.md` gốc khi có khác biệt.
**Phạm vi:** 27 mâu thuẫn được phát hiện khi rà soát SRS trước khi tạo migration nghiệp vụ đầu tiên, chia 5 nhóm. Bổ sung D7 (chốt từ `OPEN-QUESTIONS.md` mục 1, ngày 2026-09-23) — tổng 28 mục.
**Cách dùng cho AI coding agent:** Mỗi mục có RULE (quy tắc áp dụng ngay, không cần đọc phần giải thích để code đúng) + WHY (lý do, chỉ cần đọc nếu cần hiểu ngữ cảnh) + IMPACT (thay đổi cụ thể về schema/API/UI).

---

## Nhóm A — Data Lifecycle (Xóa dữ liệu)

### A1. Recipe — Soft Delete + Scheduled Hard Purge

- **RULE:** `DELETE /recipes/{id}` chỉ set `IsDeleted=true`, `DeletedAt=now`. Không cascade xóa DB thật, không xóa file MinIO ngay. 1 Hangfire Recurring Job (`PurgeExpiredRecipesJob`, chạy 03:00 AM hằng ngày) quét `IsDeleted=true AND DeletedAt < now-30d` → cascade hard-delete DB + xoá MinIO. Thêm endpoint `POST /admin/recipes/{id}/restore` và `DELETE /admin/recipes/{id}/purge` (Admin-only, xóa cứng ngay).
- **WHY:** SRS mâu thuẫn giữa FR-RCP-007 (hard delete) và BaseEntity/NFR-REL-003 (soft delete bắt buộc toàn hệ thống). Ưu tiên NFR vì đây là rule nền tảng, FR-RCP-007 là lỗi soạn thảo.
- **IMPACT (DB):** thêm cột `Recipe.DeletedAt (timestamptz, NULL)`. Global Query Filter `HasQueryFilter(r => !r.IsDeleted)` áp dụng cho Recipe + toàn bộ navigation con.
- **IMPACT (API):** giữ nguyên route `DELETE /recipes/{id}` (204). Thêm 2 route mới ở trên.
- **IMPACT (UI):** tab "Đã xóa" tại `/dashboard/recipes` + nút Khôi phục (30 ngày).

### A2. Category — Soft Delete thuần túy

- **RULE:** `DELETE /categories/{id}` chỉ set `IsDeleted=true`. Không có cơ chế 2-phase như Recipe (Category có sẵn rule chặn xóa khi còn Recipe con — xem FR-CAT-005: trả `409 CATEGORY_DELETE_HAS_RECIPES` nếu còn Recipe chưa soft-delete thuộc Category).
- **WHY:** đồng bộ với BaseEntity/NFR-REL-003, khớp với ghi chú "(soft delete)" ở đặc tả REST API.
- **IMPACT:** không đổi schema (Category đã kế thừa BaseEntity). Chỉ cần đảm bảo Global Query Filter áp dụng đúng.

---

## Nhóm B — Caching Strategy

### B1. Category Cache — Redis, TTL 30 phút

- **RULE:** dùng `IDistributedCache` (Redis qua `StackExchange.Redis`), **không dùng `IMemoryCache`** cho bất kỳ business data nào. TTL = **30 phút**. `IMemoryCache` chỉ được phép cho config tĩnh đọc từ `appsettings.json`.
- **WHY:** NFR-SCALE-001 cấm IMemoryCache (gây cache incoherence khi scale ngang ≥2 instance). TTL 30′ ưu tiên NFR-PERF-003 (định lượng, đo được) thay vì 60′ trong FR-CAT-001.
- **IMPACT:** DI `AddStackExchangeRedisCache`; interface dùng chung `ICacheService`; invalidate qua `EvictByTagAsync("categories")`.

### B2. Recipe List/Detail Cache — tách 2 cơ chế theo đúng vai trò

- **RULE:** Recipe **List** dùng **Output Caching middleware** (ASP.NET Core, backed by Redis storage provider để share giữa instance), TTL **15 phút**, vary by query string, invalidate qua `EvictByTagAsync("recipes")`. Recipe **Detail** dùng **Redis cache-aside**, TTL **5 phút** (không phải 60 phút).
- **WHY:** FR-RCP-002 (60′) mâu thuẫn NFR-PERF-003 (5′). Recipe Detail cần invalidate nhanh vì nội dung có thể sửa bất kỳ lúc nào bởi owner — 60′ quá dài.
- **IMPACT:** cấu hình `Microsoft.AspNetCore.OutputCaching.StackExchangeRedis`; 2 policy riêng `"RecipeList"` (15′) và `"RecipeDetail"` (5′, cache-aside, không dùng Output Cache middleware).

### B3. Search Cache — Redis, TTL 1 phút, bỏ phương án "không cache"

- **RULE:** Redis cache-aside, TTL = **1 phút**, cache key = hash toàn bộ query string search.
- **WHY:** FR-SRCH-001 mơ hồ ("không cache HOẶC 5 phút") mâu thuẫn NFR-PERF-003 (1 phút, định lượng rõ ràng).

---

## Nhóm C — API Convention & Business Rule

### C1. Sort Convention — `sortBy` + `sortOrder`

- **RULE:** mọi endpoint list dùng `?sortBy={field}&sortOrder={asc|desc}`. **Không dùng** cú pháp `sort=-field`. Default: `sortBy=createdAt&sortOrder=desc`.
- **WHY:** FR-RCP-001/FR-SRCH-003 dùng `sort=-createdAt`, Chương 8 (toàn bộ ví dụ endpoint) dùng `sortBy/sortOrder` — chọn theo Chương 8 vì binding tự nhiên hơn trong .NET 10 Minimal API, không cần custom parser.
- **IMPACT:** `GetRecipesQuery` có `SortBy` (enum: `createdAt|title|cookTime`) + `SortOrder` (enum: `asc|desc`), validate FluentValidation, sai giá trị → `422`.

### C2. Response Envelope — `{data, meta}` cho mọi response có body

- **RULE:**
  - List endpoint → `{"data": T[], "meta": {page, pageSize, total, totalPages, hasNextPage, hasPreviousPage}}`.
  - Detail/mutation endpoint → `{"data": T}`, không có `meta` (hoặc `null`).
  - **Ngoại lệ (KHÔNG bọc envelope):** response `204 No Content` (không có body); endpoint `/auth/*` trả token trực tiếp ở top-level (`{accessToken, refreshToken, expiresIn}`) theo chuẩn OAuth2/JWT phổ biến.
  - `PagedResult<T>` (namespace `Application/Common/Models/`) vẫn dùng nội bộ Application layer, nhưng **không** serialize trực tiếp ra HTTP response — luôn map qua `ApiResponse<T>`/`PagedApiResponse<T>` wrapper ở tầng Presentation trước khi trả JSON.
- **WHY:** Chương 3 dùng `PagedResult<T>` phẳng, Chương 5.2/8 dùng `{data,meta}`; nhiều endpoint ví dụ (auth, upload ảnh) trả DTO trần không bọc gì dù convention chung yêu cầu. Chọn `{data,meta}` vì nhất quán với RFC 7807 Error Format và dễ generate OpenAPI/TS type.
- **IMPACT:** tạo `ApiResponse<T>`/`PagedApiResponse<T>` qua Minimal API `IEndpointFilter`. **Đây là thay đổi breaking rộng nhất trong toàn bộ 27 mục — chốt trước khi viết Controller đầu tiên.**

### C3. Validation Error — `400` vs `422`, tách rõ theo ngữ nghĩa

- **RULE:** `400 Bad Request` chỉ dùng cho: JSON/query string malformed, sai kiểu dữ liệu cơ bản (không parse được), file MIME không hợp lệ. `422 Unprocessable Entity` dùng cho: FluentValidation rule (độ dài, range, required), business rule vi phạm (thiếu ingredient để publish, RowVersion/xmin conflict...).
- **WHY:** đa số FR dùng 422 cho validation, nhưng Phụ lục A gộp chung cả 2 khái niệm vào 400 — sai chuẩn RFC 7231.
- **IMPACT:** `ValidationBehavior` (MediatR pipeline) trả 422 thống nhất; `GlobalExceptionHandler` map JSON parse error → 400.

### C4. Mã lỗi `RECIPE_PUBLISH_INCOMPLETE` → `422`

- **RULE:** status code = 422 (không phải 400 như Phụ lục B ghi cũ). Hệ quả trực tiếp của C3.

### C5. Business rule Publish — bắt buộc CẢ Ingredient VÀ Step (≥1 mỗi loại)

- **RULE:** `recipe.Publish()` throw lỗi nếu `Ingredients.Count == 0` **HOẶC** `Steps.Count == 0` (không chỉ check Step như FR-RCP-005 gốc).
- **WHY:** Phụ lục B yêu cầu chặt hơn (cả 2 điều kiện) — hợp lý hơn về nghiệp vụ, tránh publish công thức không có nguyên liệu.
- **IMPACT (UI):** form tạo/sửa recipe hiển thị checklist "Điều kiện xuất bản", disable nút Publish nếu chưa đủ.

### C6. Concurrency Conflict → `409 Conflict`

- **RULE:** `RECIPE_CONCURRENCY_CONFLICT` (RowVersion/xmin mismatch) luôn trả **409**, không phải 422.
- **IMPACT:** `GlobalExceptionHandler` map `DbUpdateConcurrencyException` → 409; FE hiển thị dialog "Dữ liệu đã bị thay đổi, tải lại?" khi nhận 409 ở form edit.

### C7. Slug trùng — Tự động thêm hậu tố, slug bất biến sau lần Publish đầu tiên

- **RULE:** không reject 409 khi slug trùng. Tự động sinh `slug`, `slug-1`, `slug-2`... trong transaction (dùng lock/retry-on-conflict để tránh race condition). Sau khi Recipe đã từng Published lần đầu (`PublishedAt != null`), **slug không được đổi nữa** dù Title đổi sau đó.
- **IMPACT (DB):** `Recipe` cần cờ/field xác định "đã từng publish" (dùng `PublishedAt` sẵn có).

---

## Nhóm D — Auth Module

### D1. Trường Đăng ký/Hồ sơ — chuẩn hoá `displayName`, bỏ `fullName`

- **RULE:** Request đăng ký chỉ nhận `{displayName, email, password}`. `UserName` (cột built-in `IdentityUser`, dùng để login) tự sinh từ phần trước `@` của email, **không cho user tự chọn ở MVP**. Không dùng field `fullName` ở bất kỳ đâu.
- **WHY:** FR-AUTH dùng `fullName/userName`, nhưng schema thực tế chỉ có `DisplayName`, và mọi ví dụ API Chương 8 dùng `displayName` nhất quán.

### D2. Đăng nhập Google — Google Identity Services (ID Token), không dùng Authorization Code+PKCE qua Auth.js

- **RULE:** Frontend dùng Google Identity Services SDK (hoặc `@react-oauth/google`) lấy `idToken` trực tiếp từ Google, gửi lên `POST /api/v1/auth/google {idToken}`. Backend verify bằng `GoogleJsonWebSignature.ValidateAsync()`, tự phát hành JWT hệ thống. **Không** dùng Auth.js v5/NextAuth, không có OAuth redirect callback ở backend.
- **WHY:** SRS mô tả 3 luồng khác nhau ở 3 chỗ (Authorization Code+PKCE qua Auth.js FE; Redirect URI ở backend; `POST /auth/google {idToken}`) — chỉ luồng thứ 3 khớp với API đã đặc tả chi tiết ở Chương 8, đồng thời đơn giản nhất và khớp kiến trúc Stateless (NFR-SCALE-001).
- **IMPACT:** `AUTH_GOOGLE_TOKEN_INVALID` (400) khi verify thất bại.

### D3. Logout — không bắt buộc Access Token còn hạn

- **RULE:** `POST /auth/logout` chỉ cần `refreshToken` hợp lệ trong body để revoke, **không yêu cầu** Access Token hợp lệ trong header `Authorization`. Idempotent: luôn trả `204` kể cả khi refresh token đã revoke từ trước.
- **IMPACT:** bỏ `[Authorize]` bắt buộc khỏi endpoint này.

### D4. Refresh Token — 256-bit

- **RULE:** sinh bằng `RandomNumberGenerator.GetBytes(32)` (256-bit), encode Base64Url. Không dùng 512-bit hay 128-bit như 2 chỗ khác nhau trong SRS ghi.

### D5. Refresh Token state — `IsRevoked` là computed property, không phải cột DB

- **RULE:** Domain entity `RefreshToken` có computed property `public bool IsRevoked => RevokedAt.HasValue;`. **Không** tạo cột `IsRevoked` hay `ReplacedByToken` (plain text) trong DB — chỉ dùng `RevokedAt (timestamptz, NULL)` và `ReplacedByTokenHash (varchar(64), NULL)` đúng như schema 7.8.

### D6. BaseEntity — `ApplicationUser` và `RefreshToken` là ngoại lệ có chủ đích

- **RULE:** `BaseEntity` (Id/CreatedAt/UpdatedAt/IsDeleted/RowVersion) chỉ áp dụng cho Domain Content Entities (Recipe, Category, RecipeStep, RecipeIngredient, RecipeImage). `ApplicationUser` kế thừa `IdentityUser` (khoá `string`, varchar(450), giá trị sinh bằng `Guid.NewGuid().ToString()`), dùng `IsActive=false` để "deactivate" thay vì `IsDeleted`. `RefreshToken` chỉ có `Id, CreatedAt, ExpiresAt, RevokedAt, ReplacedByTokenHash` — không có `RowVersion`/`IsDeleted`.
- **LƯU Ý:** vị trí file `ApplicationUser.cs` đã được chốt — xem **D7** ngay dưới.

### D7. `ApplicationUser` đặt ở `Infrastructure` — Domain không phụ thuộc thư viện Identity

- **RULE:** `ApplicationUser` nằm tại `backend/src/CulinaryBlog.Infrastructure/Identity/ApplicationUser.cs` (namespace `CulinaryBlog.Infrastructure.Identity`). Project `CulinaryBlog.Domain` **không có `PackageReference` nào**. Entity trong Domain chỉ tham chiếu user bằng khoá chuỗi (`Recipe.AuthorId`, `RefreshToken.UserId` — `string`, max 450), **không** có navigation `Recipe.Author` / `RefreshToken.User`. Cần tên/ảnh tác giả → gọi `IUserQueryService.GetAuthorSummariesAsync(ids)` (Application interface, 1 truy vấn cho cả danh sách). Thao tác ghi trên user (đăng ký, đăng nhập, khoá tài khoản...) đi qua service ở Infrastructure dùng `UserManager<ApplicationUser>`.
- **WHY:** chốt `OPEN-QUESTIONS.md` mục 1 — SRS vừa đặt `ApplicationUser` trong Domain vừa cấm Domain dùng NuGet (NFR-MAINT-004), trong khi `ApplicationUser` bắt buộc kế thừa `IdentityUser`. Giữ nguyên rule NFR-MAINT-004, chuyển class sang Infrastructure (mẫu Clean Architecture chuẩn). Giữ khoá `string` (không đổi sang `Guid` như đề xuất tham khảo) để không phải đổi kiểu cột/khoá ngoại đã có trong DB.
- **IMPACT (DB):** **không đổi schema, không cần migration mới.** FK `Recipes.AuthorId → AspNetUsers.Id` (Restrict) và `RefreshTokens.UserId → AspNetUsers.Id` (Cascade) giữ nguyên, khai báo bằng `HasOne<ApplicationUser>()` trong `RecipeConfiguration` / `ApplicationUserConfiguration`. `ModelSnapshot` chỉ đổi tên kiểu CLR.
- **IMPACT (Code):** không viết `recipe.Author` hay `.Include(r => r.Author)` nữa. Test `backend/tests/CulinaryBlog.UnitTests/Architecture/DomainLayerTests.cs` tự động fail nếu Domain tham chiếu lại Identity / EF Core / Npgsql / ASP.NET Core / Application / Infrastructure.
- **Người chốt:** Thành viên 1 (2312682 — phụ trách module Auth), ngày 2026-09-23.

---

## Nhóm E — Recipe Content (Ingredient / Step / Image)

### E1. Quantity — giữ `decimal(10,3)`, xử lý phân số ở Presentation Layer

- **RULE:** DB/API luôn dùng `decimal`. Component `<QuantityInput>` (Next.js) parse 2 chiều chuỗi phân số (`1/2`) ↔ decimal (`0.5`) trước khi gửi API. API **không bao giờ** nhận chuỗi phân số.
- **IMPACT (UI):** dropdown phân số phổ biến (¼,⅓,½,⅔,¾) + input số; hiển thị lại bằng thuật toán continued-fraction rounding.

### E2. Quantity/Unit — Nullable, validate có điều kiện

- **RULE:** `Quantity` và `Unit` đều nullable (cho phép "nguyên liệu vừa đủ" không có số lượng). Validate: **nếu** `Quantity` có giá trị **thì** `Unit` bắt buộc đi kèm (và ngược lại) — không cho phép có 1 mà thiếu cái kia.
- **IMPACT (UI):** toggle "Vừa đủ (không cần số lượng cụ thể)" trong `<QuantityInput>`. Scale Servings: nguyên liệu `Quantity=null` giữ nguyên, không nhân hệ số.

### E3. Nutrition — Manual-only cho MVP

- **RULE:** chỉ cho phép Author nhập tay 4 field (`Calories, Protein, Carbs, Fat`) qua `SetNutrition()`. **Không** xây auto-calculate từ Ingredients ở giai đoạn này.
- **IMPACT (schema, chuẩn bị sẵn cho Phase 2, không breaking):** thêm cột `RecipeNutrition.Source varchar(20) NOT NULL DEFAULT 'Manual'` ngay từ bây giờ dù chưa dùng.
- **IMPACT (UI):** hiển thị disclaimer *"Giá trị dinh dưỡng do tác giả tự khai báo, chỉ mang tính tham khảo"*.

### E4. cookTime ≥ 0, prepTime > 0 (tách riêng, không gộp chung `>0`)

- **RULE:** `RuleFor(x => x.PrepTimeMinutes).GreaterThan(0)`; `RuleFor(x => x.CookTimeMinutes).GreaterThanOrEqualTo(0)` (cho phép 0 = món không cần nấu: salad, sinh tố).
- **IMPACT (UI):** hiển thị "Không cần nấu" thay vì "0 phút" khi `cookTimeMinutes == 0`.

### E5. RecipeStep — thêm cột `Description`, đổi `TimerMinutes` → `DurationMinutes`

- **RULE:** schema có đủ 4 field: `Title varchar(200) NOT NULL`, `Description text NULL`, `DurationMinutes integer NULL`, `ImageUrl`.
- **IMPACT (SQL):**
  ```sql
  ALTER TABLE "RecipeSteps" ADD COLUMN "Description" text NULL;
  ALTER TABLE "RecipeSteps" RENAME COLUMN "TimerMinutes" TO "DurationMinutes";
  ```
- **IMPACT (UI):** Cooking Mode hiển thị `Title` (heading lớn) + `Description` (chữ thường bên dưới).

### E6. Endpoint Ảnh chính — gộp vào PATCH chung, bỏ route `/primary` riêng

- **RULE:** dùng duy nhất `PATCH /api/v1/recipes/{id}/images/{imageId}` với body `{altText?, isPrimary?}`. Khi `isPrimary=true`, trong cùng transaction: set toàn bộ ảnh khác của Recipe về `IsPrimary=false` trước khi set ảnh hiện tại `=true`.

### E7. Response Upload Ảnh — bắt buộc đủ 4 field (fix bug functional, không chỉ style)

- **RULE:** response luôn là `{imageId: Guid, originalUrl: string, altText: string?, isPrimary: bool}`. **Bắt buộc có `imageId`** — thiếu field này khiến FE không thể gọi tiếp `DELETE`/`PATCH` ảnh sau khi upload (đây là bug chức năng thật, không chỉ là không nhất quán văn bản).

### E8. StepNumber — Flat-list Integer cho MVP + `ParentStepId` future-proof

- **RULE:** giữ đúng FR-RCP-010 (Integer tuyến tính, tự renumber). Ngay từ v1.0 thêm cột `ParentStepId (uuid, NULL, self-referencing FK)`, luôn `NULL` ở Phase hiện tại (UI chưa hỗ trợ bước con).
- **IMPACT (SQL):**
  ```sql
  ALTER TABLE "RecipeSteps" ADD COLUMN "ParentStepId" uuid NULL;
  ALTER TABLE "RecipeSteps" ADD CONSTRAINT "FK_RecipeSteps_ParentStep"
      FOREIGN KEY ("ParentStepId") REFERENCES "RecipeSteps"("Id") ON DELETE CASCADE;
  CREATE INDEX "IDX_RecipeStep_ParentStepId" ON "RecipeSteps" ("ParentStepId");
  ```

---

## Bảng tra nhanh (grep-friendly)

| # | Chủ đề | Quyết định 1 dòng |
|---|---|---|
| A1 | Recipe delete | Soft delete + purge job 30 ngày |
| A2 | Category delete | Soft delete thuần túy |
| B1 | Category cache | Redis, TTL 30′ |
| B2 | Recipe cache | Output Cache 15′ (list) / Redis cache-aside 5′ (detail) |
| B3 | Search cache | Redis, TTL 1′ |
| C1 | Sort param | `sortBy` + `sortOrder` |
| C2 | Response envelope | `{data, meta}`, trừ `204` và `/auth/*` |
| C3 | 400 vs 422 | 400=cú pháp, 422=nghiệp vụ |
| C4 | Publish error code | 422 |
| C5 | Publish rule | Cần cả Ingredient và Step |
| C6 | Concurrency | 409 |
| C7 | Slug trùng | Auto-suffix, immutable sau publish |
| D1 | Register field | `displayName` (bỏ `fullName`) |
| D2 | Google OAuth | Google Identity Services (idToken) |
| D3 | Logout | Không cần Access Token hợp lệ |
| D4 | Refresh token length | 256-bit |
| D5 | IsRevoked | Computed property từ `RevokedAt` |
| D6 | BaseEntity exception | User/RefreshToken ngoại lệ, khoá User là `string` |
| D7 | Vị trí ApplicationUser | `Infrastructure/Identity`; Domain không navigation tới User, dùng `IUserQueryService` |
| E1 | Quantity type | decimal, UX xử lý ở FE |
| E2 | Quantity/Unit nullable | Có, validate điều kiện |
| E3 | Nutrition | Manual-only MVP |
| E4 | cookTime | `>=0`, prepTime `>0` |
| E5 | RecipeStep field | Thêm `Description`, đổi `DurationMinutes` |
| E6 | Ảnh chính endpoint | PATCH chung |
| E7 | Upload response | Đủ 4 field, bắt buộc `imageId` |
| E8 | StepNumber | Flat + `ParentStepId` future-proof |
