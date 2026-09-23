# TÀI LIỆU NHÁNH BACKEND — PHÂN CÔNG & LỘ TRÌNH THỰC HIỆN

**Tên nhánh:** `2312726_TranQuocQuan_Category_Search_backend`  
**Nhánh xuất phát:** `develop` (Commit `40ab20a`)  
**Thành viên phụ trách:** Trần Quốc Quân — MSSV: **2312726** (Thành viên 2)  
**Phân hệ chuyên trách:** **Category và Recipe Discovery** *(Quản lý Danh mục & Tra cứu, Tìm kiếm công thức)*  
**Chủ đề nghiệp vụ xuyên suốt:** *"Người dùng tìm món gì và xem món đó như thế nào"*

---

## 1. Căn cứ Phân công Công việc
Tài liệu này được lập dựa trên:
1. **Phân công bài tập Lab - Buổi 3** trong [`README.md`](./README.md) của nhóm.
2. **Đặc tả Yêu cầu Phần mềm (SRS v1.2.1)** tại [`docs/requirements/SRS.md`](./docs/requirements/SRS.md) (các module FR-CAT, FR-RCP, FR-SRCH).
3. **Nguyên tắc làm việc** tại [`.agents/rules/workflow.md`](./.agents/rules/workflow.md) (Nhánh độc lập, Commit tiếng Việt, Incremental Workflow).

---

## 2. Danh mục 10 Trách nhiệm Cốt lõi của Phân hệ

| # | Chức năng nghiệp vụ | Tầng Database | Tầng Backend CQRS & API | Giao diện Frontend |
|---|---|---|---|---|
| **1** | **Xem danh sách Danh mục** | Bảng `Categories`, nạp Bogus (20 categories) *(Xong Lab 2)* | `GetCategoriesQuery`, Redis cache `categories:all` (TTL 30m), `GET /api/v1/categories` | Lưới hiển thị danh mục, ảnh đại diện, số lượng recipe |
| **2** | **Xem chi tiết Danh mục** | Index B-Tree `IDX_Category_Slug` *(Xong Lab 2)* | `GetCategoryBySlugQuery`, `GET /api/v1/categories/{slug}` | Trang chi tiết danh mục kèm bài viết liên quan |
| **3** | **Tạo / Sửa / Xóa Danh mục** | Ràng buộc Unique `Name`, Unique `Slug`, `xmin` *(Xong Lab 2)* | `CreateCategoryCommand`, `UpdateCategoryCommand`, `DeleteCategoryCommand` (Soft delete, chặn xóa khi còn recipe) | Giao diện quản trị Admin: thêm, sửa, xóa mềm danh mục |
| **4** | **Xem danh sách Công thức** | Khóa ngoại `CategoryId` ↔ `Recipes` *(Xong Lab 2)* | `GetRecipesQuery` (lọc Published), phân trang Keyset/Offset, `GET /api/v1/recipes` | Thẻ bài viết công thức dạng lưới (Card Grid), ảnh, tác giả |
| **5** | **Xem chi tiết Công thức** | Liên kết bảng nguyên liệu, bước nấu, ảnh *(Xong Lab 2)* | `GetRecipeBySlugQuery`, nạp dinh dưỡng, nguyên liệu, bước nấu, `GET /api/v1/recipes/{slug}` | Trang đọc công thức chuẩn SEO, responsive mobile |
| **6** | **Tìm kiếm Full-Text Search** | PostgreSQL extension `unaccent`, cột `SearchVector`, GIN Index, Trigger | `SearchRecipesQuery`, xếp hạng Relevance, `GET /api/v1/recipes/search?q=...` | Thanh tìm kiếm thông minh có gợi ý (Autocomplete) |
| **7** | **Lọc công thức đa tiêu chí** | B-Tree Indexes trên các cột điều kiện | Xây dựng Specification/Predicate lọc theo độ khó, thời gian nấu, lượng calo | Sidebar bộ lọc đa năng |
| **8** | **Sắp xếp kết quả** | Index trên `PublishedAt` | Whitelist sắp xếp: Mới nhất, xem nhiều nhất, thời gian nấu nhanh nhất | Dropdown lựa chọn tiêu chí sắp xếp |
| **9** | **Phân trang dữ liệu** | Keyset / Offset Query Optimization | Envelope metadata `{ "page": 1, "pageSize": 12, "totalCount": ..., "totalPages": ... }` | Thanh chuyển trang phân trang mượt mà |
| **10** | **Tối ưu bộ nhớ đệm Redis** | Container Redis 7 trên Docker cổng 6379 | Chiến lược Cache-Aside, tự động Invalidate cache khi có mutation | Tối ưu hóa tải trang dưới 200ms |

