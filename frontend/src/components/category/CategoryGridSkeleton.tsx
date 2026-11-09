import React from "react";
import { Card } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

interface CategoryGridSkeletonProps {
  count?: number;
}

/**
 * Skeleton component hiển thị trạng thái đang tải cho lưới danh mục món ăn.
 * Tỷ lệ kích thước và khoảng cách khớp 100% với CategoryCard thực tế (CLS = 0).
 */
export function CategoryGridSkeleton({ count = 10 }: CategoryGridSkeletonProps) {
  return (
    <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-4 xl:grid-cols-5 gap-4 sm:gap-6 mt-8">
      {Array.from({ length: count }).map((_, idx) => (
        <Card
          key={idx}
          className="h-full flex flex-col items-center p-5 text-center bg-card border-border/80"
        >
          {/* Vòng tròn ảnh đại diện Skeleton */}
          <Skeleton className="w-18 h-18 sm:w-20 sm:h-20 rounded-full mb-3.5 shrink-0" />

          {/* Tiêu đề danh mục Skeleton */}
          <Skeleton className="h-5 w-28 rounded-md mb-2" />

          {/* Mô tả ngắn Skeleton */}
          <Skeleton className="h-3.5 w-36 rounded-md mb-2.5" />

          {/* Badge đếm công thức Skeleton */}
          <Skeleton className="h-5 w-20 rounded-full mt-auto" />
        </Card>
      ))}
    </div>
  );
}
