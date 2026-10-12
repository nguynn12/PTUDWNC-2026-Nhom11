import React from "react";
import Link from "next/link";

export function Footer() {
  return (
    <footer
      style={{
        backgroundColor: "var(--bg-surface)",
        borderTop: "1px solid var(--border-subtle)",
        paddingTop: "48px",
        paddingBottom: "32px",
        marginTop: "auto",
      }}
    >
      <div className="container">
        <div
          style={{
            display: "grid",
            gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))",
            gap: "36px",
            marginBottom: "36px",
          }}
        >
          {/* CỘT 1: THÔNG TIN THƯƠNG HIỆU */}
          <div style={{ display: "flex", flexDirection: "column", gap: "12px" }}>
            <div style={{ display: "flex", alignItems: "center", gap: "10px" }}>
              <span style={{ fontSize: "1.5rem" }}>🍳</span>
              <span
                style={{
                  fontSize: "1.2rem",
                  fontWeight: 800,
                  letterSpacing: "-0.02em",
                  color: "var(--text-primary)",
                }}
              >
                Culinary<span style={{ color: "var(--primary)" }}>Blog</span>
              </span>
            </div>
            <p style={{ fontSize: "0.88rem", color: "var(--text-secondary)", lineHeight: 1.6 }}>
              Không gian chia sẻ và khám phá nghệ thuật ẩm thực. Nơi lưu giữ những công thức nấu ăn chuẩn vị và truyền cảm hứng vào bếp mỗi ngày.
            </p>
          </div>

          {/* CỘT 2: ĐIỀU HƯỚNG NHANH */}
          <div style={{ display: "flex", flexDirection: "column", gap: "10px" }}>
            <h4 style={{ fontSize: "0.95rem", color: "var(--text-primary)", fontWeight: 700 }}>
              Khám Phá
            </h4>
            <Link href="/recipes" style={{ fontSize: "0.88rem", color: "var(--text-secondary)" }}>
              Duyệt công thức mới
            </Link>
            <Link href="/categories" style={{ fontSize: "0.88rem", color: "var(--text-secondary)" }}>
              Danh mục món ăn
            </Link>
            <Link href="/search" style={{ fontSize: "0.88rem", color: "var(--text-secondary)" }}>
              Tìm kiếm toàn văn bản
            </Link>
          </div>

          {/* CỘT 3: QUẢN TRỊ & HẠ TẦNG */}
          <div style={{ display: "flex", flexDirection: "column", gap: "10px" }}>
            <h4 style={{ fontSize: "0.95rem", color: "var(--text-primary)", fontWeight: 700 }}>
              Quản Trị & Hệ Thống
            </h4>
            <Link href="/dashboard/categories" style={{ fontSize: "0.88rem", color: "var(--text-secondary)" }}>
              Quản trị Danh mục (Admin)
            </Link>
            <a
              href="http://localhost:5000/scalar/v1"
              target="_blank"
              rel="noreferrer"
              style={{ fontSize: "0.88rem", color: "var(--primary)", fontWeight: 600 }}
            >
              Scalar API Reference ↗
            </a>
            <a
              href="http://localhost:5000/health"
              target="_blank"
              rel="noreferrer"
              style={{ fontSize: "0.88rem", color: "var(--secondary)", fontWeight: 600 }}
            >
              Kiểm tra Health Checks ↗
            </a>
          </div>
        </div>

        {/* PHẦN CUỐI CHÂN TRANG */}
        <div
          style={{
            paddingTop: "24px",
            borderTop: "1px solid var(--border-subtle)",
            display: "flex",
            alignItems: "center",
            justifyContent: "space-between",
            flexWrap: "wrap",
            gap: "12px",
            fontSize: "0.82rem",
            color: "var(--text-muted)",
          }}
        >
          <div>
            © 2026 <strong>Culinary Blog</strong>. Đồ án môn học Phát triển ứng dụng Web nâng cao (Nhóm 11).
          </div>
          <div>
            Thực hiện bởi: <strong>Trần Quốc Quân</strong> (MSSV: 2312726 — Thành viên 2)
          </div>
        </div>
      </div>
    </footer>
  );
}
