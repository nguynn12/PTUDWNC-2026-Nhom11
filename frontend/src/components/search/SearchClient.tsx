"use client";

import React, { useState, useEffect, useTransition } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { useSearchRecipes } from "@/hooks/useRecipes";
import { useCategories } from "@/hooks/useCategories";
import { RecipeCard } from "@/components/recipe/RecipeCard";
import { RecipeCardSkeleton } from "@/components/common/Skeleton";
import { Pagination } from "@/components/common/Pagination";
import { EmptyState } from "@/components/common/EmptyState";
import { ErrorState } from "@/components/common/ErrorState";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Select } from "@/components/ui/select";
import {
  Search,
  X,
  Loader2,
  BookOpen,
  SlidersHorizontal,
  RotateCcw,
} from "lucide-react";
import type { RecipeSummaryDto, RecipeDifficulty } from "@/types/api";

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

const difficultyOptions = [
  { value: "", label: "Tất cả độ khó" },
  { value: "Easy", label: "Dễ" },
  { value: "Medium", label: "Trung bình" },
  { value: "Hard", label: "Khó" },
  { value: "Expert", label: "Chuyên gia" },
];

const timeOptions = [
  { value: "", label: "Tất cả thời gian" },
  { value: "15", label: "Dưới 15 phút" },
  { value: "30", label: "Dưới 30 phút" },
  { value: "60", label: "Dưới 60 phút" },
  { value: "120", label: "Dưới 2 giờ" },
];

const sortOptions = [
  { value: "relevance", label: "Độ liên quan nhất" },
  { value: "newest", label: "Mới nhất" },
  { value: "quickest", label: "Nhanh nhất" },
  { value: "calories", label: "Ít calo nhất" },
];

/**
 * Component SearchClient phục vụ chức năng tìm kiếm toàn văn bản tiếng Việt FTS (Lab 7 - TV3).
 * Tích hợp debounce 300ms, đồng bộ hóa URL parameters, bộ lọc đa chiều (Category, Difficulty, Time),
 * sắp xếp linh hoạt, phân trang và xử lý toàn diện các trạng thái Skeleton, EmptyState, ErrorState.
 */
