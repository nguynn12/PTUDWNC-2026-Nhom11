import React, { Suspense } from "react";
import type { Metadata } from "next";
import Link from "next/link";
import { SearchClient } from "@/components/search/SearchClient";

export const metadata: Metadata = {
  title: "Tìm Kiếm Công Thức Nấu Ăn",
  description:
    "Tìm kiếm công thức nấu ăn thông minh với PostgreSQL Full-Text Search. Tra cứu theo tên món ăn, nguyên liệu với khả năng tìm kiếm tiếng Việt có dấu và không dấu tại Culinary Blog.",
  openGraph: {
    title: "Tìm Kiếm Công Thức Nấu Ăn | Culinary Blog",
    description: "Công cụ tìm kiếm công thức nấu ăn thông minh và nhanh chóng.",
  },
};

export default function SearchPage() {
  return (
    <div className="container section">
      {/* Breadcrumb */}
      <nav
        aria-label="Breadcrumb"
        style={{
          display: "flex",
          alignItems: "center",
          gap: "8px",
          fontSize: "0.88rem",
          color: "var(--text-muted)",
          marginBottom: "20px",
        }}
      >
        <Link href="/" style={{ color: "var(--text-secondary)" }}>Trang chủ</Link>
        <span>/</span>
        <span style={{ color: "var(--primary)", fontWeight: 600 }}>Tìm kiếm</span>
      </nav>

      {/* Header trang */}
      <div style={{ textAlign: "center", maxWidth: "680px", margin: "0 auto 36px" }}>
        <span className="eyebrow">Tra cứu thông minh</span>
        <h1 style={{ fontSize: "clamp(2rem, 3.5vw + 1rem, 2.75rem)", marginBottom: "10px" }}>
          Tìm Kiếm Công Thức Ẩm Thực
        </h1>
        <p style={{ fontSize: "1.05rem", color: "var(--text-secondary)" }}>
          Nhập từ khóa tiếng Việt (có dấu hoặc không dấu) để tìm kiếm các món ngon chuẩn vị cùng hướng dẫn chi tiết.
        </p>
      </div>

      {/* Nội dung tìm kiếm Client bọc Suspense */}
      <Suspense
        fallback={
          <div style={{ textAlign: "center", padding: "40px" }}>
            <span style={{ color: "var(--text-muted)" }}>Đang khởi tạo công cụ tìm kiếm...</span>
          </div>
        }
      >
        <SearchClient />
      </Suspense>
    </div>
  );
}
