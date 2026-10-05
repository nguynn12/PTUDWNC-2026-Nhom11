import React from "react";
import type { Metadata } from "next";
import Link from "next/link";
import { getRecipes } from "@/lib/api/recipes";
import { getCategories } from "@/lib/api/categories";
import { RecipeFilterSidebar } from "@/components/recipe/RecipeFilterSidebar";
import { RecipeToolbar } from "@/components/recipe/RecipeToolbar";
import { RecipeListWrapper } from "@/components/recipe/RecipeListWrapper";
import { Pagination } from "@/components/common/Pagination";
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

  // Hàm tạo link phân trang bảo toàn toàn bộ bộ lọc
  const createPageUrl = (targetPage: number) => {
    const p = new URLSearchParams();
    p.set("page", targetPage.toString());
    if (categoryId) p.set("categoryId", categoryId);
    if (difficulty) p.set("difficulty", difficulty.toString());
    if (maxTotalTime) p.set("maxTotalTime", maxTotalTime.toString());
    if (minCalories) p.set("minCalories", minCalories.toString());
    if (maxCalories) p.set("maxCalories", maxCalories.toString());
    if (sortBy) p.set("sortBy", sortBy);
    if (sortOrder) p.set("sortOrder", sortOrder);
    return `/recipes?${p.toString()}`;
  };

  return (
    <div className="container section">
      {/* Breadcrumb */}
      <nav
        aria-label="Breadcrumb"
        style={{
          display: "flex",
          alignItems: "center",
          gap: "8px",
          fontSize: "0.88rem",
          color: "var(--text-muted)",
          marginBottom: "20px",
        }}
      >
        <Link href="/" style={{ color: "var(--text-secondary)" }}>Trang chủ</Link>
        <span>/</span>
        <span style={{ color: "var(--primary)", fontWeight: 600 }}>Duyệt công thức</span>
      </nav>

      {/* Header trang */}
      <div style={{ marginBottom: "32px" }}>
        <span className="eyebrow">Thực đơn phong phú mỗi ngày</span>
        <h1 style={{ fontSize: "clamp(2rem, 3.5vw + 1rem, 2.75rem)", marginBottom: "8px" }}>
          Kho Tàng Công Thức Nấu Ăn
        </h1>
        <p style={{ fontSize: "1.05rem", color: "var(--text-secondary)", maxWidth: "68ch" }}>
          Tìm kiếm cảm hứng vào bếp với các công thức được định lượng chuẩn xác và minh bạch dinh dưỡng.
        </p>
      </div>

      {/* Bố cục 2 cột: Sidebar bộ lọc bên trái + Danh sách bài viết bên phải */}
      <div
        style={{
          display: "flex",
          gap: "32px",
          alignItems: "flex-start",
        }}
      >
        {/* CỘT 1: SIDEBAR BỘ LỌC ĐA TIÊU CHÍ */}
        <RecipeFilterSidebar categories={categories} />

        {/* CỘT 2: TOOLBAR & DANH SÁCH BÀI VIẾT */}
        <div style={{ flex: 1, minWidth: 0, width: "100%" }}>
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
              <Pagination
                page={page}
                totalPages={totalPages}
                createPageUrl={createPageUrl}
              />
            </>
          ) : (
            <div
              style={{
                textAlign: "center",
                padding: "64px 20px",
                backgroundColor: "var(--bg-surface)",
                borderRadius: "var(--radius-lg)",
                border: "1px dashed var(--border-default)",
              }}
            >
              <div style={{ fontSize: "3rem", marginBottom: "16px" }}>🔍</div>
              <h3 style={{ fontSize: "1.25rem", marginBottom: "8px", color: "var(--text-primary)" }}>
                Không tìm thấy công thức phù hợp
              </h3>
              <p style={{ color: "var(--text-muted)", maxWidth: "450px", margin: "0 auto 24px" }}>
                Hãy thử nới lỏng các tiêu chí lọc (thời gian, calo, độ khó) để tìm thấy nhiều món ăn ngon hơn.
              </p>
              <Link href="/recipes" className="btn btn-outline">
                Xóa toàn bộ bộ lọc
              </Link>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
