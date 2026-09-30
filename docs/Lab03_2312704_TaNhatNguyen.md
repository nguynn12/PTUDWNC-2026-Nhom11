# BÁO CÁO LAB - MÔN PHÁT TRIỂN ỨNG DỤNG WEB NÂNG CAO

**Lab:** 03  
**Từ ngày:** 23/09/2026 **đến ngày:** 30/09/2026  
**MSSV:** 2312704  
**Họ và tên:** Tạ Nhật Nguyên  
**Nhóm:** 11

## Công việc

| STT | Công việc được giao | Liên kết đến GitHub branch | Tiến độ % |
|---:|---|---|---:|
| 1 | Cài đặt `CreateRecipeCommand`, Validator và Handler khởi tạo công thức ở trạng thái Draft, tự sinh Slug tiếng Việt URL-friendly và xử lý hậu tố khi trùng lặp. | [2312704_TaNhatNguyen_Recipe-lifecycle_backend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312704_TaNhatNguyen_Recipe-lifecycle_backend) | 100% |
| 2 | Cài đặt `UpdateRecipeCommand`, Validator và Handler hỗ trợ kiểm tra quyền tác giả, bất biến slug sau xuất bản, lưu lịch sử `RecipeSlugHistory` và kiểm soát concurrency bằng `xmin`. | [2312704_TaNhatNguyen_Recipe-lifecycle_backend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312704_TaNhatNguyen_Recipe-lifecycle_backend) | 100% |
| 3 | Cài đặt `PublishRecipeCommand` và `UnpublishRecipeCommand` xử lý chuyển trạng thái vòng đời, kiểm tra điều kiện tối thiểu 1 nguyên liệu và 1 bước nấu, ghi nhận mốc `PublishedAt`. | [2312704_TaNhatNguyen_Recipe-lifecycle_backend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312704_TaNhatNguyen_Recipe-lifecycle_backend) | 100% |
| 4 | Cài đặt `ArchiveRecipeCommand` và `UnarchiveRecipeCommand` xử lý lưu trữ và khôi phục trạng thái công thức. | [2312704_TaNhatNguyen_Recipe-lifecycle_backend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312704_TaNhatNguyen_Recipe-lifecycle_backend) | 100% |
| 5 | Cài đặt `DeleteRecipeCommand` và Handler thực hiện xóa mềm (Soft Delete) công thức nấu ăn. | [2312704_TaNhatNguyen_Recipe-lifecycle_backend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312704_TaNhatNguyen_Recipe-lifecycle_backend) | 100% |
| 6 | Cài đặt toàn bộ Minimal API endpoints cho module Recipe Lifecycle tại `/api/v1/recipes`, hỗ trợ đầy đủ HTTP method (`POST`, `PUT`, `PATCH`, `DELETE`) và header `If-Match`/`ETag`. | [2312704_TaNhatNguyen_Recipe-lifecycle_backend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312704_TaNhatNguyen_Recipe-lifecycle_backend) | 100% |
| 7 | Cài đặt lớp cơ sở `DomainException` và các exception nghiệp vụ tại tầng Domain; tích hợp `GlobalExceptionHandler` trả chuẩn RFC 7807 Problem Details. | [2312704_TaNhatNguyen_Recipe-lifecycle_backend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312704_TaNhatNguyen_Recipe-lifecycle_backend) | 100% |
| 8 | Cài đặt `ICurrentUserService` trích xuất định danh tác giả từ JWT context và cấu hình `ValidationBehavior` trong MediatR pipeline. | [2312704_TaNhatNguyen_Recipe-lifecycle_backend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312704_TaNhatNguyen_Recipe-lifecycle_backend) | 100% |
| 9 | Xây dựng bộ Unit Tests kiểm thử SlugHelper, Command Validators và Recipe Lifecycle State Transitions; đạt 28/28 tests passed 100%. | [2312704_TaNhatNguyen_Recipe-lifecycle_backend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312704_TaNhatNguyen_Recipe-lifecycle_backend) | 100% |

## Tự đánh giá

Hoàn thành 100% các yêu cầu được phân công cho Thành viên 3 trong Lab 3 về module Recipe Lifecycle. Mã nguồn tuân thủ Clean Architecture, sử dụng đúng mô hình Generic Repository và Unit of Work cho phía Command, kiểm soát chặt chẽ quyền tác giả, tính toàn vẹn trạng thái và concurrency bằng PostgreSQL xmin. Toàn bộ API và Unit Tests đã được build sạch và vượt qua kiểm thử thành công trên hệ sinh thái .NET của dự án.
