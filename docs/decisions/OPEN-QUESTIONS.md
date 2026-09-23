# Open Questions — Chưa chốt, KHÔNG tự implement

**Trạng thái:** mục 1 đã chốt ngày 2026-09-23 (chuyển sang `RESOLVED-CONFLICTS.md` mục D7). 3 điểm còn lại (mục 2–4) vẫn còn mơ hồ hoặc tự mâu thuẫn trong `SRS.md` v1.2.0, kể cả sau khi đã áp dụng toàn bộ `RESOLVED-CONFLICTS.md`. Chưa có quyết định chính thức từ nhóm.

**QUY TẮC BẮT BUỘC cho AI coding agent:** nếu 1 task được giao chạm vào bất kỳ mục nào dưới đây (tên file/class/bảng trùng khớp), **dừng lại và hỏi người dùng trước khi viết code**, dù trong mục có ghi "đề xuất" — đề xuất chỉ là gợi ý tham khảo, **không phải quyết định đã chốt**, không được tự ý áp dụng.

---

## 1. ✅ [ĐÃ CHỐT 2026-09-23] `ApplicationUser` đặt ở layer nào

**Quyết định:** đặt `ApplicationUser` trong `CulinaryBlog.Infrastructure/Identity`; Domain không còn `PackageReference` nào, `Recipe`/`RefreshToken` chỉ giữ `AuthorId`/`UserId` (`string`), không có navigation tới User. Không đổi schema DB.

**Chi tiết + tác động kỹ thuật:** xem `RESOLVED-CONFLICTS.md` mục **D7** (mục này giữ lại dạng tóm tắt để tra cứu, không còn là câu hỏi mở).

**Người chốt:** Liêng Hót Ha Luyến (2312682 — phụ trách module Auth).

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
| 1 | ApplicationUser đặt ở đâu? | ✅ Đã chốt | `Infrastructure/Identity`; Domain chỉ giữ `AuthorId`/`UserId` (string), không navigation — RESOLVED-CONFLICTS D7 | Liêng Hót Ha Luyến (2312682) | 2026-09-23 |
| 2 | "Category active" là gì? | ⬜ Chưa chốt | | | |
| 3 | Unarchive về trạng thái nào? | ⬜ Chưa chốt | | | |
| 4 | Retention ảnh bao nhiêu ngày? | ⬜ Chưa chốt | | | |
