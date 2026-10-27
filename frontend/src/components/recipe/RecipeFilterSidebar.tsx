"use client";

import React, { useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { useUIStore } from "@/store/ui-store";
import type { CategoryDto } from "@/types/api";
import { Select } from "@/components/ui/select";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";

interface RecipeFilterSidebarProps {
  categories: CategoryDto[];
}

export function RecipeFilterSidebar({ categories }: RecipeFilterSidebarProps) {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { isFilterDrawerOpen, setFilterDrawerOpen } = useUIStore();

  // Local state của form bộ lọc
  const [categoryId, setCategoryId] = useState(searchParams.get("categoryId") || "");
  const [difficulty, setDifficulty] = useState<string>(searchParams.get("difficulty") || "");
  const [maxTotalTime, setMaxTotalTime] = useState(searchParams.get("maxTotalTime") || "");
  const [minCalories, setMinCalories] = useState(searchParams.get("minCalories") || "");
  const [maxCalories, setMaxCalories] = useState(searchParams.get("maxCalories") || "");

  // Đồng bộ lại khi query params trên URL thay đổi (React render-time adjustment pattern)
  const [prevParamsStr, setPrevParamsStr] = useState(searchParams.toString());
  if (prevParamsStr !== searchParams.toString()) {
    setPrevParamsStr(searchParams.toString());
    setCategoryId(searchParams.get("categoryId") || "");
    setDifficulty(searchParams.get("difficulty") || "");
    setMaxTotalTime(searchParams.get("maxTotalTime") || "");
    setMinCalories(searchParams.get("minCalories") || "");
    setMaxCalories(searchParams.get("maxCalories") || "");
  }

  // Áp dụng bộ lọc
  const handleApplyFilter = (e?: React.FormEvent) => {
    if (e) e.preventDefault();

    const params = new URLSearchParams(searchParams.toString());
    params.set("page", "1"); // Luôn reset về trang 1 khi lọc

    if (categoryId) params.set("categoryId", categoryId);
    else params.delete("categoryId");

    if (difficulty) params.set("difficulty", difficulty);
    else params.delete("difficulty");

    if (maxTotalTime) params.set("maxTotalTime", maxTotalTime);
    else params.delete("maxTotalTime");

    if (minCalories) params.set("minCalories", minCalories);
    else params.delete("minCalories");

    if (maxCalories) params.set("maxCalories", maxCalories);
    else params.delete("maxCalories");

    setFilterDrawerOpen(false);
    router.push(`/recipes?${params.toString()}`);
  };

  // Đặt lại bộ lọc về mặc định
  const handleResetFilter = () => {
    setCategoryId("");
    setDifficulty("");
    setMaxTotalTime("");
    setMinCalories("");
    setMaxCalories("");
    setFilterDrawerOpen(false);

    const params = new URLSearchParams();
    const sortBy = searchParams.get("sortBy");
    const sortOrder = searchParams.get("sortOrder");
    if (sortBy) params.set("sortBy", sortBy);
    if (sortOrder) params.set("sortOrder", sortOrder);

    const query = params.toString();
    router.push(`/recipes${query ? `?${query}` : ""}`);
  };

  const hasActiveFilters = Boolean(
    categoryId || difficulty || maxTotalTime || minCalories || maxCalories
  );

  const filterContent = (
    <form onSubmit={handleApplyFilter} className="flex flex-col gap-6">
      {/* Tiêu đề & nút Đặt lại */}
      <div className="flex items-center justify-between pb-3 border-b border-[var(--border-subtle)]">
        <h3 className="text-base font-bold text-[var(--text-primary)] m-0 flex items-center gap-2">
          <span>🌪️</span>
          <span>Bộ lọc đa tiêu chí</span>
        </h3>
        {hasActiveFilters && (
          <button
            type="button"
            onClick={handleResetFilter}
            className="text-xs font-bold text-[var(--primary)] hover:text-[var(--primary-hover)] transition-colors cursor-pointer"
          >
            Đặt lại tất cả
          </button>
        )}
      </div>

      {/* 1. Lọc theo danh mục */}
      <div className="space-y-2">
        <label htmlFor="filter-category" className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)]">
          Danh mục món ăn
        </label>
        <Select
          id="filter-category"
          value={categoryId}
          onChange={(e) => setCategoryId(e.target.value)}
        >
          <option value="">Tất cả danh mục</option>
          {categories.map((cat) => (
            <option key={cat.id} value={cat.id}>
              {cat.name} {cat.recipeCount ? `(${cat.recipeCount})` : ""}
            </option>
          ))}
        </Select>
      </div>

      {/* 2. Lọc theo độ khó */}
      <div className="space-y-2">
        <span className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)] block">
          Độ khó chế biến
        </span>
        <div className="grid grid-cols-3 gap-2">
          {[
            { value: "Easy", label: "Dễ", variant: "easy" as const },
            { value: "Medium", label: "Vừa", variant: "medium" as const },
            { value: "Hard", label: "Khó", variant: "hard" as const },
          ].map((item) => {
            const isSelected = difficulty === item.value;
            return (
              <button
                key={item.value}
                type="button"
                onClick={() => setDifficulty(isSelected ? "" : item.value)}
                className={`py-2 px-2 rounded-xl text-xs font-semibold text-center transition-all duration-200 cursor-pointer border ${
                  isSelected
                    ? "border-[var(--primary)] bg-[var(--primary-light)] text-[var(--primary)] shadow-xs"
                    : "border-[var(--border-subtle)] bg-[var(--bg-surface)] text-[var(--text-secondary)] hover:bg-[var(--bg-surface-alt)]"
                }`}
              >
                {item.label}
              </button>
            );
          })}
        </div>
      </div>

      {/* 3. Lọc theo thời gian nấu tối đa */}
      <div className="space-y-2">
        <label htmlFor="filter-time" className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)]">
          Thời gian tối đa: {maxTotalTime ? `${maxTotalTime} phút` : "Bất kỳ"}
        </label>
        <Select
          id="filter-time"
          value={maxTotalTime}
          onChange={(e) => setMaxTotalTime(e.target.value)}
        >
          <option value="">Không giới hạn</option>
          <option value="15">Dưới 15 phút (Cực nhanh)</option>
          <option value="30">Dưới 30 phút (Nhanh)</option>
          <option value="45">Dưới 45 phút</option>
          <option value="60">Dưới 1 tiếng</option>
          <option value="90">Dưới 1 tiếng 30 phút</option>
          <option value="120">Dưới 2 tiếng</option>
        </Select>
      </div>

      {/* 4. Lọc theo khoảng calo */}
      <div className="space-y-2">
        <span className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)] block">
          Lượng Calo (kcal)
        </span>
        <div className="flex items-center gap-2">
          <Input
            type="number"
            placeholder="Tối thiểu"
            min={0}
            max={2000}
            value={minCalories}
            onChange={(e) => setMinCalories(e.target.value)}
            className="text-xs py-1.5"
          />
          <span className="text-[var(--text-muted)] font-medium">-</span>
          <Input
            type="number"
            placeholder="Tối đa"
            min={0}
            max={3000}
            value={maxCalories}
            onChange={(e) => setMaxCalories(e.target.value)}
            className="text-xs py-1.5"
          />
        </div>
      </div>

      {/* Nút hành động */}
      <div className="flex flex-col gap-2.5 pt-2">
        <Button type="submit" variant="default" className="w-full">
          Áp dụng bộ lọc
        </Button>
        {hasActiveFilters && (
          <Button
            type="button"
            variant="outline"
            onClick={handleResetFilter}
            className="w-full"
          >
            Xóa bộ lọc
          </Button>
        )}
      </div>
    </form>
  );

  return (
    <>
      {/* KHỐI SIDEBAR TRÊN MÁY TÍNH (DESKTOP) */}
      <aside className="hidden lg:block w-72 shrink-0 bg-[var(--bg-surface)] border border-[var(--border-subtle)] rounded-2xl p-6 shadow-xs sticky top-24 self-start">
        {filterContent}
      </aside>

      {/* KHỐI DRAWER TRƯỢT TRÊN MOBILE KHI BẤM NÚT BỘ LỌC */}
      {isFilterDrawerOpen && (
        <div className="fixed inset-0 z-50 lg:hidden flex justify-end bg-black/50 backdrop-blur-xs animate-in fade-in duration-200">
          <div
            className="absolute inset-0"
            onClick={() => setFilterDrawerOpen(false)}
            aria-label="Đóng bộ lọc"
          />
          <div className="relative w-full max-w-xs h-full bg-[var(--bg-surface)] p-6 shadow-2xl overflow-y-auto flex flex-col z-10 animate-in slide-in-from-right duration-300">
            <div className="flex items-center justify-between pb-4 mb-4 border-b border-[var(--border-subtle)]">
              <h2 className="text-lg font-bold text-[var(--text-primary)] m-0">Bộ lọc công thức</h2>
              <button
                type="button"
                onClick={() => setFilterDrawerOpen(false)}
                className="w-8 h-8 rounded-full flex items-center justify-center bg-[var(--bg-surface-alt)] text-[var(--text-secondary)] hover:text-[var(--text-primary)] cursor-pointer"
                aria-label="Đóng"
              >
                ✕
              </button>
            </div>
            {filterContent}
          </div>
        </div>
      )}
    </>
  );
}
