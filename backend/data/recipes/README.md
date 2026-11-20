# Dữ liệu công thức thật

Mỗi tệp `*.json` trong thư mục này là **một công thức**. Backend nạp chúng khi khởi động
(`RecipeDataImporter`) trong Development, hoặc khi bật cả `Database__AutoMigrate=true` và `SeedData__Enabled=true`.

## Quy tắc

- **Chỉ ghi dữ kiện** (tên món, thời gian, khẩu phần, nguyên liệu và số lượng, số liệu dinh dưỡng) và **tự diễn đạt lại
  các bước bằng lời của nhóm**. Không chép nguyên văn nội dung trang nguồn.
- **Luôn ghi nguồn** trong `source` (tên + URL + ngày truy cập) để đối chiếu dữ liệu. Bộ nạp giữ nguyên mô tả đã biên tập, không tự nối nguồn vào nội dung bài.
- **Dinh dưỡng**: chỉ điền khi trang nguồn có số liệu; trường nào nguồn không có thì **để trống**, không ước lượng.
- **Ảnh**: điền `url`, `altText` và ghi `license` (nguồn/giấy phép). Hiện dùng trực tiếp ảnh của trang nguồn (liên kết, không tải về)
  kèm ghi nguồn, theo quyết định của người phụ trách dữ liệu, người chịu trách nhiệm về bản quyền. Nên thay bằng ảnh tự chụp khi có.
  Tên miền ảnh phải có trong `images.remotePatterns` của `frontend/next.config.ts`.
- **Tác giả**: `authorEmail` phải là tài khoản đã tồn tại. Thiếu tài khoản thì tệp bị bỏ qua và ghi cảnh báo.
- **Danh mục**: `categoryName` tìm theo tên (hoặc slug); chưa có thì tự tạo.
- Chạy lại an toàn: công thức đã có (trùng slug) sẽ bị bỏ qua, không nhân đôi dữ liệu.
- DB đang có dữ liệu được giữ nguyên: bộ nạp chỉ thêm công thức mới, không tự xóa hoặc thay thế công thức cũ.
- Khởi động chỉ tạo tài khoản cố định và nhập JSON; không tự sinh tài khoản, danh mục, công thức hoặc chi tiết bằng Bogus. Thiếu JSON thì không tự thay bằng dữ liệu ngẫu nhiên.

## Định dạng

```json
{
  "title": "Tên món (3–200 ký tự)",
  "originalTitle": "Tên gốc trên trang nguồn (tùy chọn)",
  "description": "Mô tả 10–1800 ký tự (tự viết)",
  "categoryName": "Món canh",
  "authorEmail": "author@culinaryblog.com",
  "prepTimeMinutes": 10,
  "cookTimeMinutes": 15,
  "servings": 4,
  "difficulty": "Easy | Medium | Hard | Expert",
  "source": { "name": "Tên trang", "url": "https://...", "accessedOn": "yyyy-MM-dd", "note": "Ghi chú" },
  "nutrition": { "calories": 0, "protein": 0, "carbohydrates": 0, "fat": 0, "fiber": 0, "sodium": 0 },
  "ingredients": [ { "name": "...", "quantity": 1, "unit": "g", "notes": "..." } ],
  "steps": [ { "title": "...", "description": "...", "durationMinutes": 5 } ],
  "images": [ { "url": "...", "altText": "...", "license": "..." } ]
}
```

## Kiểm tra tích hợp ngày 17/11/2026

| Nguồn khai báo trong JSON | Số công thức | Kiểm tra liên kết ảnh |
|---|---:|---|
| Allrecipes | 10 | Môi trường kiểm tra tự động nhận 403; người dùng xác nhận mở được link trong trình duyệt của mình. Chưa kết luận ảnh lỗi với mọi người dùng |
| BBC Good Food | 6 | 6 ảnh tải được |
| RecipeTin Eats | 5 | 5 ảnh tải được |
| Món Ngon Mỗi Ngày | 1 | 1 ảnh tải được |

