import React from "react";
import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { getCategoryBySlug } from "@/lib/api/categories";
import { getRecipes } from "@/lib/api/recipes";
import { RecipeCard } from "@/components/recipe/RecipeCard";
import { Pagination } from "@/components/common/Pagination";
import type { CategoryDetailDto, RecipeSummaryDto } from "@/types/api";

// ISR 600 giây (10 phút) theo đặc tả SRS Mục 5.1 (Dòng 1788)
export const revalidate = 600;

interface PageProps {
  params: Promise<{ slug: string }>;
  searchParams: Promise<{ page?: string }>;
}

export async function generateMetadata({ params }: PageProps): Promise<Metadata> {
  const { slug } = await params;
  try {
    const category = await getCategoryBySlug(slug);
    if (!category) return { title: "Danh Mục Món Ăn" };

    return {
      title: `${category.name} — Công Thức Nấu Ăn`,
      description:
        category.description ||
        `Khám phá các công thức nấu món ${category.name} thơm ngon, chuẩn vị được tuyển chọn tại Culinary Blog.`,
      openGraph: {
        title: `${category.name} — Công Thức Nấu Ăn | Culinary Blog`,
        description: category.description || `Tổng hợp các công thức món ${category.name} chất lượng cao.`,
      },
    };
  } catch {
    return { title: "Danh Mục Món Ăn" };
  }
}

export default async function CategoryDetailPage({ params, searchParams }: PageProps) {
  const { slug } = await params;
  const resolvedSearchParams = await searchParams;
  const currentPage = Math.max(1, Number(resolvedSearchParams.page) || 1);

  let category: CategoryDetailDto | null = null;
  let recipes: RecipeSummaryDto[] = [];
  let totalPages = 1;
  let totalCount = 0;

  try {
    category = await getCategoryBySlug(slug);

    if (category) {
      const recRes = await getRecipes({
        categoryId: category.id,
        page: currentPage,
        pageSize: 12,
      });

      recipes = recRes.data || [];
      if (recRes.meta) {
        totalPages = recRes.meta.totalPages || 1;
        totalCount = recRes.meta.totalCount || recipes.length;
      } else {
        totalCount = recipes.length;
      }
    }
  } catch {
    // Nếu gặp lỗi kết nối hoặc slug không tồn tại
    category = null;
  }

  // Nếu không tìm thấy danh mục trong database
  if (!category) {
    notFound();
  }

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
          marginBottom: "24px",
          flexWrap: "wrap",
        }}
      >
        <Link href="/" style={{ color: "var(--text-secondary)" }}>Trang chủ</Link>
        <span>/</span>
        <Link href="/categories" style={{ color: "var(--text-secondary)" }}>Danh mục</Link>
        <span>/</span>
        <span style={{ color: "var(--primary)", fontWeight: 600 }}>{category.name}</span>
      </nav>

      {/* Header Danh mục */}
      <header
        style={{
          backgroundColor: "var(--bg-surface)",
          border: "1px solid var(--border-subtle)",
          borderRadius: "var(--radius-xl)",
          padding: "36px 32px",
          boxShadow: "var(--shadow-sm)",
          marginBottom: "40px",
        }}
      >
        <span className="eyebrow">Chuyên mục ẩm thực</span>
        <h1 style={{ fontSize: "clamp(2rem, 3.5vw + 1rem, 2.75rem)", marginBottom: "12px" }}>
          {category.name}
        </h1>
        {category.description && (
          <p style={{ fontSize: "1.1rem", color: "var(--text-secondary)", maxWidth: "68ch", margin: "0 0 16px" }}>
            {category.description}
          </p>
        )}
        <div style={{ display: "flex", alignItems: "center", gap: "10px" }}>
          <span className="badge badge-category" style={{ fontSize: "0.85rem", padding: "4px 12px" }}>
            {totalCount || category.recipeCount || 0} công thức đã xuất bản
          </span>
        </div>
      </header>

      {/* Lưới danh sách công thức thuộc danh mục */}
      {recipes.length > 0 ? (
        <>
          <div className="recipe-grid">
            {recipes.map((recipe, idx) => (
              <RecipeCard key={recipe.id || recipe.slug} recipe={recipe} priority={idx < 3} />
            ))}
          </div>

          {/* Phân trang */}
          <Pagination
            page={currentPage}
            totalPages={totalPages}
            createPageUrl={(p) => `/categories/${slug}?page=${p}`}
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
          <div style={{ fontSize: "3rem", marginBottom: "16px" }}>🍲</div>
          <h3 style={{ fontSize: "1.3rem", marginBottom: "8px", color: "var(--text-primary)" }}>
            Chưa có công thức nào trong danh mục này
          </h3>
          <p style={{ color: "var(--text-muted)", maxWidth: "450px", margin: "0 auto 24px" }}>
            Các đầu bếp của Culinary Blog đang chuẩn bị những món ngon mới. Bạn vui lòng quay lại sau nhé!
          </p>
          <Link href="/recipes" className="btn btn-primary">
            Duyệt các công thức khác →
          </Link>
        </div>
      )}
    </div>
  );
}
