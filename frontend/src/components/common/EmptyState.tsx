import React from "react";
import Link from "next/link";
import { UtensilsCrossed, PlusCircle } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { cn } from "@/lib/utils";

export interface EmptyStateProps {
  /** Tiêu đề chính thể hiện trạng thái rỗng. */
  title?: string;
  /** Mô tả chi tiết hoặc gợi ý hành động tiếp theo. */
  description?: string;
  /** Biểu tượng minh họa tùy chọn. */
  icon?: React.ReactNode;
  /** Nút kêu gọi hành động (Quý - TV4 props). */
  actionText?: string;
  actionHref?: string;
  onAction?: () => void;
  /** Nút kêu gọi hành động (TV3 props). */
  action?: {
    label: string;
    href?: string;
    onClick?: () => void;
    icon?: React.ReactNode;
  };
  /** Lớp CSS tùy biến. */
  className?: string;
}

/**
 * Component EmptyState hiển thị trạng thái chưa có dữ liệu hoặc tìm kiếm rỗng (Lab 6).
 * Hỗ trợ linh hoạt cả prop actionText/actionHref và prop action { label, href, onClick }.
 */
export function EmptyState({
  title = "Không tìm thấy dữ liệu",
  description = "Hiện tại chưa có mục nào phù hợp với yêu cầu của bạn.",
  icon,
  actionText,
  actionHref,
  onAction,
  action,
  className,
}: EmptyStateProps) {
  const finalActionLabel = action?.label ?? actionText;
  const finalActionHref = action?.href ?? actionHref;
  const finalActionClick = action?.onClick ?? onAction;
  const finalActionIcon = action?.icon ?? <PlusCircle className="w-4 h-4" aria-hidden="true" />;

  return (
    <Card
      className={cn(
        "max-w-xl mx-auto border-dashed border-2 border-[var(--border-subtle)] bg-[var(--bg-surface)] shadow-none text-center rounded-2xl",
        className
      )}
    >
      <CardContent className="flex flex-col items-center justify-center p-6 sm:p-10 gap-4 sm:gap-5">
        <div className="rounded-3xl bg-[var(--primary-light)] p-5 text-[var(--primary)] border border-[var(--primary-subtle)] shadow-xs transition-transform duration-300 hover:scale-105">
          {icon ?? <UtensilsCrossed className="w-10 h-10" aria-hidden="true" />}
        </div>

        <div className="space-y-2 max-w-md">
          <h3 className="text-xl font-bold tracking-tight text-[var(--text-primary)]">
            {title}
          </h3>
          {description && (
            <p className="text-sm text-[var(--text-secondary)] leading-relaxed">
              {description}
            </p>
          )}
        </div>

        {finalActionLabel && (
          <div className="pt-2">
            {finalActionHref ? (
              <Button asChild className="gap-2 shadow-sm font-medium">
                <Link href={finalActionHref}>
                  {finalActionIcon}
                  {finalActionLabel}
                </Link>
              </Button>
            ) : finalActionClick ? (
              <Button
                type="button"
                onClick={finalActionClick}
                className="gap-2 shadow-sm font-medium"
              >
                {finalActionIcon}
                {finalActionLabel}
              </Button>
            ) : null}
          </div>
        )}
      </CardContent>
    </Card>
  );
}

export default EmptyState;
