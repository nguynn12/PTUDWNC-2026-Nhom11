## Phân công bài tập Lab - Buổi 3

**Mục tiêu:** Triển khai các chức năng Backend chính dựa trên mô hình dữ liệu đã hoàn thành ở Buổi 2. Mỗi thành viên phụ trách một module xuyên suốt từ xử lý nghiệp vụ đến API. Cả nhóm phối hợp hoàn thiện Domain Exceptions, Repository, Unit of Work và middleware xử lý lỗi toàn cục.

### Phân công theo thành viên

| Hạng mục | Liêng Hót Ha Luyến | Trần Quốc Quân | Tạ Nhật Nguyên | Nguyễn Phú Quý |
|---|---|---|---|---|
| **Module phụ trách** | Tài khoản và xác thực | Category và Recipe Discovery | Recipe Lifecycle | Recipe Content và Media |
| **Command, Validator và Handler** | Register, Login, Refresh Token, Logout và xác thực Email | Create, Update, Delete Category; Category List, Category Detail, Recipe List, Recipe Detail và Search | Create, Update, Publish, Unpublish, Archive, Unarchive và Delete Recipe | Add, Update, Delete Ingredient; Add, Update, Delete Recipe Step; quản lý Recipe Image |
| **Nghiệp vụ và truy vấn** | Cài đặt `IdentityService`; tạo JWT Access Token và quản lý Refresh Token | Cài đặt phân trang, lọc, sắp xếp và PostgreSQL Full-Text Search cho Recipe | Kiểm tra quyền sở hữu; xử lý quy tắc chuyển trạng thái; cập nhật `RecipeSlugHistory`; kiểm soát concurrency bằng `xmin` và ETag/`If-Match` | Tích hợp MinIO; quản lý ảnh chính `IsPrimary`, thứ tự `OrderIndex` và liên kết ảnh với Recipe |
| **Minimal API endpoints** | Tạo endpoints cho chức năng tài khoản và xác thực. Tối thiểu 2 endpoints, gồm Register và Login | Tạo endpoints cho Category, Recipe List, Recipe Detail và Search. Tối thiểu 2 endpoints | Tạo endpoints cho Recipe Lifecycle. Tối thiểu 2 endpoints | Tạo endpoints cho Ingredient, Step và Image. Tối thiểu 2 endpoints |
| **Kiểm tra chức năng** | Kiểm tra luồng Register → Login → Access Token → Refresh Token → API có phân quyền; kiểm tra token sai/hết hạn và tài khoản không hợp lệ | Kiểm tra Category, Recipe List/Detail, phân trang, lọc, sắp xếp và Full-Text Search bằng dữ liệu PostgreSQL | Kiểm tra Recipe không tồn tại, sai chủ sở hữu, sai trạng thái và concurrency conflict | Kiểm tra thêm/sửa/xóa Ingredient, Step; upload ảnh và liên kết dữ liệu với Recipe |

### Công việc chung của cả nhóm

| Hạng mục | Công việc cần hoàn thành |
|---|---|
| **Tích hợp và cấu trúc** | Đồng bộ mã nguồn Database từ Buổi 2; thống nhất cấu trúc `Domain`, `Application`, `Infrastructure`, `API`; kiểm tra PostgreSQL sau khi tích hợp |
| **Domain Exceptions** | Cài đặt các lớp Domain Exceptions cần thiết trong `Domain`; thống nhất exception cho lỗi nghiệp vụ và dữ liệu |
| **Repository và Unit of Work** | Khai báo interface; cài đặt Repository và Unit of Work trong `Infrastructure`; đăng ký Dependency Injection; bảo đảm các thao tác cần tính nhất quán dùng transaction phù hợp |
| **Middleware xử lý lỗi** | Tạo middleware toàn cục trong `API` để bắt exception, ánh xạ sang HTTP status code phù hợp và trả RFC 7807 Problem Details; không đưa stack trace hoặc thông tin nhạy cảm vào response |
| **Tích hợp Backend** | Thống nhất định dạng Request, Response và mã lỗi; cấu hình MediatR, FluentValidation, ASP.NET Core Identity, JWT Authentication và các package EF Core/PostgreSQL cần thiết |
| **Kiểm thử và review** | Kiểm tra API bằng Swagger, HTTP Client hoặc Postman; review chéo trước khi merge vào `develop`; chạy `dotnet build` và sửa lỗi sau tích hợp |

### Kết quả cần đạt cuối Buổi 3

| Nhóm chức năng | Kết quả cần đạt |
|---|---|
| **Yêu cầu tối thiểu của Lab** | Hoàn thành Domain Exceptions; Repository và Unit of Work; mỗi thành viên có ít nhất 2 API endpoints; middleware toàn cục trả RFC 7807 Problem Details |
| **Tài khoản và phân quyền** | Register, Login, JWT Access Token, Refresh Token và Authorization theo `Author`/`Admin` hoạt động |
| **Category và Discovery** | API Category, Recipe List, Recipe Detail và Search hoạt động; phân trang, lọc, sắp xếp và Full-Text Search được kiểm tra |
| **Recipe Lifecycle** | Create, Update, Publish, Unpublish, Archive, Unarchive và Delete hoạt động; kiểm tra quyền sở hữu và concurrency |
| **Recipe Content và Media** | API Ingredient, Recipe Step, Recipe Image hoạt động; upload file được tích hợp với MinIO |
| **Tích hợp và build** | Các API chính được kiểm thử; toàn bộ Backend build thành công sau khi merge |