- Đã thử trên PostgreSQL riêng: DB mới có 22 công thức, 283 nguyên liệu, 112 bước và 22 bản ghi ảnh. Bản ghi ảnh tồn tại không chứng minh liên kết ảnh tải được.
- Bản sao DB cũ tăng từ 100 lên 122 công thức; khởi động lại vẫn 122. Cache danh sách/danh mục được hủy sau khi nhập thêm món.
- Sau tích hợp, đã nhập 22 món vào DB demo `culinary_blog_dev`, rồi xóa đúng 100 món Bogus theo yêu cầu. Đã sao lưu trước khi xóa. DB hiện có 22 công thức / 283 nguyên liệu / 112 bước / 22 ảnh.
- Đã xóa 15 tài khoản Bogus, 5 danh mục mẫu không dùng; 15 danh mục còn lại dùng ảnh từ món thực tế trong danh mục, bỏ mô tả ngẫu nhiên và ảnh Picsum. Giữ tài khoản cố định và các tài khoản do người dùng/kiểm thử tạo riêng.
- Chưa nghiệm thu đối chiếu từng định lượng và số liệu dinh dưỡng với trang nguồn. Các tài khoản tác giả là tài khoản demo của dự án, không phải tác giả gốc của các trang nguồn.
- Khác mục 6 của kế hoạch trước demo: bộ nạp hiện ghi qua DbContext và liên kết ảnh ngoài, chưa tạo qua Command và chưa tải ảnh lên MinIO. Chưa đánh dấu DATA-01…03 hoàn thành.

## Bổ sung 20 công thức trong PR 40

- Kho JSON có 42 công thức thuộc 17 danh mục: giữ 22 món cũ, thêm 16 món từ tài liệu tham chiếu Savoury Days và 4 món từ Helen's Recipes.
- 20 ảnh mới lấy từ đúng bài nguồn, đã tải kiểm tra và xem ảnh từng món; thay toàn bộ ảnh Unsplash không đúng món. Không nối thông tin nguồn vào mô tả bài trên blog.
- Ảnh bên ngoài được hiển thị với `referrerPolicy="no-referrer"` để tránh bị Savoury Days chặn khi nhúng từ tên miền khác. Kiểm thử ảnh kiểm cả ảnh tải thật, không chỉ kiểm URL.
- Định lượng hỗn hợp được khôi phục từ Word và tách thành nguyên liệu riêng khi có số lượng từng thành phần. Nguyên liệu ghi "vừa đủ" giữ `quantity: null`, `unit: null` cùng ghi chú, không tự đặt số 1.
- 20 món mới để `nutrition: null` vì các số trong Word là ước tính chưa có bảng tính kiểm chứng. Không thay số thiếu bằng 0; trang chi tiết không hiện bảng dinh dưỡng trống.
- DB demo đã có 42 món / 540 nguyên liệu / 199 bước / 42 ảnh / 17 danh mục. Bộ nạp không tự cập nhật các slug đã tồn tại.
- Rà ngày 18/11: 10 link ảnh `www.allrecipes.com/thmb` trả 403. JSON chuyển sang ảnh gốc công khai `images.media-allrecipes.com/userphotos` lấy từ thư viện ảnh của đúng bài nguồn; đã xem lại nội dung từng ảnh. Trên máy demo, 10 ảnh này được lưu đủ bản gốc, medium và thumbnail trong MinIO, cập nhật 10 bản ghi ảnh và 6 ảnh danh mục dùng chung. Không thay quyền truy cập bucket. Kiểm tra lại 42/42 ảnh tải và giải mã được.
- Dinh dưỡng hiện có ở 22 món; 20 món mới vẫn chưa có số liệu có căn cứ. Giao diện báo rõ khi thiếu dữ liệu và không quảng bá toàn bộ công thức đều có dinh dưỡng.
- Các định lượng được bảo toàn theo tài liệu nhóm, chưa xác minh toàn bộ với bài nguồn. Việc ảnh tải được và JSON nhập thành công không thay thế nghiệm thu nội dung từng công thức.
