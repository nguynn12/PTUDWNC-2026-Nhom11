import React from "react";
import type { Metadata } from "next";
import Link from "next/link";
import { getRecipes } from "@/lib/api/recipes";
import { getCategories } from "@/lib/api/categories";
import { RecipeFilterSidebar } from "@/components/recipe/RecipeFilterSidebar";
import { RecipeToolbar } from "@/components/recipe/RecipeToolbar";
import { RecipeListWrapper } from "@/components/recipe/RecipeListWrapper";
import { Pagination } from "@/components/common/Pagination";
import { EmptyState } from "@/components/common/EmptyState";
import type {
  RecipeSummaryDto,
  CategoryDto,
  GetRecipesParams,
  RecipeDifficulty,
} from "@/types/api";

// Đánh dấu Server-Side Rendering (SSR) động theo URL searchParams (SRS Mục 5.1)
export const dynamic = "force-dynamic";

export const metadata: Metadata = {
  title: "Duyệt Công Thức Nấu Ăn",
  description:
    "Khám phá danh sách các công thức nấu ăn phong phú. Hỗ trợ bộ lọc đa tiêu chí theo độ khó, thời gian nấu, calo và danh mục tại Culinary Blog.",
  openGraph: {
    title: "Khám Phá Công Thức Nấu Ăn | Culinary Blog",
    description: "Bộ sưu tập công thức nấu ăn chuẩn vị kèm hướng dẫn từng bước chi tiết.",
  },
};

interface RecipesPageProps {
  searchParams: Promise<Record<string, string | undefined>>;
}

export default async function RecipesPage({ searchParams }: RecipesPageProps) {
  const resolvedParams = await searchParams;

  const page = Math.max(1, Number(resolvedParams.page) || 1);
  const pageSize = 12;
  const categoryId = resolvedParams.categoryId || undefined;
  const difficulty = (resolvedParams.difficulty as RecipeDifficulty) || undefined;
  const maxTotalTime = resolvedParams.maxTotalTime ? Number(resolvedParams.maxTotalTime) : undefined;
  const minCalories = resolvedParams.minCalories ? Number(resolvedParams.minCalories) : undefined;
  const maxCalories = resolvedParams.maxCalories ? Number(resolvedParams.maxCalories) : undefined;
  const sortBy = (resolvedParams.sortBy as GetRecipesParams["sortBy"]) || "newest";
  const sortOrder = (resolvedParams.sortOrder as "asc" | "desc") || "desc";

  const queryParams: GetRecipesParams = {
    page,
    pageSize,
    categoryId,
    difficulty,
    maxTotalTime,
    minCalories,
    maxCalories,
    sortBy,
    sortOrder,
  };

  let recipes: RecipeSummaryDto[] = [];
  let categories: CategoryDto[] = [];
  let totalCount = 0;
  let totalPages = 1;

  try {
    const [recRes, catRes] = await Promise.all([
      getRecipes(queryParams).catch(() => ({ data: [], meta: null })),
      getCategories().catch(() => []),
    ]);

    recipes = recRes.data || [];
    categories = catRes || [];

    if (recRes.meta) {
      totalCount = recRes.meta.totalCount || recipes.length;
      totalPages = recRes.meta.totalPages || 1;
    } else {
      totalCount = recipes.length;
    }
  } catch {
    recipes = [];
    categories = [];
  }

  return (
    <div className="container mx-auto px-4 py-8 md:py-12 max-w-7xl">
      {/* Breadcrumb */}
      <nav
        aria-label="Breadcrumb"
        className="flex items-center gap-2 text-sm text-[var(--text-muted)] mb-6"
      >
        <Link href="/" className="hover:text-[var(--primary)] transition-colors">
          Trang chủ
        </Link>
        <span>/</span>
        <span className="text-[var(--primary)] font-semibold">Duyệt công thức</span>
      </nav>

      {/* Header trang */}
      <div className="mb-8">
        <span className="eyebrow block mb-2 text-xs font-bold tracking-widest uppercase text-[var(--primary)]">
          Thực đơn phong phú mỗi ngày
        </span>
        <h1 className="text-3xl sm:text-4xl lg:text-5xl font-extrabold text-[var(--text-primary)] tracking-tight mb-3">
          Kho Tàng Công Thức Nấu Ăn
        </h1>
        <p className="text-base sm:text-lg text-[var(--text-secondary)] max-w-2xl leading-relaxed">
          Tìm kiếm cảm hứng vào bếp với các công thức được định lượng chuẩn xác và minh bạch dinh dưỡng.
        </p>
      </div>

      {/* Bố cục 2 cột: Sidebar bộ lọc bên trái + Danh sách bài viết bên phải */}
      <div className="flex flex-col lg:flex-row gap-8 items-start">
        {/* CỘT 1: SIDEBAR BỘ LỌC ĐA TIÊU CHÍ */}
        <RecipeFilterSidebar categories={categories} />

        {/* CỘT 2: TOOLBAR & DANH SÁCH BÀI VIẾT */}
        <div className="flex-1 min-w-0 w-full">
          <RecipeToolbar
            totalCount={totalCount}
            currentSort={sortBy}
            currentOrder={sortOrder}
          />

          {recipes.length > 0 ? (
            <>
              {/* Lưới / Danh sách thẻ công thức */}
              <RecipeListWrapper recipes={recipes} />

              {/* Phân trang đồng bộ URL */}
              <Pagination page={page} totalPages={totalPages} />
            </>
          ) : (
            <EmptyState
              icon="🔍"
              title="Không tìm thấy công thức phù hợp"
              description="Hãy thử nới lỏng các tiêu chí lọc (thời gian, calo, độ khó) để tìm thấy nhiều món ăn ngon hơn."
              actionText="Xóa toàn bộ bộ lọc"
              actionHref="/recipes"
            />
          )}
        </div>
      </div>
    </div>
  );
}

