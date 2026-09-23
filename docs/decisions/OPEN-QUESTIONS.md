# Open Questions — Chưa chốt, KHÔNG tự implement

**Trạng thái:** 4 điểm dưới đây vẫn còn mơ hồ hoặc tự mâu thuẫn trong `SRS.md` v1.2.0, kể cả sau khi đã áp dụng toàn bộ `RESOLVED-CONFLICTS.md`. Chưa có quyết định chính thức từ nhóm.

**QUY TẮC BẮT BUỘC cho AI coding agent:** nếu 1 task được giao chạm vào bất kỳ mục nào dưới đây (tên file/class/bảng trùng khớp), **dừng lại và hỏi người dùng trước khi viết code**, dù trong mục có ghi "đề xuất" — đề xuất chỉ là gợi ý tham khảo, **không phải quyết định đã chốt**, không được tự ý áp dụng.

---

## 1. 🔴 [BLOCKING] `ApplicationUser` đặt sai layer so với chính rule của SRS

**Vấn đề:** `docs/architecture/README.md` / SRS liệt kê `ApplicationUser` nằm trong project `CulinaryBlog.Domain`, đồng thời quy định layer này *"Không có NuGet dependencies (chỉ .NET BCL)"*. Nhưng `ApplicationUser` (mục 7.7 SRS) bắt buộc kế thừa `IdentityUser<string>` — 1 class từ package `Microsoft.AspNetCore.Identity`. Hai rule này loại trừ lẫn nhau.

**Vì sao blocking:** đây là quyết định vị trí file/project reference — ảnh hưởng tới toàn bộ code sau này tham chiếu tới User (Recipe.AuthorId, RefreshToken.UserId, mọi Command/Query có `[Authorize]`...). Đổi sau khi đã có nhiều code phụ thuộc sẽ tốn công sửa lại toàn bộ.

**Không tự chọn — nhưng nếu cần 1 đề xuất tham khảo để hỏi lại người dùng:** đặt `ApplicationUser` trong `CulinaryBlog.Infrastructure`; `Domain` chỉ giữ tham chiếu dạng `AuthorId (Guid)` ở Recipe, không có entity User đầy đủ trong Domain.

**Ai quyết:** cần xác nhận từ người phụ trách module Auth (Thành viên 1) + được ghi lại thành 1 dòng sửa trong `SRS.md` hoặc `docs/architecture/README.md` (vì đây là NFR-MAINT-004, có kế hoạch enforce bằng ArchUnit.NET test — sửa code mà không sửa rule/test sẽ vẫn bị fail).

---

## 2. 🟡 "Category active" không có định nghĩa

**Vấn đề:** điều kiện Publish Recipe (FR-RCP-005) yêu cầu *"Category đang active"*, nhưng bảng `Category` không có cột `IsActive` nào — chỉ có `IsDeleted`.

**Không tự chọn — đề xuất tham khảo:** `active` = `IsDeleted == false`, không thêm cột mới.

**Ai quyết:** người phụ trách module Category/Taxonomy (Thành viên 2).

---

## 3. 🟡 Unarchive trả về trạng thái nào?

**Vấn đề:** endpoint `PATCH /recipes/{id}/unarchive` tồn tại trong đặc tả API nhưng SRS không mô tả logic bên trong — không rõ `Status` sau khi unarchive là `Draft` hay khôi phục đúng trạng thái trước khi archive.

**Không tự chọn — đề xuất tham khảo:** luôn set về `Draft` (an toàn hơn, không cần thêm cột lưu "trạng thái trước đó").

**Ai quyết:** người phụ trách Recipe Lifecycle (Thành viên 3).

---

## 4. 🟢 Retention ảnh sau khi xóa — bao nhiêu ngày?

**Vấn đề:** SRS chỉ ghi *"lập lịch xóa file sau retention phù hợp"* — không có con số cụ thể ở bất kỳ đâu trong tài liệu.

**Không tự chọn — đề xuất tham khảo:** 30 ngày, đồng bộ với `PurgeExpiredRecipesJob` (xem `RESOLVED-CONFLICTS.md` mục A1) để dùng chung 1 background job.

**Ai quyết:** người phụ trách Recipe Composition & Media (Thành viên 4).

---

## Bảng theo dõi (điền khi họp xong, xoá dòng khỏi file này sau khi chốt và chuyển nội dung đã chốt sang `RESOLVED-CONFLICTS.md`)

| # | Câu hỏi | Trạng thái | Phương án đã chọn | Người chốt | Ngày |
|---|---|---|---|---|---|
| 1 | ApplicationUser đặt ở đâu? | ⬜ Chưa chốt | | | |
| 2 | "Category active" là gì? | ⬜ Chưa chốt | | | |
| 3 | Unarchive về trạng thái nào? | ⬜ Chưa chốt | | | |
| 4 | Retention ảnh bao nhiêu ngày? | ⬜ Chưa chốt | | | |
