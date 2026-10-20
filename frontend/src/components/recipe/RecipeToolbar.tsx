"use client";

import React from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { useUIStore } from "@/store/useUIStore";
import { Select } from "@/components/ui/select";
import { Button } from "@/components/ui/button";

interface RecipeToolbarProps {
  totalCount: number;
  currentSort?: string;
  currentOrder?: string;
}

export function RecipeToolbar({
  totalCount,
  currentSort = "newest",
  currentOrder = "desc",
}: RecipeToolbarProps) {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { setFilterDrawerOpen, recipesViewMode, setRecipesViewMode } = useUIStore();

  const handleSortChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const value = e.target.value;
    const params = new URLSearchParams(searchParams.toString());

    if (value === "newest") {
      params.set("sortBy", "newest");
      params.set("sortOrder", "desc");
    } else if (value === "quickest") {
      params.set("sortBy", "totalTime");
      params.set("sortOrder", "asc");
    } else if (value === "calories") {
      params.set("sortBy", "calories");
      params.set("sortOrder", "asc");
    } else if (value === "title") {
      params.set("sortBy", "title");
      params.set("sortOrder", "asc");
    }

    params.set("page", "1");
    router.push(`/recipes?${params.toString()}`);
  };

  // Xác định option đang active
  const getSelectedSortOption = () => {
    if (currentSort === "totalTime" && currentOrder === "asc") return "quickest";
    if (currentSort === "calories" && currentOrder === "asc") return "calories";
    if (currentSort === "title" && currentOrder === "asc") return "title";
    return "newest";
  };

  return (
    <div className="flex flex-wrap items-center justify-between gap-4 p-4 md:px-5 md:py-3.5 rounded-2xl border border-[var(--border-subtle)] bg-[var(--bg-surface)] shadow-xs mb-6">
      {/* SỐ LƯỢNG KẾT QUẢ & NÚT BỘ LỌC MOBILE */}
      <div className="flex items-center gap-3">
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={() => setFilterDrawerOpen(true)}
          className="lg:hidden inline-flex items-center gap-1.5 font-semibold text-xs rounded-xl"
        >
          <span>🌪️</span>
          <span>Bộ lọc</span>
        </Button>

        <span className="text-sm md:text-base text-[var(--text-secondary)]">
          Tìm thấy <strong className="text-[var(--text-primary)] font-bold">{totalCount}</strong> công thức
        </span>
      </div>

      {/* SẮP XẾP & CHẾ ĐỘ XEM */}
      <div className="flex items-center gap-3 sm:gap-4 ml-auto">
        <div className="flex items-center gap-2">
          <label htmlFor="sort-select" className="text-xs sm:text-sm text-[var(--text-muted)] whitespace-nowrap hidden sm:inline">
            Sắp xếp:
          </label>
          <div className="w-[155px] sm:w-[175px]">
            <Select
              id="sort-select"
              value={getSelectedSortOption()}
              onChange={handleSortChange}
              className="h-9 text-xs sm:text-sm rounded-xl py-1 px-3"
            >
              <option value="newest">Mới nhất</option>
              <option value="quickest">Nấu nhanh nhất</option>
              <option value="calories">Calo thấp nhất</option>
              <option value="title">Tên món ăn (A-Z)</option>
            </Select>
          </div>
        </div>

        {/* NÚT CHUYỂN GRID / LIST */}
        <div className="inline-flex items-center bg-[var(--bg-surface-alt)] p-1 rounded-xl border border-[var(--border-subtle)]">
          <button
            type="button"
            onClick={() => setRecipesViewMode("grid")}
            className={`px-2.5 py-1 rounded-lg text-sm transition-all duration-200 cursor-pointer ${
              recipesViewMode === "grid"
                ? "bg-[var(--bg-surface)] text-[var(--primary)] shadow-xs font-bold"
                : "text-[var(--text-muted)] hover:text-[var(--text-primary)]"
            }`}
            title="Xem dạng lưới"
            aria-label="Xem dạng lưới"
          >
            ⊞
          </button>
          <button
            type="button"
            onClick={() => setRecipesViewMode("list")}
            className={`px-2.5 py-1 rounded-lg text-sm transition-all duration-200 cursor-pointer ${
              recipesViewMode === "list"
                ? "bg-[var(--bg-surface)] text-[var(--primary)] shadow-xs font-bold"
                : "text-[var(--text-muted)] hover:text-[var(--text-primary)]"
            }`}
            title="Xem dạng danh sách"
            aria-label="Xem dạng danh sách"
          >
            ☰
          </button>
        </div>
      </div>
    </div>
  );
}
