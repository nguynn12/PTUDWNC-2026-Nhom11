import React from "react";
import { Container } from "@/components/common/Container";
import { Skeleton } from "@/components/ui/skeleton";
import { RecipeGridSkeleton } from "@/components/recipe";

/**
 * Loading State tự động cho trang chi tiết danh mục (/categories/[slug]).
 * Đảm bảo CLS = 0 khi người dùng chuyển từ trang chủ hoặc danh mục vào xem bài viết.
 */
export default function CategoryDetailLoading() {
  return (
    <div className="py-8 sm:py-12">
      <Container>
        {/* Skeleton cho PageHeader */}
        <div className="space-y-3 max-w-2xl">
          {/* Breadcrumbs skeleton */}
          <div className="flex items-center gap-2">
            <Skeleton className="h-4 w-16 rounded-md" />
            <span className="text-muted-foreground/40 text-xs">/</span>
            <Skeleton className="h-4 w-20 rounded-md" />
            <span className="text-muted-foreground/40 text-xs">/</span>
            <Skeleton className="h-4 w-28 rounded-md" />
          </div>

          {/* Badge skeleton */}
          <Skeleton className="h-6 w-36 rounded-full" />

          {/* Title skeleton */}
          <Skeleton className="h-9 w-64 sm:w-80 rounded-lg" />

          {/* Description skeleton */}
          <Skeleton className="h-4 w-full rounded-md" />
          <Skeleton className="h-4 w-2/3 rounded-md" />
        </div>

        {/* Skeleton cho thanh sắp xếp */}
        <div className="flex items-center justify-between gap-4 mt-8 pb-4 border-b border-border/60">
          <Skeleton className="h-5 w-40 rounded-md" />
          <div className="flex gap-2">
            <Skeleton className="h-8 w-24 rounded-lg" />
            <Skeleton className="h-8 w-28 rounded-lg" />
          </div>
        </div>

        {/* Skeleton cho Lưới công thức */}
        <div className="mt-8">
          <RecipeGridSkeleton count={6} />
        </div>
      </Container>
    </div>
  );
}