export function SearchClient() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [, startTransition] = useTransition();

  // Khởi tạo state từ URL search params
  const initialQ = searchParams.get("q") || "";
  const initialCategory = searchParams.get("category") || "";
  const initialDifficulty = searchParams.get("difficulty") || "";
  const initialMaxTime = searchParams.get("maxTime") || "";
  const initialSortBy = (searchParams.get("sortBy") as any) || "relevance";
  const initialPage = Number(searchParams.get("page")) || 1;

  const [searchTerm, setSearchTerm] = useState(initialQ);
  const [debouncedQuery, setDebouncedQuery] = useState(initialQ);
  const [selectedCategory, setSelectedCategory] = useState(initialCategory);
  const [selectedDifficulty, setSelectedDifficulty] = useState(initialDifficulty);
  const [selectedMaxTime, setSelectedMaxTime] = useState(initialMaxTime);
  const [sortBy, setSortBy] = useState(initialSortBy);
  const [page, setPage] = useState(initialPage);
  const [showFilters, setShowFilters] = useState(
    Boolean(initialCategory || initialDifficulty || initialMaxTime || initialSortBy !== "relevance")
  );

  // Lấy danh sách danh mục để phục vụ bộ lọc
  const { data: categories = [] } = useCategories();

  // Cơ chế Debounce 300ms cho ô nhập tìm kiếm
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedQuery(searchTerm.trim());
      setPage(1);
    }, 300);

    return () => clearTimeout(timer);
  }, [searchTerm]);

  // Đồng bộ hóa trạng thái lên URL search query params
  useEffect(() => {
    const params = new URLSearchParams();
    if (debouncedQuery) params.set("q", debouncedQuery);
    if (selectedCategory) params.set("category", selectedCategory);
    if (selectedDifficulty) params.set("difficulty", selectedDifficulty);
    if (selectedMaxTime) params.set("maxTime", selectedMaxTime);
    if (sortBy && sortBy !== "relevance") params.set("sortBy", sortBy);
    if (page > 1) params.set("page", String(page));

    const queryString = params.toString();
    const newUrl = queryString ? `/search?${queryString}` : "/search";

    startTransition(() => {
      router.replace(newUrl, { scroll: false });
    });
  }, [debouncedQuery, selectedCategory, selectedDifficulty, selectedMaxTime, sortBy, page, router]);

  // Hook tìm kiếm Full-Text Search
  const { data, isLoading, isFetching, isError, error, refetch } = useSearchRecipes({
    q: debouncedQuery,
    page,
    pageSize: 12,
    categoryId: selectedCategory || undefined,
    difficulty: (selectedDifficulty as RecipeDifficulty) || undefined,
    maxTotalTime: selectedMaxTime ? Number(selectedMaxTime) : undefined,
    sortBy: sortBy || "relevance",
    sortOrder: sortBy === "relevance" || sortBy === "newest" ? "desc" : "asc",
  });

  const recipes = data?.data || [];
  const totalCount = data?.meta?.totalCount || recipes.length;
  const totalPages = data?.meta?.totalPages || 1;
  const isQueryTooShort = debouncedQuery.length < 2;

  const hasActiveFilters = Boolean(
    selectedCategory || selectedDifficulty || selectedMaxTime || sortBy !== "relevance"
  );

  const handleSelectKeyword = (kw: string) => {
    setSearchTerm(kw);
    setDebouncedQuery(kw);
    setPage(1);
  };

  const handleClearSearch = () => {
    setSearchTerm("");
    setDebouncedQuery("");
    setPage(1);
  };

  const handleResetAllFilters = () => {
    setSelectedCategory("");
    setSelectedDifficulty("");
    setSelectedMaxTime("");
    setSortBy("relevance");
    setPage(1);
  };

  return (
    <div className="max-w-4xl mx-auto">
      {/* 1. THANH TÌM KIẾM CỠ LỚN */}
      <div className="relative mb-4">
        <div className="flex items-center bg-card border-2 border-border focus-within:border-primary focus-within:ring-2 focus-within:ring-primary/20 rounded-2xl px-4 py-2 shadow-xs transition-all duration-200">
          <Search className="w-5 h-5 text-primary shrink-0 mr-3" aria-hidden="true" />

          <input
            type="text"
            placeholder="Tìm theo tên món, nguyên liệu (ví dụ: bò kho, cá hồi, phở bò...)"
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
              onClick={handleClearSearch}
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

      {/* 2. THANH CÔNG CỤ: TỪ KHÓA GỢI Ý & NÚT BẬT BỘ LỌC */}
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3 mb-6">
        {/* Gợi ý nhanh */}
        <div className="flex items-center gap-2 flex-wrap">
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
                className="focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary rounded-full cursor-pointer"
              >
                <Badge
                  variant={isSelected ? "default" : "secondary"}
                  className={`px-3 py-1 text-xs transition-all ${
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

        {/* Nút bật/tắt bộ lọc nâng cao */}
        <Button
          type="button"
          variant={hasActiveFilters ? "default" : "outline"}
          size="sm"
          onClick={() => setShowFilters(!showFilters)}
          className="rounded-full text-xs shrink-0 self-end sm:self-auto gap-1.5"
        >
          <SlidersHorizontal className="w-3.5 h-3.5" />
          <span>Bộ lọc</span>
          {hasActiveFilters && (
            <span className="w-2 h-2 rounded-full bg-primary-foreground shrink-0 ml-0.5" />
          )}
        </Button>
      </div>

      {/* 3. KHỐI BỘ LỌC ĐA CHIỀU (FILTER TOOLBAR) */}
      {showFilters && (
        <div className="p-4 sm:p-5 rounded-2xl border border-border/80 bg-card/60 backdrop-blur-xs mb-8 transition-all animate-in fade-in-50 duration-200">
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3 sm:gap-4">
            {/* Danh mục */}
            <div>
              <label className="block text-xs font-semibold text-muted-foreground mb-1.5">
                Danh mục
              </label>
              <Select
                value={selectedCategory}
                onChange={(e) => {
                  setSelectedCategory(e.target.value);
                  setPage(1);
                }}
              >
                <option value="">Tất cả danh mục</option>
                {categories.map((cat) => (
                  <option key={cat.id} value={cat.id}>
                    {cat.name}
                  </option>
                ))}
              </Select>
            </div>

            {/* Độ khó */}
            <div>
              <label className="block text-xs font-semibold text-muted-foreground mb-1.5">
                Độ khó
              </label>
              <Select
                value={selectedDifficulty}
                onChange={(e) => {
                  setSelectedDifficulty(e.target.value);
                  setPage(1);
                }}
              >
                {difficultyOptions.map((opt) => (
                  <option key={opt.value} value={opt.value}>
                    {opt.label}
                  </option>
                ))}
              </Select>
            </div>

            {/* Thời gian */}
            <div>
              <label className="block text-xs font-semibold text-muted-foreground mb-1.5">
                Thời gian thực hiện
              </label>
              <Select
                value={selectedMaxTime}
                onChange={(e) => {
                  setSelectedMaxTime(e.target.value);
                  setPage(1);
                }}
              >
                {timeOptions.map((opt) => (
                  <option key={opt.value} value={opt.value}>
                    {opt.label}
                  </option>
                ))}
              </Select>
            </div>

            {/* Sắp xếp */}
            <div>
              <label className="block text-xs font-semibold text-muted-foreground mb-1.5">
                Sắp xếp theo
              </label>
              <Select
                value={sortBy}
                onChange={(e) => {
                  setSortBy(e.target.value);
                  setPage(1);
                }}
              >
                {sortOptions.map((opt) => (
                  <option key={opt.value} value={opt.value}>
                    {opt.label}
                  </option>
                ))}
              </Select>
            </div>
          </div>

          {/* Nút đặt lại bộ lọc */}
          {hasActiveFilters && (
            <div className="flex justify-end mt-4 pt-3 border-t border-border/40">
              <Button
                type="button"
                variant="ghost"
                size="sm"
                onClick={handleResetAllFilters}
                className="text-xs text-muted-foreground hover:text-foreground gap-1.5"
              >
                <RotateCcw className="w-3.5 h-3.5" />
                Đặt lại toàn bộ lọc
              </Button>
            </div>
          )}
        </div>
      )}

      {/* 4. TRƯỜNG HỢP 1: TỪ KHÓA QUÁ NGẮN (< 2 KÝ TỰ THEO FR-SRCH-001) */}
      {isQueryTooShort && (
        <div className="text-center py-16 px-6 rounded-2xl border-2 border-dashed border-border bg-card">
          <div className="w-16 h-16 rounded-full bg-primary/10 text-primary flex items-center justify-center mx-auto mb-4">
            <BookOpen className="w-8 h-8" />
          </div>
          <h3 className="text-xl font-bold text-foreground mb-2">
            Nhập ít nhất 2 ký tự để tìm kiếm
          </h3>
          <p className="text-muted-foreground text-sm max-w-md mx-auto leading-relaxed">
            Hệ thống hỗ trợ tìm kiếm toàn văn bản tiếng Việt có dấu hoặc không dấu. Hãy nhập tên món ăn, nguyên liệu hoặc chọn một trong các gợi ý phổ biến ở trên.
          </p>
        </div>
      )}

      {/* 5. TRƯỜNG HỢP 2: ĐANG TẢI DỮ LIỆU */}
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

      {/* 6. TRƯỜNG HỢP 3: CÓ LỖI XẢY RA (VÍ DỤ LỖI MẠNG HOẶC HTTP 429 RATE LIMIT) */}
      {!isQueryTooShort && !isLoading && isError && (
        <div className="py-8">
          <ErrorState
            error={error}
            title={
              (error as any)?.status === 429
                ? "Tần suất tìm kiếm quá nhanh"
                : "Không thể thực hiện tìm kiếm"
            }
            message={
              (error as any)?.status === 429
                ? "Bạn đang gửi yêu cầu quá thường xuyên. Vui lòng nghỉ ngơi vài giây rồi nhấn Thử lại nhé!"
                : undefined
            }
            onRetry={() => refetch()}
            retryLabel="Thử lại"
          />
        </div>
      )}

      {/* 7. TRƯỜNG HỢP 4: CÓ KẾT QUẢ TÌM KIẾM */}
      {!isQueryTooShort && !isLoading && !isError && recipes.length > 0 && (
        <div>
          <div className="flex items-center justify-between mb-6 flex-wrap gap-2">
            <div>
              <h2 className="text-lg sm:text-xl font-bold text-foreground">
                Tìm thấy <span className="text-primary">{totalCount}</span> công thức cho &quot;{debouncedQuery}&quot;
              </h2>
              {hasActiveFilters && (
                <p className="text-xs text-muted-foreground mt-0.5">
                  (Đang áp dụng bộ lọc tùy chỉnh)
                </p>
              )}
            </div>
            <span className="text-xs text-muted-foreground">
              Trang {page} / {totalPages}
            </span>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
            {recipes.map((recipe: RecipeSummaryDto, idx: number) => (
              <RecipeCard key={recipe.id || recipe.slug} recipe={recipe} priority={idx < 3} />
            ))}
          </div>

          {/* Phân trang */}
          {totalPages > 1 && (
            <div className="mt-8">
              <Pagination page={page} totalPages={totalPages} onPageChange={(p) => setPage(p)} />
            </div>
          )}
        </div>
      )}

      {/* 8. TRƯỜNG HỢP 5: KHÔNG TÌM THẤY KẾT QUẢ */}
      {!isQueryTooShort && !isLoading && !isError && recipes.length === 0 && (
        <div className="py-8">
          <EmptyState
            title={`Không tìm thấy công thức nào cho "${debouncedQuery}"`}
            description="Mẹo tìm kiếm: Hãy thử kiểm tra lại chính tả, thử từ khóa ngắn gọn hơn (như 'bò', 'cá', 'canh') hoặc thử bỏ bớt các tiêu chí lọc."
            action={{
              label: hasActiveFilters ? "Xóa bộ lọc & thử lại" : "Thử từ khóa khác",
              onClick: () => {
                if (hasActiveFilters) {
                  handleResetAllFilters();
                } else {
                  handleClearSearch();
                }
              },
            }}
          />
        </div>
      )}
    </div>
  );
}
