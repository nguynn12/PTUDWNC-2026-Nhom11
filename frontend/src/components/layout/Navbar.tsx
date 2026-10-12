"use client";

import React from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useUIStore } from "@/store/useUIStore";
import { UserMenu } from "@/components/layout/UserMenu";

export function Navbar() {
  const pathname = usePathname();
  const { isMobileMenuOpen, toggleMobileMenu, setMobileMenuOpen } = useUIStore();

  const navLinks = [
    { href: "/", label: "Trang chủ" },
    { href: "/recipes", label: "Khám phá công thức" },
    { href: "/categories", label: "Danh mục" },
    { href: "/search", label: "Tìm kiếm" },
    { href: "/dashboard/categories", label: "Quản trị Admin" },
  ];

  const isActive = (href: string) => {
    if (href === "/") return pathname === "/";
    return pathname.startsWith(href);
  };

  return (
    <header
      style={{
        position: "sticky",
        top: 0,
        zIndex: 50,
        backgroundColor: "rgba(251, 249, 246, 0.92)",
        backdropFilter: "blur(12px)",
        borderBottom: "1px solid var(--border-subtle)",
        transition: "all var(--transition-base)",
      }}
    >
      <div
        className="container"
        style={{
          height: "var(--header-height)",
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          gap: "20px",
        }}
      >
        {/* LOGO THƯƠNG HIỆU */}
        <Link
          href="/"
          style={{
            display: "flex",
            alignItems: "center",
            gap: "10px",
            textDecoration: "none",
          }}
          onClick={() => setMobileMenuOpen(false)}
        >
          <div
            style={{
              width: "42px",
              height: "42px",
              borderRadius: "var(--radius-md)",
              backgroundColor: "var(--primary)",
              color: "var(--text-inverse)",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              fontSize: "1.3rem",
              fontWeight: 800,
              boxShadow: "0 3px 10px rgba(200, 90, 23, 0.35)",
            }}
          >
            🍳
          </div>
          <div>
            <span
              style={{
                fontSize: "1.3rem",
                fontWeight: 800,
                letterSpacing: "-0.02em",
                color: "var(--text-primary)",
                display: "block",
                lineHeight: 1.1,
              }}
            >
              Culinary<span style={{ color: "var(--primary)" }}>Blog</span>
            </span>
            <span
              style={{
                fontSize: "0.68rem",
                color: "var(--text-muted)",
                letterSpacing: "0.08em",
                textTransform: "uppercase",
                fontWeight: 700,
              }}
            >
              Nghệ thuật ẩm thực
            </span>
          </div>
        </Link>

        {/* MENU ĐIỀU HƯỚNG MÁY TÍNH (DESKTOP) */}
        <nav
          style={{
            display: "none",
            alignItems: "center",
            gap: "8px",
          }}
          className="desktop-nav"
        >
          {navLinks.map((link) => {
            const active = isActive(link.href);
            return (
              <Link
                key={link.href}
                href={link.href}
                style={{
                  padding: "8px 14px",
                  borderRadius: "var(--radius-md)",
                  fontSize: "0.92rem",
                  fontWeight: active ? 700 : 500,
                  color: active ? "var(--primary)" : "var(--text-secondary)",
                  backgroundColor: active ? "var(--primary-light)" : "transparent",
                  transition: "all var(--transition-fast)",
                }}
              >
                {link.label}
              </Link>
            );
          })}
        </nav>

        {/* NÚT THAO TÁC BÊN PHẢI */}
        <div style={{ display: "flex", alignItems: "center", gap: "10px" }}>
          {/* Nút Tìm kiếm nhanh */}
          <Link
            href="/search"
            className="btn btn-outline btn-sm"
            style={{
              display: "inline-flex",
              alignItems: "center",
              gap: "6px",
            }}
          >
            🔍 <span className="search-text">Tìm kiếm</span>
          </Link>

          {/* Tài khoản: nút Đăng nhập hoặc menu người dùng (module Auth) */}
          <UserMenu />

          {/* Nút Hamburger menu Mobile */}
          <button
            type="button"
            onClick={toggleMobileMenu}
            className="btn btn-outline btn-sm mobile-menu-btn"
            style={{
              padding: "8px 10px",
              fontSize: "1.1rem",
            }}
            aria-label="Mở menu di động"
            aria-expanded={isMobileMenuOpen}
          >
            {isMobileMenuOpen ? "✕" : "☰"}
          </button>
        </div>
      </div>

      {/* MENU ĐIỀU HƯỚNG TRÊN MOBILE (DRAWER XUỐNG KHI MỞ) */}
      {isMobileMenuOpen && (
        <div
          style={{
            backgroundColor: "var(--bg-surface)",
            borderBottom: "1px solid var(--border-default)",
            padding: "16px 20px 24px",
            display: "flex",
            flexDirection: "column",
            gap: "8px",
            boxShadow: "var(--shadow-md)",
          }}
        >
          {navLinks.map((link) => {
            const active = isActive(link.href);
            return (
              <Link
                key={link.href}
                href={link.href}
                onClick={() => setMobileMenuOpen(false)}
                style={{
                  padding: "12px 16px",
                  borderRadius: "var(--radius-md)",
                  fontSize: "1rem",
                  fontWeight: active ? 700 : 500,
                  color: active ? "var(--primary)" : "var(--text-primary)",
                  backgroundColor: active ? "var(--primary-light)" : "transparent",
                }}
              >
                {link.label}
              </Link>
            );
          })}
        </div>
      )}
    </header>
  );
}