---

## 3. Lộ trình 11 Commits Nguyên tử (Atomic Commits)
Nhánh được chia thành **11 commits độc lập**, mỗi commit hoàn thành trọn vẹn 1 nhiệm vụ và luôn đảm bảo **biên dịch thành công (Build Green)** để dễ dàng kiểm tra và backup / rollback khi cần thiết:

```text
develop (40ab20a)
   │
   ├── Commit 01: cấu hình: tích hợp MediatR, FluentValidation, Caching và DI
   ├── Commit 02: thêm: DTOs và Request Contracts cho phân hệ Category
   ├── Commit 03: thêm: CQRS Queries đọc danh mục món ăn kèm Redis cache
   ├── Commit 04: thêm: CQRS Commands và FluentValidation cho thêm sửa xóa danh mục
   ├── Commit 05: thêm: Minimal API Endpoints và xử lý ngoại lệ Problem Details cho Category
   ├── Commit 06: kiểm-thử: bổ sung unit test cho phân hệ Category và xác thực API
   ├── Commit 07: thêm: API duyệt danh sách và xem chi tiết công thức nấu ăn
   ├── Commit 08: thêm: bộ lọc công thức đa tiêu chí và sắp xếp kết quả
   ├── Commit 09: cấu hình: migration bổ sung SearchVector, GIN Index và trigger Full-Text Search
   ├── Commit 10: thêm: API tìm kiếm công thức tiếng Việt sử dụng PostgreSQL Full-Text Search
   └── Commit 11: kiểm-thử: hoàn tất kiểm thử và hoàn thiện toàn bộ phân hệ Backend
         │
         ▼
   Tạo Pull Request vào nhánh develop
```

---

## 4. Danh mục API Endpoints Quản lý trên Nhánh

| HTTP Method | Endpoint Route | Quyền truy cập | Trách nhiệm xử lý |
|---|---|---|---|
| `GET` | `/api/v1/categories` | Public | Đọc danh sách Category kèm RecipeCount, Redis Cache 30m |
| `GET` | `/api/v1/categories/{slug}` | Public | Đọc chi tiết danh mục theo Slug |
| `POST` | `/api/v1/categories` | Admin | Tạo danh mục mới, sinh Slug tự động, validate FluentValidation, xóa cache |
| `PUT` | `/api/v1/categories/{id:guid}` | Admin | Sửa Name/Description/Image, **giữ nguyên Slug**, xóa cache |
| `DELETE` | `/api/v1/categories/{id:guid}` | Admin | Xóa mềm danh mục. **Chặn xóa (409)** nếu còn Recipe active |
| `GET` | `/api/v1/recipes` | Public | Duyệt danh sách Recipe Published, phân trang & lọc theo CategoryId |
| `GET` | `/api/v1/recipes/{slug}` | Public | Đọc chi tiết bài viết công thức theo Slug kèm nguyên liệu, bước nấu |
| `GET` | `/api/v1/recipes/search` | Public | Tìm kiếm toàn văn bản tiếng Việt có dấu/không dấu, xếp hạng độ khớp |

---

## 5. Quy chuẩn Kỹ thuật & Nghiệm thu (Definition of Done)
- **Kiến trúc:** Tuân thủ Clean Architecture (.NET 10 Minimal API). `Application` độc lập, chỉ dùng `IApplicationDbContext`.
- **Validation:** 100% Request mutation đi qua FluentValidation và `ValidationBehavior`.
- **Định dạng Envelope:** Response thành công theo chuẩn `{ "data": ..., "meta": ... }`.
- **Xử lý lỗi:** Toàn bộ lỗi nghiệp vụ trả về chuẩn **RFC 7807 Problem Details** (400, 404, 409, 422).
- **Kiểm thử:** Đầy đủ Unit Tests cho Commands, Queries và Validators; 100% test suites pass trước khi gửi PR.
