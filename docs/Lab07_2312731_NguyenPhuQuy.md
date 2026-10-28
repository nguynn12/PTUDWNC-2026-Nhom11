# BÁO CÁO LAB - MÔN PHÁT TRIỂN ỨNG DỤNG WEB NÂNG CAO

**Lab:** 07  
**Thời gian:** 21/10/2026 đến 27/10/2026  
**MSSV:** 2312731  
**Họ và tên:** Nguyễn Phú Quý  
**Nhóm:** 11  
**Phân công nhiệm vụ cá nhân:** Phụ trách chính **Yêu cầu 4: Xây dựng và hoàn thiện trang xem chi tiết công thức (món ăn)** và **Hạ tầng lưu trữ tệp tin / Kiểm thử phân hệ Recipe**.

---

## 1. Đối chiếu hiện trạng 6 yêu cầu tối thiểu của Giảng viên (Lab 7)

Căn cứ theo bảng phân công và tiến độ triển khai của Nhóm 11:

| STT | Yêu cầu tối thiểu của Lab 7 | Thành viên phụ trách | Tình trạng triển khai | Mức độ hoàn thành |
|:---:|---|---|---|:---:|
| **1** | Cài đặt các tính năng: Đăng ký, đăng nhập, đăng xuất, đổi mật khẩu, quên mật khẩu | Liêng Hót Ha Luyến | Đã có UI & Backend cho Login, Register, Logout, Forgot/Reset Password. Endpoint đổi mật khẩu và dịch vụ gửi mail SMTP thực tế đang được tích hợp. | Đạt yêu cầu cốt lõi |
| **2** | Xây dựng trang hồ sơ (profile), cho phép cập nhật hồ sơ | Liêng Hót Ha Luyến | Đã hoàn thành trang `/profile`, form cập nhật thông tin cá nhân (tên hiển thị, bio, avatarUrl). | 100% |
| **3** | Xây dựng trang chủ (có header, footer, sidebar hiển thị category, danh sách công thức mới) | Trần Quốc Quân | Bố cục hoàn chỉnh với Navbar, Footer, Sidebar lọc danh mục dính (`sticky`), lưới hiển thị danh sách công thức mới nhất. | 100% |
| **4** | **Xây dựng trang xem chi tiết công thức (món ăn)** | **Nguyễn Phú Quý (Báo cáo cá nhân)** | **Đã hoàn thiện 100% giao diện chi tiết cao cấp, thư viện ảnh tương tác, bảng nguyên liệu định dạng phân số, các bước nấu có hẹn giờ, cảnh báo dinh dưỡng theo SRS, kiểm tra quyền tác giả/Admin, công thức liên quan và SEO Schema.org Recipe.** | **100% (Phần UI/UX & Logic)** |
| **5** | Cài đặt chức năng tìm kiếm | Tạ Nhật Nguyên | Trang `/search` với ô tìm kiếm có debounce (300ms), bộ lọc đa tiêu chí, PostgreSQL Full-Text Search có dấu và không dấu. | 100% |
| **6** | Xây dựng trang xem danh sách tất cả category | Trần Quốc Quân | Trang `/categories` dạng lưới hiển thị toàn bộ danh mục kèm ảnh đại diện, số lượng công thức và điều hướng chi tiết. | 100% |

---

## 2. Bảng phân công & Tiến độ công việc cá nhân (Nguyễn Phú Quý - MSSV: 2312731)

