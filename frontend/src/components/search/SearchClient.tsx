"use client";

import React, { useState, useEffect } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { useSearchRecipes } from "@/hooks/useRecipes";
import { RecipeCard } from "@/components/recipe/RecipeCard";
import { RecipeCardSkeleton } from "@/components/common/Skeleton";
import { Pagination } from "@/components/common/Pagination";
import type { RecipeSummaryDto } from "@/types/api";

const trendingKeywords = [
  "Phở bò",
  "Cá hồi",
  "Bò kho",
  "Salad",
  "Món chay",
  "Súp bí đỏ",
  "Tráng miệng",
  "Gà nướng",
];

export function SearchClient() {
  const router = useRouter();
  const searchParams = useSearchParams();

  // Khởi tạo từ khóa từ URL
  const initialQ = searchParams.get("q") || "";
  const [searchTerm, setSearchTerm] = useState(initialQ);
  const [debouncedQuery, setDebouncedQuery] = useState(initialQ);
  const [page, setPage] = useState(1);

  // Cơ chế Debounce 300ms
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedQuery(searchTerm.trim());
      setPage(1);

      // Cập nhật query param lên URL
      if (searchTerm.trim()) {
        router.replace(`/search?q=${encodeURIComponent(searchTerm.trim())}`, {
          scroll: false,
        });
      } else {
        router.replace("/search", { scroll: false });
      }
    }, 300);

    return () => clearTimeout(timer);
  }, [searchTerm, router]);

  // Gọi hook tìm kiếm FTS với TanStack Query
  const { data, isLoading, isFetching } = useSearchRecipes({
    q: debouncedQuery,
    page,
    pageSize: 12,
  });

  const recipes = data?.data || [];
  const totalCount = data?.meta?.totalCount || recipes.length;
  const totalPages = data?.meta?.totalPages || 1;
  const isQueryTooShort = debouncedQuery.length < 2;

  const handleSelectKeyword = (kw: string) => {
    setSearchTerm(kw);
  };

  const handleClear = () => {
    setSearchTerm("");
    setDebouncedQuery("");
  };

  return (
    <div style={{ maxWidth: "1000px", margin: "0 auto" }}>
      {/* KHỐI THANH TÌM KIẾM CỠ LỚN */}
      <div
        style={{
          position: "relative",
          marginBottom: "28px",
        }}
      >
        <div
          style={{
            display: "flex",
            alignItems: "center",
            backgroundColor: "var(--bg-surface)",
            border: "2px solid var(--border-default)",
            borderRadius: "var(--radius-xl)",
            padding: "8px 16px 8px 24px",
            boxShadow: "var(--shadow-sm)",
            transition: "all var(--transition-fast)",
          }}
        >
          <span style={{ fontSize: "1.4rem", marginRight: "12px", color: "var(--primary)" }}>
            🔍
          </span>

          <input
            type="text"
            placeholder="Tìm theo tên món, nguyên liệu (ví dụ: cá hồi, bò kho, phở bò...)"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            style={{
              flex: 1,
              border: "none",
              background: "transparent",
              fontSize: "1.1rem",
              color: "var(--text-primary)",
              outline: "none",
              padding: "8px 0",
            }}
            autoFocus
          />

          {searchTerm && (
            <button
              type="button"
              onClick={handleClear}
              className="btn btn-ghost btn-sm"
              style={{
                fontSize: "1.1rem",
                color: "var(--text-muted)",
                padding: "6px 10px",
              }}
              aria-label="Xóa từ khóa"
            >
              ✕
            </button>
          )}

          {isFetching && (
            <span
              style={{
                fontSize: "0.85rem",
                color: "var(--primary)",
                fontWeight: 600,
                marginLeft: "8px",
              }}
            >
              Đang tìm...
            </span>
          )}
        </div>
      </div>

      {/* TỪ KHÓA GỢI Ý THỊNH HÀNH */}
      <div
        style={{
          display: "flex",
          alignItems: "center",
          gap: "8px",
          flexWrap: "wrap",
          marginBottom: "40px",
        }}
      >
        <span style={{ fontSize: "0.85rem", fontWeight: 700, color: "var(--text-muted)" }}>
          Gợi ý nhanh:
        </span>
        {trendingKeywords.map((kw) => (
          <button
            key={kw}
            type="button"
            onClick={() => handleSelectKeyword(kw)}
            style={{
              padding: "5px 12px",
              borderRadius: "var(--radius-full)",
              fontSize: "0.85rem",
              fontWeight: 500,
              backgroundColor:
                searchTerm.toLowerCase() === kw.toLowerCase()
                  ? "var(--primary-light)"
                  : "var(--bg-surface)",
              color:
                searchTerm.toLowerCase() === kw.toLowerCase()
                  ? "var(--primary)"
                  : "var(--text-secondary)",
              border: `1px solid ${
                searchTerm.toLowerCase() === kw.toLowerCase()
                  ? "var(--primary)"
                  : "var(--border-subtle)"
              }`,
              cursor: "pointer",
              transition: "all var(--transition-fast)",
            }}
          >
            {kw}
          </button>
        ))}
      </div>

      {/* TRƯỜNG HỢP 1: TỪ KHÓA QUÁ NGẮN (< 2 KÝ TỰ THEO FR-SRCH-001) */}
      {isQueryTooShort && (
        <div
          style={{
            textAlign: "center",
            padding: "64px 20px",
            backgroundColor: "var(--bg-surface)",
            borderRadius: "var(--radius-lg)",
            border: "1px dashed var(--border-default)",
          }}
        >
          <div style={{ fontSize: "3.2rem", marginBottom: "16px" }}>📖</div>
          <h3 style={{ fontSize: "1.35rem", marginBottom: "8px", color: "var(--text-primary)" }}>
            Nhập ít nhất 2 ký tự để tìm kiếm
          </h3>
          <p style={{ color: "var(--text-muted)", maxWidth: "500px", margin: "0 auto" }}>
            Hệ thống hỗ trợ tìm kiếm toàn văn bản tiếng Việt có dấu hoặc không dấu. Hãy nhập tên món ăn hoặc bấm vào các gợi ý phía trên.
          </p>
        </div>
      )}

      {/* TRƯỜNG HỢP 2: ĐANG TẢI DỮ LIỆU */}
      {!isQueryTooShort && isLoading && (
        <div>
          <div style={{ marginBottom: "20px" }}>
            <span style={{ fontSize: "0.95rem", color: "var(--text-muted)" }}>
              Đang tìm kiếm công thức cho từ khóa &quot;<strong>{debouncedQuery}</strong>&quot;...
            </span>
          </div>
          <div className="recipe-grid">
            {Array.from({ length: 6 }).map((_, i) => (
              <RecipeCardSkeleton key={i} />
            ))}
          </div>
        </div>
      )}

      {/* TRƯỜNG HỢP 3: CÓ KẾT QUẢ TÌM KIẾM */}
      {!isQueryTooShort && !isLoading && recipes.length > 0 && (
        <div>
          <div
            style={{
              display: "flex",
              alignItems: "center",
              justifyContent: "space-between",
              marginBottom: "24px",
              flexWrap: "wrap",
              gap: "8px",
            }}
          >
            <h2 style={{ fontSize: "1.25rem", margin: 0, fontWeight: 700 }}>
              Tìm thấy <span style={{ color: "var(--primary)" }}>{totalCount}</span> công thức cho &quot;
              {debouncedQuery}&quot;
            </h2>
            <span style={{ fontSize: "0.82rem", color: "var(--text-muted)" }}>
              (Đã xếp hạng theo độ liên quan nhất)
            </span>
          </div>

          <div className="recipe-grid">
            {recipes.map((recipe: RecipeSummaryDto, idx: number) => (
              <RecipeCard key={recipe.id || recipe.slug} recipe={recipe} priority={idx < 3} />
            ))}
          </div>

          {/* Phân trang */}
          <Pagination page={page} totalPages={totalPages} onPageChange={(p) => setPage(p)} />
        </div>
      )}

      {/* TRƯỜNG HỢP 4: KHÔNG TÌM THẤY KẾT QUẢ */}
      {!isQueryTooShort && !isLoading && recipes.length === 0 && (
        <div
          style={{
            textAlign: "center",
            padding: "64px 20px",
            backgroundColor: "var(--bg-surface)",
            borderRadius: "var(--radius-lg)",
            border: "1px dashed var(--border-default)",
          }}
        >
          <div style={{ fontSize: "3rem", marginBottom: "16px" }}>🍳</div>
          <h3 style={{ fontSize: "1.3rem", marginBottom: "8px", color: "var(--text-primary)" }}>
            Không tìm thấy công thức nào cho &quot;{debouncedQuery}&quot;
          </h3>
          <p style={{ color: "var(--text-muted)", maxWidth: "480px", margin: "0 auto 20px", lineHeight: 1.6 }}>
            Mẹo tìm kiếm: Hãy thử kiểm tra lại chính tả, thử từ khóa đơn giản hơn (như &quot;bò&quot;, &quot;cá&quot;, &quot;canh&quot;) hoặc bấm vào các gợi ý món ăn phổ biến.
          </p>
          <button type="button" onClick={handleClear} className="btn btn-outline">
            Thử tìm kiếm khác
          </button>
        </div>
      )}
    </div>
  );
}
