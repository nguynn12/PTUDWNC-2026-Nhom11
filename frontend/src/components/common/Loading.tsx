import React from 'react';
import { Loader2 } from 'lucide-react';
import { cn } from '@/lib/utils';

export interface LoadingProps {
  /** Kích thước biểu tượng xoay (sm: 16px, md: 28px, lg: 44px). Mặc định là 'md'. */
  size?: 'sm' | 'md' | 'lg';
  /** Biến thể hiển thị: inline (dòng), overlay (phủ thẻ cha relative), fullScreen (phủ toàn màn hình cố định). Mặc định 'inline'. */
  variant?: 'inline' | 'overlay' | 'fullScreen';
  /** Văn bản thông báo hiển thị kèm (nếu có). */
  text?: string;
  /** Lớp CSS tùy biến bổ sung. */
  className?: string;
}

const sizeClasses = {
  sm: 'w-4 h-4 text-sm',
  md: 'w-7 h-7 text-base',
  lg: 'w-11 h-11 text-lg',
};

const spinnerSizeClasses = {
  sm: 'w-4 h-4',
  md: 'w-7 h-7',
  lg: 'w-11 h-11',
};

/**
 * Component Loading đa năng dùng chung cho toàn bộ ứng dụng ẩm thực (Lab 6 - TV3).
 * Hỗ trợ các biến thể inline, overlay mờ và phủ toàn màn hình (fullScreen) với màu thương hiệu.
 */
export function Loading({
  size = 'md',
  variant = 'inline',
  text,
  className,
}: LoadingProps) {
  const content = (
    <div
      role="status"
      aria-live="polite"
      className={cn(
        'flex flex-col items-center justify-center gap-3 text-muted-foreground select-none',
        sizeClasses[size],
        className
      )}
    >
      <Loader2
        className={cn(
          'animate-spin text-primary transition-all duration-300',
          spinnerSizeClasses[size]
        )}
        aria-hidden="true"
      />
      {text && (
        <span className="font-medium text-foreground/80 tracking-wide animate-pulse">
          {text}
        </span>
      )}
      <span className="sr-only">Đang nạp dữ liệu...</span>
    </div>
  );

  if (variant === 'fullScreen') {
    return (
      <div
        className="fixed inset-0 z-50 flex items-center justify-center bg-background/80 backdrop-blur-sm transition-opacity duration-300"
        aria-modal="true"
      >
        <div className="rounded-2xl bg-card/90 p-8 shadow-2xl border border-border/50 max-w-sm w-full mx-4 text-center">
          {content}
        </div>
      </div>
    );
  }

  if (variant === 'overlay') {
    return (
      <div className="absolute inset-0 z-20 flex items-center justify-center bg-background/70 backdrop-blur-[2px] rounded-lg transition-all">
        {content}
      </div>
    );
  }

  return content;
}

export default Loading;
