# Dữ liệu công thức thật

Mỗi tệp `*.json` trong thư mục này là **một công thức**. Backend nạp chúng khi khởi động
(`RecipeDataImporter`) nếu bật tự migrate/seed (Development, hoặc `Database__AutoMigrate=true` như trong docker compose).

## Quy tắc

- **Chỉ ghi dữ kiện** (tên món, thời gian, khẩu phần, nguyên liệu và số lượng, số liệu dinh dưỡng) và **tự diễn đạt lại
  các bước bằng lời của nhóm**. Không chép nguyên văn nội dung trang nguồn.
- **Luôn ghi nguồn** trong `source` (tên + URL + ngày truy cập). Nguồn được nối vào cuối mô tả công thức.
- **Dinh dưỡng**: chỉ điền khi trang nguồn có số liệu; trường nào nguồn không có thì **để trống**, không ước lượng.
- **Ảnh**: điền `url`, `altText` và ghi `license` (nguồn/giấy phép). Hiện dùng trực tiếp ảnh của trang nguồn (liên kết, không tải về)
  kèm ghi nguồn, theo quyết định của người phụ trách dữ liệu, người chịu trách nhiệm về bản quyền. Nên thay bằng ảnh tự chụp khi có.
  Tên miền ảnh phải có trong `images.remotePatterns` của `frontend/next.config.ts`.
- **Tác giả**: `authorEmail` phải là tài khoản đã tồn tại. Thiếu tài khoản thì tệp bị bỏ qua và ghi cảnh báo.
- **Danh mục**: `categoryName` tìm theo tên (hoặc slug); chưa có thì tự tạo.
- Chạy lại an toàn: công thức đã có (trùng slug) sẽ bị bỏ qua, không nhân đôi dữ liệu.

## Định dạng

```json
{
  "title": "Tên món (3–200 ký tự)",
  "originalTitle": "Tên gốc trên trang nguồn (tùy chọn)",
  "description": "Mô tả 10–1800 ký tự (tự viết)",
  "categoryName": "Món canh",
  "authorEmail": "author@culinaryblog.local",
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

## Tiến độ nguồn tham khảo (nhóm trưởng gợi ý)

| Nguồn | Trạng thái |
|---|---|
| monngonmoingay.com — Canh sườn hạt sen | Đã nhập: `canh-suon-hat-sen.json` |
| recipetineats.com — Bún thịt nướng | Đã nhập: `bun-thit-nuong-sa.json` |
| allrecipes.com — Authentic Pho | Chưa nhập: công cụ không tải được trang, cần nhập tay hoặc dán nội dung |
| bbcgoodfood.com — Vietnamese chicken noodle soup | Chưa nhập: công cụ không tải được trang, cần nhập tay hoặc dán nội dung |
