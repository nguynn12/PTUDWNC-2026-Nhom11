"use client";

import React from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { useUIStore } from "@/store/useUIStore";

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
    <div
      style={{
        display: "flex",
        alignItems: "center",
        justifyContent: "space-between",
        gap: "16px",
        padding: "14px 20px",
        backgroundColor: "var(--bg-surface)",
        border: "1px solid var(--border-subtle)",
        borderRadius: "var(--radius-lg)",
        boxShadow: "var(--shadow-xs)",
        marginBottom: "24px",
        flexWrap: "wrap",
      }}
    >
      {/* SỐ LƯỢNG KẾT QUẢ & NÚT BỘ LỌC MOBILE */}
      <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
        <button
          type="button"
          onClick={() => setFilterDrawerOpen(true)}
          className="btn btn-outline btn-sm mobile-filter-btn"
          style={{ display: "inline-flex", alignItems: "center", gap: "6px" }}
        >
          <span>🌪️</span> Bộ lọc
        </button>

        <span style={{ fontSize: "0.95rem", color: "var(--text-secondary)" }}>
          Tìm thấy <strong style={{ color: "var(--text-primary)" }}>{totalCount}</strong> công thức
        </span>
      </div>

      {/* SẮP XẾP & CHẾ ĐỘ XEM */}
      <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
        <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
          <label htmlFor="sort-select" style={{ fontSize: "0.85rem", color: "var(--text-muted)", whiteSpace: "nowrap" }}>
            Sắp xếp:
          </label>
          <select
            id="sort-select"
            value={getSelectedSortOption()}
            onChange={handleSortChange}
            className="select"
            style={{
              padding: "6px 12px",
              fontSize: "0.88rem",
              width: "auto",
              minWidth: "160px",
            }}
          >
            <option value="newest">Mới nhất</option>
            <option value="quickest">Nấu nhanh nhất</option>
            <option value="calories">Calo thấp nhất</option>
            <option value="title">Tên món ăn (A-Z)</option>
          </select>
        </div>

        {/* NÚT CHUYỂN GRID / LIST */}
        <div
          style={{
            display: "inline-flex",
            backgroundColor: "var(--bg-surface-alt)",
            padding: "3px",
            borderRadius: "var(--radius-md)",
            border: "1px solid var(--border-subtle)",
          }}
        >
          <button
            type="button"
            onClick={() => setRecipesViewMode("grid")}
            style={{
              padding: "4px 8px",
              borderRadius: "var(--radius-sm)",
              backgroundColor: recipesViewMode === "grid" ? "var(--bg-surface)" : "transparent",
              color: recipesViewMode === "grid" ? "var(--primary)" : "var(--text-muted)",
              boxShadow: recipesViewMode === "grid" ? "var(--shadow-xs)" : "none",
              border: "none",
              cursor: "pointer",
              fontSize: "0.95rem",
            }}
            title="Xem dạng lưới"
            aria-label="Xem dạng lưới"
          >
            ⊞
          </button>
          <button
            type="button"
            onClick={() => setRecipesViewMode("list")}
            style={{
              padding: "4px 8px",
              borderRadius: "var(--radius-sm)",
              backgroundColor: recipesViewMode === "list" ? "var(--bg-surface)" : "transparent",
              color: recipesViewMode === "list" ? "var(--primary)" : "var(--text-muted)",
              boxShadow: recipesViewMode === "list" ? "var(--shadow-xs)" : "none",
              border: "none",
              cursor: "pointer",
              fontSize: "0.95rem",
            }}
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
