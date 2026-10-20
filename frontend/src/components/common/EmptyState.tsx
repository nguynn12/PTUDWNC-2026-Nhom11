import React from 'react';
import Link from 'next/link';
import { UtensilsCrossed, PlusCircle } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { cn } from '@/lib/utils';

export interface EmptyStateProps {
  /** Tiêu đề chính thể hiện trạng thái rỗng. */
  title: string;
  /** Mô tả chi tiết hoặc gợi ý hành động tiếp theo. */
  description?: string;
  /** Biểu tượng minh họa tùy chọn. Nếu không truyền sẽ dùng UtensilsCrossed mặc định. */
  icon?: React.ReactNode;
  /** Nút kêu gọi hành động (Call To Action). Có thể là link href hoặc onClick. */
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
 * Component EmptyState hiển thị trạng thái chưa có dữ liệu hoặc tìm kiếm rỗng (Lab 6 - TV3).
 * Giúp giao diện sinh động, thân thiện và định hướng rõ ràng cho người dùng.
 */
export function EmptyState({
  title,
  description,
  icon,
  action,
  className,
}: EmptyStateProps) {
  return (
    <Card
      className={cn(
        'max-w-xl mx-auto border-dashed border-2 border-border/80 bg-muted/20 shadow-none text-center',
        className
      )}
    >
      <CardContent className="flex flex-col items-center justify-center p-10 gap-5">
        <div className="rounded-3xl bg-primary/10 p-5 text-primary border border-primary/20 shadow-sm transition-transform duration-300 hover:scale-105">
          {icon ?? <UtensilsCrossed className="w-10 h-10" aria-hidden="true" />}
        </div>

        <div className="space-y-2 max-w-md">
          <h3 className="text-xl font-bold tracking-tight text-foreground">
            {title}
          </h3>
          {description && (
            <p className="text-sm text-muted-foreground leading-relaxed">
              {description}
            </p>
          )}
        </div>

        {action && (
          <div className="pt-2">
            {action.href ? (
              <Button asChild className="gap-2 shadow-sm font-medium">
                <Link href={action.href}>
                  {action.icon ?? <PlusCircle className="w-4 h-4" aria-hidden="true" />}
                  {action.label}
                </Link>
              </Button>
            ) : action.onClick ? (
              <Button
                type="button"
                onClick={action.onClick}
                className="gap-2 shadow-sm font-medium"
              >
                {action.icon ?? <PlusCircle className="w-4 h-4" aria-hidden="true" />}
                {action.label}
              </Button>
            ) : null}
          </div>
        )}
      </CardContent>
    </Card>
  );
}

export default EmptyState;
