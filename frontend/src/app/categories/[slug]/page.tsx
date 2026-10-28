import React, { Suspense } from "react";
import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { getCategoryBySlug } from "@/lib/api/categories";
import { getRecipes } from "@/lib/api/recipes";
import { RecipeCard } from "@/components/recipe/RecipeCard";
import { Pagination } from "@/components/common/Pagination";
import { Container } from "@/components/common/Container";
import { PageHeader } from "@/components/common/PageHeader";
import { ErrorState } from "@/components/common/ErrorState";
import { EmptyState } from "@/components/common/EmptyState";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Clock, Flame, Sparkles } from "lucide-react";
import type { CategoryDetailDto, RecipeSummaryDto, GetRecipesParams } from "@/types/api";

// ISR 600 giây (10 phút) theo đặc tả SRS
export const revalidate = 600;

interface PageProps {
  params: Promise<{ slug: string }>;
  searchParams: Promise<{ page?: string; sort?: string }>;
}

export async function generateMetadata({ params }: PageProps): Promise<Metadata> {
  const { slug } = await params;
  try {
    const category = await getCategoryBySlug(slug);
    if (!category) {
      return {
        title: "Danh Mục Không Tồn Tại",
        description: "Không tìm thấy danh mục món ăn yêu cầu.",
      };
    }

    const title = `${category.name} — Khám Phá Công Thức Chuẩn Vị`;
    const description =
      category.description ||
      `Khám phá các công thức nấu món ${category.name} thơm ngon, chuẩn vị với định lượng nguyên liệu chính xác và dinh dưỡng minh bạch tại Culinary Blog.`;

    return {
      title,
      description,
      alternates: {
        canonical: `/categories/${category.slug}`,
      },
      openGraph: {
        title: `${title} | Culinary Blog`,
        description,
        url: `/categories/${category.slug}`,
        images: category.imageUrl
          ? [{ url: category.imageUrl, alt: category.name }]
          : undefined,
      },
      twitter: {
        card: "summary_large_image",
        title: `${title} | Culinary Blog`,
        description,
      },
    };
  } catch {
    return {
      title: "Danh Mục Món Ăn",
      description: "Khám phá công thức nấu ăn theo chủ đề tại Culinary Blog.",
    };
  }
}

