"use client";

import React, { useState, useEffect } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { Search, X } from "lucide-react";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";

export interface HeaderSearchProps {
  className?: string;
  placeholder?: string;
  onSearchSubmit?: () => void;
}

/**
 * Component HeaderSearch phục vụ ô tìm kiếm nhanh trên thanh điều hướng chung (Navbar).
 * 
 * PHÂN CÔNG PHỐI HỢP NHÓM (Buổi 7):
 * - Thành viên 2 (Quân): Thiết lập khung (Stub/Interface) và nhúng an toàn vào Navbar (Desktop & Mobile).
 * - Thành viên 3 (Nguyên): Tiếp nhận mở rộng logic chuyên sâu (gợi ý tự động Autocomplete, debounce, lịch sử tìm kiếm).
 */
export function HeaderSearch({
  className,
  placeholder = "Tìm món ăn, nguyên liệu...",
  onSearchSubmit,
}: HeaderSearchProps) {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [query, setQuery] = useState("");

  // Đồng bộ từ khóa khi người dùng đang ở sẵn trên trang /search?q=...
  useEffect(() => {
    const q = searchParams.get("q");
    if (q) {
      setQuery(q);
    }
  }, [searchParams]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const trimmed = query.trim();

    if (trimmed) {
      router.push(`/search?q=${encodeURIComponent(trimmed)}`);
    } else {
      router.push("/search");
    }

    if (onSearchSubmit) {
      onSearchSubmit();
    }
  };

  const handleClear = () => {
    setQuery("");
  };

  return (
    <form
      onSubmit={handleSubmit}
      className={cn("relative flex items-center", className)}
      role="search"
    >
      {/* Icon kính lúp */}
      <Search className="absolute left-3 h-4 w-4 text-muted-foreground pointer-events-none" />

      {/* Ô nhập từ khóa */}
      <Input
        type="search"
        placeholder={placeholder}
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        className="h-9 w-full rounded-full pl-9 pr-8 text-xs bg-muted/50 border-border/80 focus:bg-background focus:ring-2 focus:ring-primary/20 transition-all placeholder:text-muted-foreground"
      />

      {/* Nút xóa nhanh từ khóa khi có nội dung */}
      {query && (
        <button
          type="button"
          onClick={handleClear}
          className="absolute right-2.5 text-muted-foreground hover:text-foreground p-0.5 rounded-full hover:bg-muted transition-colors cursor-pointer"
          aria-label="Xóa từ khóa tìm kiếm"
        >
          <X className="h-3 w-3" />
        </button>
      )}

      {/* TODO (TV3 - Tạ Nhật Nguyên): Bổ sung popup gợi ý tìm kiếm tức thì (Search Autocomplete/Dropdown) nếu cần */}
    </form>
  );
}
