import React from "react";
import { Container } from "@/components/common/Container";
import { Skeleton } from "@/components/ui/skeleton";
import { CategoryGridSkeleton } from "@/components/category";

/**
 * Trang trạng thái tải (Loading State) tự động của Next.js App Router cho phân hệ Categories.
 * Ngăn chặn Layout Shift và đem lại cảm giác phản hồi nhanh mượt mà.
 */
export default function CategoriesLoading() {
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
          </div>

          {/* Badge skeleton */}
          <Skeleton className="h-6 w-48 rounded-full" />

          {/* Title skeleton */}
          <Skeleton className="h-9 w-72 sm:w-96 rounded-lg" />

          {/* Description skeleton */}
          <Skeleton className="h-4 w-full rounded-md" />
          <Skeleton className="h-4 w-3/4 rounded-md" />
        </div>

        {/* Skeleton cho Lưới danh mục */}
        <CategoryGridSkeleton count={10} />
      </Container>
    </div>
  );
}
