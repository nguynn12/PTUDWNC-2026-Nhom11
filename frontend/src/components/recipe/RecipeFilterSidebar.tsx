"use client";

import React, { useState, useEffect } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { useUIStore } from "@/store/useUIStore";
import type { CategoryDto, RecipeDifficulty } from "@/types/api";

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

  // Đồng bộ lại khi query params trên URL thay đổi
  useEffect(() => {
    setCategoryId(searchParams.get("categoryId") || "");
    setDifficulty(searchParams.get("difficulty") || "");
    setMaxTotalTime(searchParams.get("maxTotalTime") || "");
    setMinCalories(searchParams.get("minCalories") || "");
    setMaxCalories(searchParams.get("maxCalories") || "");
  }, [searchParams]);

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
    <form onSubmit={handleApplyFilter} style={{ display: "flex", flexDirection: "column", gap: "24px" }}>
      <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between" }}>
        <h3 style={{ fontSize: "1.15rem", margin: 0, display: "flex", alignItems: "center", gap: "8px" }}>
          <span>🌪️</span> Bộ lọc đa tiêu chí
        </h3>
        {hasActiveFilters && (
          <button
            type="button"
            onClick={handleResetFilter}
            style={{ fontSize: "0.82rem", color: "var(--primary)", fontWeight: 600, background: "none", border: "none", cursor: "pointer" }}
          >
            Đặt lại
          </button>
        )}
      </div>

      {/* 1. Lọc theo danh mục */}
      <div className="input-group">
        <label htmlFor="filter-category" className="label">
          Danh mục món ăn
        </label>
        <select
          id="filter-category"
          value={categoryId}
          onChange={(e) => setCategoryId(e.target.value)}
          className="select"
        >
          <option value="">Tất cả danh mục</option>
          {categories.map((cat) => (
            <option key={cat.id} value={cat.id}>
              {cat.name} ({cat.recipeCount || 0})
            </option>
          ))}
        </select>
      </div>

      {/* 2. Lọc theo độ khó */}
      <div className="input-group">
        <span className="label">Độ khó chế biến</span>
        <div style={{ display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: "6px" }}>
          {[
            { value: "Easy", label: "Dễ" },
            { value: "Medium", label: "Vừa" },
            { value: "Hard", label: "Khó" },
          ].map((item) => {
            const isSelected = difficulty === item.value;
            return (
              <button
                key={item.value}
                type="button"
                onClick={() => setDifficulty(isSelected ? "" : item.value)}
                style={{
                  padding: "8px 6px",
                  borderRadius: "var(--radius-md)",
                  fontSize: "0.85rem",
                  fontWeight: 600,
                  textAlign: "center",
                  border: `1px solid ${isSelected ? "var(--primary)" : "var(--border-default)"}`,
                  backgroundColor: isSelected ? "var(--primary-light)" : "var(--bg-surface)",
                  color: isSelected ? "var(--primary)" : "var(--text-secondary)",
                  cursor: "pointer",
                  transition: "all var(--transition-fast)",
                }}
              >
                {item.label}
              </button>
            );
          })}
        </div>
      </div>

      {/* 3. Lọc theo thời gian nấu tối đa */}
      <div className="input-group">
        <label htmlFor="filter-time" className="label">
          Thời gian tối đa: {maxTotalTime ? `${maxTotalTime} phút` : "Bất kỳ"}
        </label>
        <select
          id="filter-time"
          value={maxTotalTime}
          onChange={(e) => setMaxTotalTime(e.target.value)}
          className="select"
        >
          <option value="">Không giới hạn</option>
          <option value="15">Dưới 15 phút (Cực nhanh)</option>
          <option value="30">Dưới 30 phút (Nhanh)</option>
          <option value="45">Dưới 45 phút</option>
          <option value="60">Dưới 1 tiếng</option>
          <option value="90">Dưới 1 tiếng 30 phút</option>
          <option value="120">Dưới 2 tiếng</option>
        </select>
      </div>

      {/* 4. Lọc theo khoảng calo */}
      <div className="input-group">
        <span className="label">Lượng Calo (kcal)</span>
        <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
          <input
            type="number"
            placeholder="Tối thiểu"
            min={0}
            max={2000}
            value={minCalories}
            onChange={(e) => setMinCalories(e.target.value)}
            className="input"
            style={{ fontSize: "0.88rem", padding: "8px 10px" }}
          />
          <span style={{ color: "var(--text-muted)" }}>-</span>
          <input
            type="number"
            placeholder="Tối đa"
            min={0}
            max={3000}
            value={maxCalories}
            onChange={(e) => setMaxCalories(e.target.value)}
            className="input"
            style={{ fontSize: "0.88rem", padding: "8px 10px" }}
          />
        </div>
      </div>

      {/* Nút hành động */}
      <div style={{ display: "flex", flexDirection: "column", gap: "8px", marginTop: "8px" }}>
        <button type="submit" className="btn btn-primary" style={{ width: "100%" }}>
          Áp dụng bộ lọc
        </button>
        {hasActiveFilters && (
          <button
            type="button"
            onClick={handleResetFilter}
            className="btn btn-outline"
            style={{ width: "100%" }}
          >
            Xóa bộ lọc
          </button>
        )}
      </div>
    </form>
  );

  return (
    <>
      {/* KHỐI SIDEBAR TRÊN MÁY TÍNH (DESKTOP) */}
      <aside
        className="filter-desktop-sidebar"
        style={{
          width: "280px",
          flexShrink: 0,
          backgroundColor: "var(--bg-surface)",
          border: "1px solid var(--border-subtle)",
          borderRadius: "var(--radius-lg)",
          padding: "24px",
          boxShadow: "var(--shadow-xs)",
          height: "fit-content",
          position: "sticky",
          top: "92px",
        }}
      >
        {filterContent}
      </aside>

      {/* KHỐI DRAWER TRƯỢT TRÊN MOBILE KHI BẤM NÚT BỘ LỌC */}
      {isFilterDrawerOpen && (
        <div
          style={{
            position: "fixed",
            inset: 0,
            zIndex: 100,
            display: "flex",
            justifyContent: "flex-end",
          }}
        >
          {/* Lớp nền mờ */}
          <div
            onClick={() => setFilterDrawerOpen(false)}
            style={{
              position: "absolute",
              inset: 0,
              backgroundColor: "var(--bg-overlay)",
              backdropFilter: "blur(4px)",
            }}
          />

          {/* Khung nội dung Drawer */}
          <div
            style={{
              position: "relative",
              width: "min(340px, 90vw)",
              height: "100%",
              backgroundColor: "var(--bg-surface)",
              padding: "28px 24px",
              boxShadow: "var(--shadow-lg)",
              overflowY: "auto",
              zIndex: 1,
            }}
          >
            <div
              style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                marginBottom: "20px",
                paddingBottom: "12px",
                borderBottom: "1px solid var(--border-subtle)",
              }}
            >
              <h3 style={{ fontSize: "1.2rem", margin: 0 }}>Bộ Lọc Công Thức</h3>
              <button
                type="button"
                onClick={() => setFilterDrawerOpen(false)}
                className="btn btn-ghost btn-sm"
                style={{ fontSize: "1.2rem", padding: "4px 8px" }}
              >
                ✕
              </button>
            </div>
            {filterContent}
          </div>
        </div>
      )}

      <style jsx>{`
        @media (max-width: 900px) {
          .filter-desktop-sidebar {
            display: none !important;
          }
        }
      `}</style>
    </>
  );
}
