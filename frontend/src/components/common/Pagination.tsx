"use client";

import React from "react";
import Link from "next/link";
import { usePathname, useSearchParams } from "next/navigation";

interface PaginationProps {
  page: number;
  totalPages: number;
  onPageChange?: (page: number) => void;
}

export function Pagination({
  page,
  totalPages,
  onPageChange,
}: PaginationProps) {
  const pathname = usePathname();
  const searchParams = useSearchParams();

  if (totalPages <= 1) return null;

  const getUrl = (targetPage: number) => {
    const params = new URLSearchParams(searchParams ? searchParams.toString() : "");
    params.set("page", targetPage.toString());
    return `${pathname}?${params.toString()}`;
  };

  // Tính toán dải số trang hiển thị
  const getPageNumbers = () => {
    const pages: (number | string)[] = [];
    const maxVisible = 5;

    if (totalPages <= maxVisible + 2) {
      for (let i = 1; i <= totalPages; i++) pages.push(i);
    } else {
      pages.push(1);
      if (page > 3) pages.push("...");

      const start = Math.max(2, page - 1);
      const end = Math.min(totalPages - 1, page + 1);

      for (let i = start; i <= end; i++) {
        if (!pages.includes(i)) pages.push(i);
      }

      if (page < totalPages - 2) pages.push("...");
      pages.push(totalPages);
    }
    return pages;
  };

  const renderPageItem = (p: number | string, index: number) => {
    if (typeof p === "string") {
      return (
        <span
          key={`ellipsis-${index}`}
          style={{
            display: "inline-flex",
            alignItems: "center",
            justifyContent: "center",
            width: "36px",
            height: "36px",
            color: "var(--text-muted)",
          }}
        >
          ...
        </span>
      );
    }

    const isActive = p === page;
    const style: React.CSSProperties = {
      display: "inline-flex",
      alignItems: "center",
      justifyContent: "center",
      minWidth: "38px",
      height: "38px",
      padding: "0 8px",
      borderRadius: "var(--radius-md)",
      fontWeight: 600,
      fontSize: "0.9rem",
      backgroundColor: isActive ? "var(--primary)" : "var(--bg-surface)",
      color: isActive ? "var(--text-inverse)" : "var(--text-primary)",
      border: `1px solid ${isActive ? "var(--primary)" : "var(--border-default)"}`,
      cursor: isActive ? "default" : "pointer",
      transition: "all var(--transition-fast)",
    };

    if (isActive) {
      return (
        <span key={p} style={style} aria-current="page">
          {p}
        </span>
      );
    }

    if (onPageChange) {
      return (
        <button
          key={p}
          type="button"
          onClick={() => onPageChange(p)}
          style={style}
        >
          {p}
        </button>
      );
    }

    return (
      <Link key={p} href={getUrl(p)} style={style}>
        {p}
      </Link>
    );
  };

  const hasPrev = page > 1;
  const hasNext = page < totalPages;

  return (
    <nav
      aria-label="Điều hướng phân trang"
      style={{
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        gap: "6px",
        marginTop: "40px",
        flexWrap: "wrap",
      }}
    >
      {/* Nút Trước */}
      {onPageChange ? (
        <button
          type="button"
          disabled={!hasPrev}
          onClick={() => onPageChange(page - 1)}
          className="btn btn-outline btn-sm"
          style={{
            height: "38px",
            opacity: hasPrev ? 1 : 0.4,
            cursor: hasPrev ? "pointer" : "not-allowed",
          }}
        >
          ← Trước
        </button>
      ) : hasPrev ? (
        <Link
          href={getUrl(page - 1)}
          className="btn btn-outline btn-sm"
          style={{ height: "38px" }}
        >
          ← Trước
        </Link>
      ) : (
        <span
          className="btn btn-outline btn-sm"
          style={{
            height: "38px",
            opacity: 0.4,
            cursor: "not-allowed",
            display: "inline-flex",
            alignItems: "center",
          }}
        >
          ← Trước
        </span>
      )}

      {/* Danh sách các trang */}
      {getPageNumbers().map((p, idx) => renderPageItem(p, idx))}

      {/* Nút Sau */}
      {onPageChange ? (
        <button
          type="button"
          disabled={!hasNext}
          onClick={() => onPageChange(page + 1)}
          className="btn btn-outline btn-sm"
          style={{
            height: "38px",
            opacity: hasNext ? 1 : 0.4,
            cursor: hasNext ? "pointer" : "not-allowed",
          }}
        >
          Sau →
        </button>
      ) : hasNext ? (
        <Link
          href={getUrl(page + 1)}
          className="btn btn-outline btn-sm"
          style={{ height: "38px" }}
        >
          Sau →
        </Link>
      ) : (
        <span
          className="btn btn-outline btn-sm"
          style={{
            height: "38px",
            opacity: 0.4,
            cursor: "not-allowed",
            display: "inline-flex",
            alignItems: "center",
          }}
        >
          Sau →
        </span>
      )}
    </nav>
  );
}

