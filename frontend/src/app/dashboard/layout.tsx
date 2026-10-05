import type { Metadata } from "next";
import Link from "next/link";

export const metadata: Metadata = {
  title: "Quản trị Hệ thống | Culinary Blog",
  description: "Bảng điều khiển và quản trị phân hệ Culinary Blog dành cho Quản trị viên (Admin).",
  robots: {
    index: false,
    follow: false,
  },
};

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div style={{ minHeight: "100vh", backgroundColor: "var(--bg-default)", display: "flex", flexDirection: "column" }}>
      {/* Admin Top Header */}
      <header className="admin-header">
        <div className="container">
          <div style={{ display: "flex", flexWrap: "wrap", alignItems: "center", justifyContent: "space-between", gap: "16px" }}>
            <div style={{ display: "flex", alignItems: "center", gap: "16px" }}>
              <Link
                href="/"
                style={{
                  display: "flex",
                  alignItems: "center",
                  gap: "8px",
                  fontSize: "1.25rem",
                  fontWeight: 800,
                  color: "var(--primary)",
                  textDecoration: "none",
                }}
              >
                <span>🍳</span>
                <span>Culinary<span style={{ color: "var(--text-primary)" }}>Blog</span></span>
              </Link>
              <div style={{ height: "24px", width: "1px", backgroundColor: "var(--border-default)" }} />
              <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
                <span className="admin-badge">Admin Console</span>
              </div>
            </div>

            {/* Navigation Links */}
            <nav style={{ display: "flex", alignItems: "center", gap: "12px", flexWrap: "wrap" }}>
              <Link
                href="/dashboard/categories"
                className="btn btn-outline btn-sm"
                style={{
                  fontWeight: 600,
                  borderColor: "var(--primary)",
                  color: "var(--primary)",
                  backgroundColor: "var(--primary-light)",
                }}
              >
                📁 Quản lý Danh mục
              </Link>
              <Link
                href="/recipes"
                className="btn btn-ghost btn-sm"
                style={{ fontWeight: 500 }}
              >
                🍲 Xem Công thức
              </Link>
              <Link
                href="/"
                className="btn btn-outline btn-sm"
                style={{ fontWeight: 500 }}
              >
                🏠 Về Trang chủ
              </Link>
            </nav>
          </div>
        </div>
      </header>

      {/* Main Content Area */}
      <main style={{ flex: 1, paddingBottom: "64px" }}>
        {children}
      </main>
    </div>
  );
}
