import React from "react";
import { Card } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { cn } from "@/lib/utils";

export interface CategorySidebarSkeletonProps {
  className?: string;
  itemCount?: number;
}

export function CategorySidebarSkeleton({
  className,
  itemCount = 8,
}: CategorySidebarSkeletonProps) {
  return (
    <aside
      aria-label="Đang tải danh mục món ăn"
      className={cn("w-full transition-all duration-200", className)}
    >
      {/* =====================================================================
          1. GIAO DIỆN DESKTOP SKELETON (lg >= 1024px)
          ===================================================================== */}
      <div className="hidden lg:block sticky top-24 max-h-[calc(100vh-120px)] flex flex-col">
        <Card className="p-4 shadow-xs border-border/70 bg-card flex flex-col h-full overflow-hidden">
          {/* Header Skeleton */}
          <div className="flex items-center justify-between pb-3.5 border-b border-border/60 shrink-0">
            <div className="flex items-center gap-2.5">
              <Skeleton className="w-8 h-8 rounded-lg shrink-0" />
              <div className="space-y-1.5">
                <Skeleton className="h-4 w-32 rounded" />
                <Skeleton className="h-3 w-24 rounded" />
              </div>
            </div>
            <Skeleton className="h-5 w-8 rounded-full" />
          </div>

          {/* List Items Skeleton */}
          <div className="py-2 space-y-1.5 flex-1 overflow-hidden">
            {Array.from({ length: itemCount }).map((_, i) => (
              <div
                key={i}
                className="flex items-center justify-between px-3 py-2.5 rounded-lg"
              >
                <div className="flex items-center gap-2.5 flex-1 pr-2">
                  <Skeleton className="w-2 h-2 rounded-full shrink-0" />
                  <Skeleton
                    className="h-4 rounded"
                    style={{
                      width: `${55 + ((i * 17) % 35)}%`,
                    }}
                  />
                </div>
                <Skeleton className="h-4 w-6 rounded-full shrink-0" />
              </div>
            ))}
          </div>

          {/* Footer Link Skeleton */}
          <div className="pt-3 border-t border-border/60 shrink-0">
            <Skeleton className="h-8 w-full rounded-md" />
          </div>
        </Card>
      </div>

      {/* =====================================================================
          2. GIAO DIỆN MOBILE & TABLET SKELETON (< 1024px)
          ===================================================================== */}
      <div className="block lg:hidden mb-6">
        <div className="flex items-center justify-between mb-2.5 px-0.5">
          <Skeleton className="h-4 w-28 rounded" />
          <Skeleton className="h-3 w-16 rounded" />
        </div>

        {/* Horizontal Chips Skeleton */}
        <div className="flex items-center gap-2 overflow-x-hidden pb-2 -mx-4 px-4 sm:mx-0 sm:px-0">
          <Skeleton className="h-7 w-20 rounded-full shrink-0" />
          <Skeleton className="h-7 w-24 rounded-full shrink-0" />
          <Skeleton className="h-7 w-28 rounded-full shrink-0" />
          <Skeleton className="h-7 w-20 rounded-full shrink-0" />
          <Skeleton className="h-7 w-24 rounded-full shrink-0" />
        </div>
      </div>
    </aside>
  );
}

export default CategorySidebarSkeleton;
