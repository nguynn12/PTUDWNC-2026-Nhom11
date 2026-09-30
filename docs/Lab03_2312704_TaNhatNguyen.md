# BÁO CÁO LAB - MÔN PHÁT TRIỂN ỨNG DỤNG WEB NÂNG CAO

**Lab:** 03  
**Từ ngày:** 23/09/2026 **đến ngày:** 30/09/2026  
**MSSV:** 2312704  
**Họ và tên:** Tạ Nhật Nguyên  
**Nhóm:** 11

## Công việc

| STT | Công việc được giao | Liên kết đến GitHub branch | Tiến độ % |
|---:|---|---|---:|
| 1 | **Triển khai phân hệ Backend cho Recipe Lifecycle**<br><br>**Đã hoàn thành:**<br>- Tạo đầy đủ Command, Validator và Handler cho các chức năng: Create, Update, Publish, Unpublish, Archive, Unarchive và Delete Recipe.<br>- Áp dụng generic `IRepository<T>` và `IUnitOfWork` cho các thao tác ghi dữ liệu phía Command theo yêu cầu Lab.<br>- Kiểm tra quyền sở hữu tác giả (`AuthorId`) qua `ICurrentUserService`, xử lý mã lỗi `RECIPE_FORBIDDEN` (403).<br>- Tự sinh Slug tiếng Việt URL-friendly, xử lý hậu tố khi trùng lặp, giữ slug bất biến sau khi xuất bản và lưu vết `RecipeSlugHistory`.<br>- Kiểm soát concurrency qua PostgreSQL `xmin` và header `If-Match`/`ETag`, xử lý xung đột `RECIPE_CONCURRENCY_CONFLICT` (409).<br>- Kiểm tra điều kiện xuất bản (tối thiểu 1 nguyên liệu và 1 bước nấu) theo quy tắc C4, C5, trả mã lỗi `RECIPE_PUBLISH_INCOMPLETE` (422).<br>- Xây dựng 7 Minimal API endpoints tại `/api/v1/recipes` hỗ trợ chuẩn HTTP method (`POST`, `PUT`, `PATCH`, `DELETE`).<br>- Cài đặt các lớp `DomainException` tại tầng Domain và cấu hình `GlobalExceptionHandler` chuẩn RFC 7807 Problem Details.<br>- Viết bộ Unit Tests kiểm thử validation, slug và chuyển trạng thái; chạy `dotnet test` đạt 28/28 tests passed (100%). | [2312704_TaNhatNguyen_Recipe-lifecycle_backend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312704_TaNhatNguyen_Recipe-lifecycle_backend) | 100% |

## Tự đánh giá

Hoàn thành 100% công việc được phân công cho Thành viên 3 trong Lab 3 về module Recipe Lifecycle. Mã nguồn tuân thủ Clean Architecture, sử dụng đúng mô hình Generic Repository và Unit of Work cho phía Command, kiểm soát chặt chẽ quyền tác giả, tính toàn vẹn trạng thái và concurrency bằng PostgreSQL xmin. Toàn bộ API và Unit Tests đã được build sạch (0 error, 0 warning) và vượt qua 28/28 bài kiểm thử thành công trên hệ sinh thái .NET của dự án.
