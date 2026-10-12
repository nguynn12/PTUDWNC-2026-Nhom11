import React from "react";
import Link from "next/link";

export default function NotFound() {
  return (
    <div className="container section" style={{ textAlign: "center", padding: "80px 20px" }}>
      <div
        style={{
          maxWidth: "540px",
          margin: "0 auto",
          backgroundColor: "var(--bg-surface)",
          border: "1px solid var(--border-subtle)",
          borderRadius: "var(--radius-xl)",
          padding: "48px 32px",
          boxShadow: "var(--shadow-md)",
        }}
      >
        <div style={{ fontSize: "4.5rem", marginBottom: "16px" }}>🍲</div>
        <span className="eyebrow" style={{ color: "var(--text-muted)" }}>
          Lỗi 404 — Không tìm thấy trang
        </span>
        <h1 style={{ fontSize: "2rem", margin: "12px 0 16px" }}>
          Món Ăn Này Chưa Sẵn Sàng!
        </h1>
        <p style={{ color: "var(--text-secondary)", lineHeight: 1.6, marginBottom: "28px" }}>
          Đường dẫn công thức hoặc danh mục bạn đang tìm kiếm có thể đã được đổi tên, chuyển đi hoặc không tồn tại trong hệ thống của chúng tôi.
        </p>

        <div style={{ display: "flex", gap: "12px", justifyContent: "center", flexWrap: "wrap" }}>
          <Link href="/" className="btn btn-primary">
            🏠 Về trang chủ
          </Link>
          <Link href="/recipes" className="btn btn-outline">
            🍳 Khám phá công thức khác
          </Link>
        </div>
      </div>
    </div>
  );
}
