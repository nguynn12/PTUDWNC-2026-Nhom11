import React from "react";
import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { getCategoryBySlug } from "@/lib/api/categories";
import { getRecipes } from "@/lib/api/recipes";
import { RecipeCard } from "@/components/recipe/RecipeCard";
import { Pagination } from "@/components/common/Pagination";
import { Container } from "@/components/common/Container";
import { PageHeader } from "@/components/common/PageHeader";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { UtensilsCrossed } from "lucide-react";
import type { CategoryDetailDto, RecipeSummaryDto } from "@/types/api";

// ISR 600 giây (10 phút) theo đặc tả SRS Mục 5.1
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
    category = null;
  }

  // Nếu không tìm thấy danh mục trong database
  if (!category) {
    notFound();
  }

  const recipeCountDisplay = totalCount || category.recipeCount || 0;

  return (
    <div className="py-8 sm:py-12">
      <Container>
        {/* Header danh mục */}
        <PageHeader
          breadcrumbs={[
            { label: "Trang chủ", href: "/" },
            { label: "Danh mục", href: "/categories" },
            { label: category.name },
          ]}
          badge="Chuyên mục ẩm thực"
          title={category.name}
          description={category.description || `Khám phá các công thức nấu món ${category.name} đặc sắc nhất.`}
          actions={
            <Badge variant="outline" className="text-sm font-medium px-3.5 py-1.5 border-primary/30 text-primary bg-primary/5">
              {recipeCountDisplay} công thức đã xuất bản
            </Badge>
          }
        />

        {/* Lưới danh sách công thức thuộc danh mục */}
        <div className="mt-10">
          {recipes.length > 0 ? (
            <>
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                {recipes.map((recipe, idx) => (
                  <RecipeCard key={recipe.id || recipe.slug} recipe={recipe} priority={idx < 3} />
                ))}
              </div>

              {/* Phân trang */}
              <Pagination
                page={currentPage}
                totalPages={totalPages}
              />
            </>
          ) : (
            <div className="text-center py-16 px-6 rounded-2xl border-2 border-dashed border-border bg-card max-w-xl mx-auto">
              <div className="w-16 h-16 rounded-full bg-primary/10 text-primary flex items-center justify-center mx-auto mb-4">
                <UtensilsCrossed className="w-8 h-8" />
              </div>
              <h3 className="text-xl font-bold text-foreground mb-2">
                Chưa có công thức nào trong danh mục này
              </h3>
              <p className="text-muted-foreground text-sm max-w-md mx-auto mb-6">
                Các đầu bếp của Culinary Blog đang chuẩn bị những món ngon mới. Bạn vui lòng quay lại sau nhé!
              </p>
              <div className="flex items-center justify-center gap-3">
                <Button asChild variant="default">
                  <Link href="/recipes">Duyệt các công thức khác</Link>
                </Button>
                <Button asChild variant="outline">
                  <Link href="/categories">Tất cả danh mục</Link>
                </Button>
              </div>
            </div>
          )}
        </div>
      </Container>
    </div>
  );
}
