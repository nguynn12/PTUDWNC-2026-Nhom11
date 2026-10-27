"use client";

import React, { useState, useEffect } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { useSearchRecipes } from "@/hooks/useRecipes";
import { RecipeCard } from "@/components/recipe/RecipeCard";
import { RecipeCardSkeleton } from "@/components/common/Skeleton";
import { Pagination } from "@/components/common/Pagination";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Search, X, Loader2, BookOpen, UtensilsCrossed } from "lucide-react";
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
    <div className="max-w-4xl mx-auto">
      {/* KHỐI THANH TÌM KIẾM CỠ LỚN */}
      <div className="relative mb-6">
        <div className="flex items-center bg-card border-2 border-border focus-within:border-primary focus-within:ring-2 focus-within:ring-primary/20 rounded-2xl px-4 py-2 shadow-xs transition-all duration-200">
          <Search className="w-5 h-5 text-primary shrink-0 mr-3" />

          <input
            type="text"
            placeholder="Tìm theo tên món, nguyên liệu (ví dụ: cá hồi, bò kho, phở bò...)"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="flex-1 bg-transparent text-base sm:text-lg text-foreground placeholder:text-muted-foreground outline-none py-1.5"
            autoFocus
          />

          {searchTerm && (
            <Button
              type="button"
              variant="ghost"
              size="icon"
              onClick={handleClear}
              className="h-8 w-8 text-muted-foreground hover:text-foreground shrink-0 rounded-full"
              aria-label="Xóa từ khóa"
            >
              <X className="w-4 h-4" />
            </Button>
          )}

          {isFetching && (
            <div className="flex items-center gap-1.5 text-xs font-medium text-primary ml-2 shrink-0">
              <Loader2 className="w-3.5 h-3.5 animate-spin" />
              <span className="hidden sm:inline">Đang tìm...</span>
            </div>
          )}
        </div>
      </div>

      {/* TỪ KHÓA GỢI Ý THỊNH HÀNH */}
      <div className="flex items-center gap-2 flex-wrap mb-10">
        <span className="text-xs font-semibold text-muted-foreground">
          Gợi ý nhanh:
        </span>
        {trendingKeywords.map((kw) => {
          const isSelected = searchTerm.toLowerCase() === kw.toLowerCase();
          return (
            <button
              key={kw}
              type="button"
              onClick={() => handleSelectKeyword(kw)}
              className="focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary rounded-full"
            >
              <Badge
                variant={isSelected ? "default" : "secondary"}
                className={`cursor-pointer px-3 py-1 text-xs transition-all ${
                  isSelected
                    ? "bg-primary text-primary-foreground font-semibold shadow-xs"
                    : "hover:bg-primary/10 hover:text-primary"
                }`}
              >
                {kw}
              </Badge>
            </button>
          );
        })}
      </div>

      {/* TRƯỜNG HỢP 1: TỪ KHÓA QUÁ NGẮN (< 2 KÝ TỰ THEO FR-SRCH-001) */}
      {isQueryTooShort && (
        <div className="text-center py-16 px-6 rounded-2xl border-2 border-dashed border-border bg-card">
          <div className="w-16 h-16 rounded-full bg-primary/10 text-primary flex items-center justify-center mx-auto mb-4">
            <BookOpen className="w-8 h-8" />
          </div>
          <h3 className="text-xl font-bold text-foreground mb-2">
            Nhập ít nhất 2 ký tự để tìm kiếm
          </h3>
          <p className="text-muted-foreground text-sm max-w-md mx-auto leading-relaxed">
            Hệ thống hỗ trợ tìm kiếm toàn văn bản tiếng Việt có dấu hoặc không dấu. Hãy nhập tên món ăn hoặc bấm vào các gợi ý phía trên.
          </p>
        </div>
      )}

      {/* TRƯỜNG HỢP 2: ĐANG TẢI DỮ LIỆU */}
      {!isQueryTooShort && isLoading && (
        <div>
          <div className="mb-6">
            <span className="text-sm text-muted-foreground">
              Đang tìm kiếm công thức cho từ khóa &quot;<strong>{debouncedQuery}</strong>&quot;...
            </span>
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
            {Array.from({ length: 6 }).map((_, i) => (
              <RecipeCardSkeleton key={i} />
            ))}
          </div>
        </div>
      )}

      {/* TRƯỜNG HỢP 3: CÓ KẾT QUẢ TÌM KIẾM */}
      {!isQueryTooShort && !isLoading && recipes.length > 0 && (
        <div>
          <div className="flex items-center justify-between mb-6 flex-wrap gap-2">
            <h2 className="text-lg sm:text-xl font-bold text-foreground">
              Tìm thấy <span className="text-primary">{totalCount}</span> công thức cho &quot;{debouncedQuery}&quot;
            </h2>
            <span className="text-xs text-muted-foreground">
              (Đã xếp hạng theo độ liên quan nhất)
            </span>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
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
        <div className="text-center py-16 px-6 rounded-2xl border-2 border-dashed border-border bg-card">
          <div className="w-16 h-16 rounded-full bg-muted text-muted-foreground flex items-center justify-center mx-auto mb-4">
            <UtensilsCrossed className="w-8 h-8" />
          </div>
          <h3 className="text-xl font-bold text-foreground mb-2">
            Không tìm thấy công thức nào cho &quot;{debouncedQuery}&quot;
          </h3>
          <p className="text-muted-foreground text-sm max-w-md mx-auto mb-6 leading-relaxed">
            Mẹo tìm kiếm: Hãy thử kiểm tra lại chính tả, thử từ khóa đơn giản hơn (như &quot;bò&quot;, &quot;cá&quot;, &quot;canh&quot;) hoặc bấm vào các gợi ý món ăn phổ biến.
          </p>
          <Button type="button" variant="outline" onClick={handleClear} className="rounded-xl">
            Thử tìm kiếm khác
          </Button>
        </div>
      )}
    </div>
  );
}
