import React from "react";
import Link from "next/link";

interface PaginationProps {
  page: number;
  totalPages: number;
  onPageChange?: (page: number) => void;
  createPageUrl?: (page: number) => string;
}

export function Pagination({
  page,
  totalPages,
  onPageChange,
  createPageUrl,
}: PaginationProps) {
  if (totalPages <= 1) return null;

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

    if (createPageUrl && !isActive) {
      return (
        <Link key={p} href={createPageUrl(p)} style={style}>
          {p}
        </Link>
      );
    }

    return (
      <button
        key={p}
        type="button"
        disabled={isActive}
        onClick={() => onPageChange?.(p)}
        style={style}
        aria-current={isActive ? "page" : undefined}
      >
        {p}
      </button>
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
      {createPageUrl && hasPrev ? (
        <Link
          href={createPageUrl(page - 1)}
          className="btn btn-outline btn-sm"
          style={{ height: "38px" }}
        >
          ← Trước
        </Link>
      ) : (
        <button
          type="button"
          disabled={!hasPrev}
          onClick={() => onPageChange?.(page - 1)}
          className="btn btn-outline btn-sm"
          style={{
            height: "38px",
            opacity: hasPrev ? 1 : 0.4,
            cursor: hasPrev ? "pointer" : "not-allowed",
          }}
        >
          ← Trước
        </button>
      )}

      {/* Danh sách các trang */}
      {getPageNumbers().map((p, idx) => renderPageItem(p, idx))}

      {/* Nút Sau */}
      {createPageUrl && hasNext ? (
        <Link
          href={createPageUrl(page + 1)}
          className="btn btn-outline btn-sm"
          style={{ height: "38px" }}
        >
          Sau →
        </Link>
      ) : (
        <button
          type="button"
          disabled={!hasNext}
          onClick={() => onPageChange?.(page + 1)}
          className="btn btn-outline btn-sm"
          style={{
            height: "38px",
            opacity: hasNext ? 1 : 0.4,
            cursor: hasNext ? "pointer" : "not-allowed",
          }}
        >
          Sau →
        </button>
      )}
    </nav>
  );
}
