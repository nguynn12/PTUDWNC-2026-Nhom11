"use client";

import React, { useState } from "react";
import { useRouter } from "next/navigation";
import { Search, X } from "lucide-react";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";

export interface HeaderSearchProps {
  className?: string;
  inputClassName?: string;
  placeholder?: string;
  onSearchComplete?: () => void;
}

/**
 * Component ô tìm kiếm nhanh trên Header (Lab 7 - TV3).
 * Cho phép nhập từ khóa và nhấn Enter để chuyển trực tiếp đến trang tìm kiếm /search?q=...
 */
export function HeaderSearch({
  className,
  inputClassName,
  placeholder = "Tìm món ăn, nguyên liệu...",
  onSearchComplete,
}: HeaderSearchProps) {
  const router = useRouter();
  const [query, setQuery] = useState("");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const trimmed = query.trim();
    if (trimmed) {
      router.push(`/search?q=${encodeURIComponent(trimmed)}`);
    } else {
      router.push("/search");
    }
    setQuery("");
    onSearchComplete?.();
  };

  const handleClear = () => {
    setQuery("");
  };

  return (
    <form
      onSubmit={handleSubmit}
      className={cn("relative flex items-center w-full", className)}
      role="search"
    >
      <Search
        className="absolute left-3 h-4 w-4 text-muted-foreground pointer-events-none"
        aria-hidden="true"
      />
      <Input
        type="search"
        placeholder={placeholder}
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        className={cn(
          "h-9 w-full rounded-full pl-9 pr-8 text-xs bg-muted/50 border-border/80 focus:bg-background focus:ring-2 focus:ring-primary/20 transition-all",
          inputClassName
        )}
      />
      {query && (
        <button
          type="button"
          onClick={handleClear}
          className="absolute right-2.5 text-muted-foreground hover:text-foreground p-0.5 rounded-full transition-colors"
          aria-label="Xóa từ khóa"
        >
          <X className="h-3 w-3" />
        </button>
      )}
    </form>
  );
}