| STT | Nhiệm vụ được giao | Liên kết GitHub Branch | Tiến độ |
|---:|---|---|---:|
| 1 | **Xây dựng và hoàn thiện trang chi tiết công thức `RecipeDetail.tsx` với dữ liệu thực tế từ Backend.**<br><br>**Đã hoàn thành:**<br>• Hiển thị đầy đủ thông tin: Tiêu đề công thức, ảnh chính tỉ lệ 16:9 sắc nét, danh mục kèm badge và đường dẫn điều hướng, ngày đăng bài chuẩn hóa tiếng Việt (`vi-VN`), badge mức độ khó chuẩn màu ẩm thực (Dễ / Vừa / Khó);<br>• Khối thông số nhanh 5 cột: Thời gian chuẩn bị, thời gian nấu, tổng thời gian, khẩu phần ăn và năng lượng calo;<br>• Tích hợp tính năng **Thư viện ảnh tương tác (Interactive Image Gallery)**: Cho phép chuyển đổi linh hoạt giữa ảnh chính và dải thumbnail thu nhỏ của toàn bộ album ảnh công thức;<br>• Thông tin tác giả hiển thị chuyên nghiệp với ảnh đại diện bo tròn, tên tác giả và liên kết trang cá nhân.<br><br>**Chưa hoàn thành:** Không có. | [2312731_NguyenPhuQuy_Recipe-details_frontend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312731_NguyenPhuQuy_Recipe-details_frontend) | 100% |
| 2 | **Chuẩn hóa danh sách nguyên liệu theo Quyết định thiết kế E1 (Hiển thị phân số và nhãn "Vừa đủ").**<br><br>**Đã hoàn thành:**<br>• Triển khai hàm tiện ích `formatIngredientQuantity` chuyển đổi định lượng số thực sang ký tự phân số toán học trực quan (ví dụ: `0.5` ➔ `½`, `0.25` ➔ `¼`, `0.75` ➔ `¾`, `1.5` ➔ `1 ½`), giúp người nội trợ dễ dàng đong đếm gia vị theo thói quen nấu nướng thực tế;<br>• Tự động hiển thị nhãn `"Vừa đủ"` khi số lượng nguyên liệu không xác định (null, undefined hoặc 0) thay vì hiển thị số 0 trống rỗng;<br>• Bổ sung trường ghi chú sơ chế nguyên liệu (`ing.notes`) dạng chữ nghiêng tinh tế phía dưới tên nguyên liệu (ví dụ: *thái hạt lựu*, *xay nhuyễn*);<br>• Thống kê tổng số lượng nguyên liệu ngay trên thanh tiêu đề thẻ.<br><br>**Chưa hoàn thành:** Không có. | [2312731_NguyenPhuQuy_Recipe-details_frontend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312731_NguyenPhuQuy_Recipe-details_frontend) | 100% |
| 3 | **Trình bày trực quan các bước thực hiện, hẹn giờ và cảnh báo giá trị dinh dưỡng theo SRS.**<br><br>**Đã hoàn thành:**<br>• Trình bày các bước làm món ăn dạng danh sách thẻ Card với số thứ tự bước tròn nổi bật trên nền màu thương hiệu `--primary`;<br>• Giữ nguyên cấu trúc định dạng xuống dòng (`whitespace-pre-line`) của mô tả từng bước;<br>• Tích hợp Badge đồng hồ đếm ngược (`⏱️ Hẹn giờ: X phút`) cho các bước yêu cầu thời lượng nấu cụ thể;<br>• Hiển thị ảnh minh họa riêng biệt cho từng bước thực hiện;<br>• Khối giá trị dinh dưỡng 5 chỉ số (Calo, Đạm, Tinh bột, Chất béo, Chất xơ) kèm dòng trích dẫn pháp lý bắt buộc theo đặc tả SRS v1.3.0 Dòng 1295: *“Thông tin dinh dưỡng do tác giả cung cấp và chỉ mang tính tham khảo.”*.<br><br>**Chưa hoàn thành:** Không có. | [2312731_NguyenPhuQuy_Recipe-details_frontend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312731_NguyenPhuQuy_Recipe-details_frontend) | 100% |
| 4 | **Phân quyền hiển thị (Ownership Check), cảnh báo trạng thái bản nháp và khối Công thức liên quan.**<br><br>**Đã hoàn thành:**<br>• Tích hợp `useAuthStore` kiểm tra quyền tác giả (`user.id === recipe.authorId`) hoặc quyền Quản trị viên (`roles.includes("Admin")`);<br>• Hiển thị nút bấm *"Chỉnh sửa công thức"* dẫn thẳng tới `/dashboard/recipes/{id}/edit` khi chính chủ hoặc Admin đang xem bài viết;<br>• Hiển thị Banner cảnh báo màu vàng khi công thức ở trạng thái Bản nháp (`Draft`) hoặc Lưu trữ (`Archived`), nhắc nhở người xem đây là chế độ xem trước nội bộ;<br>• Tích hợp khối **"Công thức cùng chuyên mục" (Related Recipes)** ở cuối trang: Tự động truy vấn tối đa 4 công thức tương tự cùng danh mục và hiển thị dạng lưới thẻ `RecipeCard` responsive.<br><br>**Chưa hoàn thành:** Không có. | [2312731_NguyenPhuQuy_Recipe-details_frontend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312731_NguyenPhuQuy_Recipe-details_frontend) | 100% |
| 5 | **Tối ưu hóa SEO chuẩn Google với Metadata động và dữ liệu có cấu trúc Schema.org Recipe JSON-LD.**<br><br>**Đã hoàn thành:**<br>• Xây dựng hàm `generateMetadata` tại `app/recipes/[slug]/page.tsx` tự động sinh tiêu đề, mô tả tóm tắt, thẻ OpenGraph Facebook và Twitter Card chứa ảnh thumbnail chất lượng cao;<br>• Nhúng thẻ `<script type="application/ld+json">` chuẩn Schema.org định dạng `Recipe`: Bao gồm đầy đủ thông tin tên món, mô tả, ảnh, tác giả, thời gian sơ chế (`prepTime`), thời gian nấu (`cookTime`), khẩu phần (`recipeYield`), danh sách nguyên liệu và các bước làm (`HowToStep`);<br>• Cấu hình Incremental Static Regeneration (ISR) với `revalidate = 300` (5 phút) theo đúng SRS Mục 5.1.<br><br>**Chưa hoàn thành:** Không có. | [2312731_NguyenPhuQuy_Recipe-details_frontend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312731_NguyenPhuQuy_Recipe-details_frontend) | 100% |
| 6 | **Hạ tầng lưu trữ tệp tin (File Storage) và Kiểm thử phân hệ Backend.**<br><br>**Đã hoàn thành:**<br>• Dịch vụ lưu trữ tệp tin `FileStorageService.cs` kiểm tra định dạng an toàn (JPG, PNG, WebP), kiểm tra magic bytes, chống tấn công path traversal và giới hạn 5 MB;<br>• Cấu hình chính sách Upload Rate Limiting (5 requests/phút/IP) trả về mã lỗi HTTP 429 chuẩn RFC 7807;<br>• Cấu hình danh sách domain lưu trữ an toàn trong `images.remotePatterns` tại `next.config.ts`;<br>• Bộ Unit Tests backend đạt **274/274 tests passed (100% pass)**.<br><br>**Chưa hoàn thành:**<br>• Việc kết nối trực tiếp MinIO qua `AWSSDK.S3` và khởi chạy container MinIO trong `docker-compose.yml` đang được giữ ở chế độ fallback local disk (`wwwroot/uploads`) để phối hợp đồng bộ bucket policy cùng tính năng upload avatar của Thành viên 1. Lý do cá nhân: Em chủ động ưu tiên dồn thời gian hoàn thiện chỉn chu trải nghiệm người dùng, vi hiệu ứng phân số, gallery và SEO cho trang chi tiết công thức trước khi tích hợp S3 driver. | [2312731_NguyenPhuQuy_Recipe-details_backend](https://github.com/nguynn12/PTUDWNC-2026-Nhom11/tree/2312731_NguyenPhuQuy_Recipe-details_backend) | 90% |

---

## 3. Chi tiết Minh chứng Triển khai Kỹ thuật

### 3.1. Cơ chế hiển thị phân số và định lượng nguyên liệu (Quyết định thiết kế E1)
- **Tập tin:** `frontend/src/components/recipe/RecipeDetail.tsx`
- **Giải thuật xử lý:**
  ```typescript
  function formatIngredientQuantity(quantity?: number | null): string {
    if (quantity === null || quantity === undefined || quantity === 0) {
      return "Vừa đủ";
    }

    const whole = Math.floor(quantity);
    const remainder = Math.round((quantity - whole) * 1000) / 1000;

    const fractionMap: Record<string, string> = {
      "0.5": "½",
      "0.25": "¼",
      "0.75": "¾",
      "0.333": "⅓",
      "0.667": "⅔",
      "0.125": "⅛",
      "0.375": "⅜",
      "0.625": "⅝",
      "0.875": "⅞",
    };

    let fraction = "";
    for (const [key, glyph] of Object.entries(fractionMap)) {
      if (Math.abs(parseFloat(key) - remainder) < 0.02) {
        fraction = glyph;
        break;
      }
    }

    if (fraction) {
      return whole > 0 ? `${whole} ${fraction}` : fraction;
    }

    return Number(quantity.toFixed(2)).toString();
  }
  ```
- **Kết quả:** Hiển thị mượt mà định lượng ẩm thực: `½ muỗng cà phê`, `1 ½ chén`, `¼ thìa canh`, hoặc nhãn nổi bật `Vừa đủ` đối với hành lá, tiêu xay, rau thơm.

### 3.2. Quản lý bản quyền công thức (Owner Action & Draft Notice)
- Tích hợp `useAuthStore` từ Zustand:
  ```typescript
  const user = useAuthStore((s) => s.user);
  const isAuthorOrAdmin =
    Boolean(user) &&
    (user?.id === recipe.author?.id ||
      user?.id === (recipe as { authorId?: string }).authorId ||
      user?.roles?.includes("Admin"));
  ```
- Khi tác giả truy cập trang chi tiết công thức của chính mình, hệ thống lập tức hiển thị nút **Chỉnh sửa công thức** với icon cây bút dẫn về `/dashboard/recipes/{id}/edit`.
- Nếu bài viết ở trạng thái `Draft` hoặc `Archived`, banner thông báo nền vàng nhẹ (`amber-500/10`) xuất hiện giúp tác giả phân biệt rõ bài nháp nội bộ với bài viết công khai.

### 3.3. Dữ liệu có cấu trúc Schema.org Recipe (Chuẩn Google Rich Results)
Thẻ JSON-LD Schema.org được tự động khởi tạo và nhúng trực tiếp vào mã nguồn HTML:
```json
{
  "@context": "https://schema.org",
  "@type": "Recipe",
  "name": "Bò Lúc Lắc Thơm Ngon",
  "description": "Hướng dẫn chế biến món bò lúc lắc chuẩn vị nhà hàng...",
  "image": "https://images.unsplash.com/...",
  "author": {
    "@type": "Person",
    "name": "Bếp Trưởng Nguyễn Phú Quý"
  },
  "prepTime": "PT15M",
  "cookTime": "PT20M",
  "totalTime": "PT35M",
  "recipeYield": "4 khẩu phần",
  "recipeCategory": "Món Bò",
  "recipeIngredient": ["500 g Thịt thăn bò", "1 quả Ớt chuông", "Vừa đủ Hành tây"],
  "recipeInstructions": [
    {
      "@type": "HowToStep",
      "name": "Bước 1",
      "text": "Sơ chế và ướp thịt bò với tỏi, dầu hào, tiêu trong 15 phút."
    }
  ]
}
```

---

## 4. Kết quả Kiểm thử & Đóng gói Hệ thống

### 4.1. TypeScript Typecheck
- **Lệnh thực thi:** `npm run typecheck` tại thư mục `frontend`
- **Kết quả:** **Code 0 (Passed)**. Không có bất kỳ lỗi xung đột kiểu dữ liệu nào giữa các module.

### 4.2. Next.js Production Build
- **Lệnh thực thi:** `npm run build` (Turbopack engine)
- **Kết quả:** **Code 0 (Thành công 100%)**.
  - Biên dịch hoàn tất trong 11.5 giây.
  - Tối ưu hóa tĩnh 14/14 static pages.
  - Tuyến đường `/recipes/[slug]` hỗ trợ đầy đủ ISR revalidation 300s.

### 4.3. Backend Unit Tests
- **Lệnh thực thi:** `dotnet test tests/CulinaryBlog.UnitTests/CulinaryBlog.UnitTests.csproj`
- **Kết quả:** **Passed! Failed: 0, Passed: 274, Skipped: 0, Total: 274 (100% Pass)**.
  - Kiểm thử đầy đủ phân hệ Recipe Content, Rate Limiting, Ownership Check, Image Services.

### 4.4. Kiểm thử hiển thị Đa thiết bị (Responsive Design)
- **Mobile (375px - 430px):**
  - Khối thông số nhanh tự co giãn 2 cột.
  - Dải thumbnail ảnh gallery cuộn mượt mà theo phương ngang.
  - Bảng nguyên liệu và các bước làm hiển thị 1 cột thẳng hàng, không bị tràn màn hình.
- **Tablet (768px - 1024px):**
  - Khối thông số chia 4 cột cân đối.
  - Lưới công thức liên quan hiển thị 2 cột trực quan.
- **Desktop (1280px trở lên):**
  - Bố cục 2 cột bất đối xứng tỷ lệ vàng: Cột trái (5/12) gồm Thẻ nguyên liệu bo góc + Thẻ dinh dưỡng; Cột phải (7/12) là các bước thực hiện chi tiết kèm ảnh minh họa và đồng hồ đếm ngược.
  - Lưới công thức liên quan 4 cột hiện đại.

---

## 5. Tự đánh giá và Kết luận

- **Mức độ hoàn thành nhiệm vụ Lab 7:** **98%**
- Toàn bộ tính năng thuộc Yêu cầu 4 của Giảng viên (xem chi tiết công thức, hiển thị nguyên liệu, các bước nấu, ảnh gallery, phân quyền tác giả, công thức cùng chuyên mục và SEO) đã hoàn thành xuất sắc và vận hành ổn định trên giao diện người dùng.
- Về phần hạ tầng lưu trữ S3/MinIO, mã nguồn đã sẵn sàng với bộ kiểm tra bảo mật nghiêm ngặt và sẽ hoàn tất kết nối container phân tán trong buổi làm việc chung tiếp theo của nhóm.