export default async function CategoryDetailPage({ params, searchParams }: PageProps) {
  const { slug } = await params;
  const resolvedSearchParams = await searchParams;
  const currentPage = Math.max(1, Number(resolvedSearchParams.page) || 1);
  const currentSort = resolvedSearchParams.sort || "newest";

  // Xác thực tham số sắp xếp hợp lệ
  const validSortBy: GetRecipesParams["sortBy"] =
    currentSort === "quickest" || currentSort === "calories" ? currentSort : "newest";

  let category: CategoryDetailDto | null = null;
  let fetchError: unknown = null;

  try {
    category = await getCategoryBySlug(slug);
  } catch (err: unknown) {
    const errorStatus = (err as { status?: number })?.status;
    // Nếu API trả về 404 thực sự thì điều hướng tới trang 404 chuẩn của Next.js
    if (errorStatus === 404) {
      notFound();
    }
    // Lỗi mạng hoặc sự cố máy chủ khác
    fetchError = err;
  }

  // Nếu gặp sự cố máy chủ/mạng và không lấy được thông tin danh mục
  if (fetchError && !category) {
    return (
      <div className="py-12">
        <Container>
          <ErrorState
            error={fetchError}
            title="Không thể tải thông tin danh mục"
            message="Đã có sự cố kết nối tới máy chủ. Vui lòng kiểm tra lại dịch vụ Backend hoặc thử lại sau ít phút."
            showBackHome={true}
          />
        </Container>
      </div>
    );
  }

  // Trường hợp không có dữ liệu danh mục
  if (!category) {
    notFound();
  }

  let recipes: RecipeSummaryDto[] = [];
  let totalPages = 1;
  let totalCount = 0;

  try {
    const recRes = await getRecipes({
      categoryId: category.id,
      page: currentPage,
      pageSize: 12,
      sortBy: validSortBy,
    });

    recipes = recRes?.data || [];
    if (recRes?.meta) {
      totalPages = recRes.meta.totalPages || 1;
      totalCount = recRes.meta.totalCount ?? recRes.meta.total ?? recipes.length;
    } else {
      totalCount = recipes.length;
    }
  } catch (err) {
    console.error("Lỗi khi tải danh sách công thức theo danh mục:", err);
  }

  const recipeCountDisplay = totalCount || category.recipeCount || 0;

  return (
    <div className="py-8 sm:py-12">
      <Container>
        {/* Header danh mục có Breadcrumbs chuẩn SEO */}
        <PageHeader
          breadcrumbs={[
            { label: "Trang chủ", href: "/" },
            { label: "Danh mục", href: "/categories" },
            { label: category.name },
          ]}
          badge="Chuyên mục ẩm thực"
          title={category.name}
          description={
            category.description ||
            `Khám phá các công thức nấu món ${category.name} đặc sắc nhất với định lượng và dinh dưỡng minh bạch.`
          }
          actions={
            <Badge
              variant="outline"
              className="text-sm font-medium px-3.5 py-1.5 border-primary/30 text-primary bg-primary/5"
            >
              {recipeCountDisplay} công thức đã xuất bản
            </Badge>
          }
        />

        {/* Thanh công cụ: Sắp xếp công thức */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mt-8 pb-4 border-b border-border/60">
          <div className="text-xs sm:text-sm text-muted-foreground">
            Hiển thị <span className="font-semibold text-foreground">{recipes.length}</span> trên tổng số{" "}
            <span className="font-semibold text-foreground">{recipeCountDisplay}</span> công thức
          </div>

          {/* Các nút chọn kiểu sắp xếp */}
          <div className="flex items-center gap-1.5 bg-muted/50 p-1 rounded-xl self-start sm:self-auto border border-border/50">
            <Button
              asChild
              size="sm"
              variant={validSortBy === "newest" ? "default" : "ghost"}
              className="rounded-lg text-xs h-8 px-3 gap-1.5"
            >
              <Link href={`/categories/${slug}?sort=newest`}>
                <Sparkles className="w-3.5 h-3.5" />
                Mới nhất
              </Link>
            </Button>

            <Button
              asChild
              size="sm"
              variant={validSortBy === "quickest" ? "default" : "ghost"}
              className="rounded-lg text-xs h-8 px-3 gap-1.5"
            >
              <Link href={`/categories/${slug}?sort=quickest`}>
                <Clock className="w-3.5 h-3.5" />
                Nấu nhanh
              </Link>
            </Button>

            <Button
              asChild
              size="sm"
              variant={validSortBy === "calories" ? "default" : "ghost"}
              className="rounded-lg text-xs h-8 px-3 gap-1.5"
            >
              <Link href={`/categories/${slug}?sort=calories`}>
                <Flame className="w-3.5 h-3.5" />
                Ít calo
              </Link>
            </Button>
          </div>
        </div>

        {/* Lưới danh sách công thức thuộc danh mục */}
        <div className="mt-8">
          {recipes.length > 0 ? (
            <>
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                {recipes.map((recipe, idx) => (
                  <RecipeCard key={recipe.id || recipe.slug} recipe={recipe} priority={idx < 3} />
                ))}
              </div>

              {/* Phân trang bọc trong Suspense */}
              <Suspense fallback={null}>
                <div className="mt-10">
                  <Pagination page={currentPage} totalPages={totalPages} />
                </div>
              </Suspense>
            </>
          ) : (
            <div className="mt-6">
              <EmptyState
                title="Chưa có công thức nào trong danh mục này"
                description={`Các đầu bếp của Culinary Blog đang chuẩn bị những món ngon mới cho chủ đề ${category.name}. Bạn vui lòng quay lại sau nhé!`}
                actionText="Khám phá danh mục khác"
                actionHref="/categories"
              />
            </div>
          )}
        </div>
      </Container>
    </div>
  );
}
