import React from "react";
import { Card } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { cn } from "@/lib/utils";

export interface RecipeGridSkeletonProps {
  count?: number;
  className?: string;
  columnsClassName?: string;
}

export function RecipeGridSkeleton({
  count = 6,
  className,
  columnsClassName = "grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6",
}: RecipeGridSkeletonProps) {
  return (
    <div
      aria-label="Đang tải danh sách công thức"
      className={cn("w-full", className)}
    >
      <div className={columnsClassName}>
        {Array.from({ length: count }).map((_, i) => (
          <Card
            key={i}
            className="flex flex-col h-full overflow-hidden border border-[var(--border-subtle)] bg-[var(--bg-card)] rounded-2xl shadow-xs"
          >
            {/* Ảnh bìa Skeleton tỷ lệ 16:10 */}
            <div className="relative w-full pt-[62.5%] bg-muted/60">
              <div className="absolute top-3 inset-x-3 flex items-center justify-between pointer-events-none">
                <Skeleton className="h-5 w-20 rounded-full" />
                <Skeleton className="h-5 w-14 rounded-full" />
              </div>
            </div>

            {/* Nội dung chi tiết thẻ */}
            <div className="flex flex-col flex-1 p-4 sm:p-5 gap-3">
              {/* Thông tin nhanh: thời gian, khẩu phần */}
              <div className="flex items-center gap-3">
                <Skeleton className="h-4 w-16 rounded" />
                <Skeleton className="h-4 w-16 rounded" />
              </div>

              {/* Tiêu đề công thức 2 dòng */}
              <div className="space-y-1.5 flex-1">
                <Skeleton className="h-5 w-4/5 rounded" />
                <Skeleton className="h-5 w-3/5 rounded" />
              </div>

              {/* Mô tả ngắn */}
              <div className="space-y-1 pt-1">
                <Skeleton className="h-3.5 w-full rounded" />
                <Skeleton className="h-3.5 w-5/6 rounded" />
              </div>

              {/* Chân thẻ: Tác giả và nút xem */}
              <div className="flex items-center justify-between pt-3 border-t border-border/50 mt-1">
                <div className="flex items-center gap-2">
                  <Skeleton className="w-6 h-6 rounded-full shrink-0" />
                  <Skeleton className="h-3.5 w-20 rounded" />
                </div>
                <Skeleton className="h-4 w-20 rounded" />
              </div>
            </div>
          </Card>
        ))}
      </div>
    </div>
  );
}

export default RecipeGridSkeleton;
