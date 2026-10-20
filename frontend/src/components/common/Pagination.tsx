"use client";

import React from "react";
import Link from "next/link";
import { usePathname, useSearchParams } from "next/navigation";
import { ChevronLeft, ChevronRight, MoreHorizontal } from "lucide-react";
import { cn } from "@/lib/utils";

export interface PaginationProps {
  page?: number;
  currentPage?: number;
  totalPages: number;
  onPageChange?: (page: number) => void;
  className?: string;
  pageSize?: number;
  totalItems?: number;
}

export function Pagination({
  page: pageProp,
  currentPage: currentPageProp,
  totalPages,
  onPageChange,
  className,
}: PaginationProps) {
  const page = pageProp ?? currentPageProp ?? 1;
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
          className="flex h-9 w-9 items-center justify-center text-muted-foreground"
          aria-hidden="true"
        >
          <MoreHorizontal className="h-4 w-4" />
        </span>
      );
    }

    const isActive = p === page;
    const baseClass = cn(
      "inline-flex h-9 min-w-9 items-center justify-center rounded-lg px-3 text-sm font-medium transition-colors",
      isActive
        ? "bg-primary text-primary-foreground shadow-sm pointer-events-none"
        : "text-foreground hover:bg-muted hover:text-foreground border border-transparent hover:border-border"
    );

    if (isActive) {
      return (
        <span key={p} className={baseClass} aria-current="page">
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
          className={baseClass}
        >
          {p}
        </button>
      );
    }

    return (
      <Link key={p} href={getUrl(p)} className={baseClass}>
        {p}
      </Link>
    );
  };

  const hasPrev = page > 1;
  const hasNext = page < totalPages;

  const btnNavClass =
    "inline-flex h-9 items-center justify-center gap-1.5 rounded-lg border border-input bg-background px-3 text-sm font-medium transition-colors hover:bg-muted hover:text-foreground";
  const btnDisabledClass =
    "inline-flex h-9 items-center justify-center gap-1.5 rounded-lg border border-border/50 bg-background/50 px-3 text-sm font-medium text-muted-foreground opacity-50 cursor-not-allowed";

  return (
    <nav
      aria-label="Điều hướng phân trang"
      className={cn("flex items-center justify-center gap-1.5 pt-8 pb-4 flex-wrap", className)}
    >
      {/* Nút Trước */}
      {onPageChange ? (
        <button
          type="button"
          disabled={!hasPrev}
          onClick={() => onPageChange(page - 1)}
          className={hasPrev ? btnNavClass : btnDisabledClass}
          aria-label="Về trang trước"
        >
          <ChevronLeft className="h-4 w-4" />
          <span className="hidden sm:inline">Trước</span>
        </button>
      ) : hasPrev ? (
        <Link
          href={getUrl(page - 1)}
          className={btnNavClass}
          aria-label="Về trang trước"
        >
          <ChevronLeft className="h-4 w-4" />
          <span className="hidden sm:inline">Trước</span>
        </Link>
      ) : (
        <span className={btnDisabledClass} aria-hidden="true">
          <ChevronLeft className="h-4 w-4" />
          <span className="hidden sm:inline">Trước</span>
        </span>
      )}

      {/* Danh sách các số trang */}
      <div className="flex items-center gap-1">
        {getPageNumbers().map((p, idx) => renderPageItem(p, idx))}
      </div>

      {/* Nút Sau */}
      {onPageChange ? (
        <button
          type="button"
          disabled={!hasNext}
          onClick={() => onPageChange(page + 1)}
          className={hasNext ? btnNavClass : btnDisabledClass}
          aria-label="Tới trang sau"
        >
          <span className="hidden sm:inline">Sau</span>
          <ChevronRight className="h-4 w-4" />
        </button>
      ) : hasNext ? (
        <Link
          href={getUrl(page + 1)}
          className={btnNavClass}
          aria-label="Tới trang sau"
        >
          <span className="hidden sm:inline">Sau</span>
          <ChevronRight className="h-4 w-4" />
        </Link>
      ) : (
        <span className={btnDisabledClass} aria-hidden="true">
          <span className="hidden sm:inline">Sau</span>
          <ChevronRight className="h-4 w-4" />
        </span>
      )}
    </nav>
  );
}
