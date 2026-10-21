import React from 'react';
import { ChefHat } from 'lucide-react';
import { Loading } from './Loading';
import { cn } from '@/lib/utils';

export interface PageLoaderProps {
  /** Thông điệp hiển thị dưới biểu tượng đầu bếp. Mặc định là 'Đang chuẩn bị công thức...'. */
  text?: string;
  /** Lớp CSS tùy biến cho container bao ngoài. */
  className?: string;
}

/**
 * Component PageLoader hiển thị trạng thái đang tải cấp trang hoặc cấp layout (Lab 6 - TV3).
 * Kết hợp biểu tượng đầu bếp ẩm thực tạo ấn tượng thương hiệu chuyên nghiệp và hiệu ứng nhịp thở.
 */
export function PageLoader({
  text = 'Đang chuẩn bị công thức...',
  className,
}: PageLoaderProps) {
  return (
    <div
      role="status"
      aria-live="polite"
      className={cn(
        'min-h-[50vh] flex flex-col items-center justify-center gap-6 p-8 text-center select-none',
        className
      )}
    >
      <div className="relative flex items-center justify-center">
        {/* Vòng hào quang nhịp thở nền */}
        <div className="absolute -inset-3 rounded-full bg-primary/15 animate-ping duration-1000" />
        <div className="relative rounded-2xl bg-primary/10 p-5 text-primary border border-primary/20 shadow-inner">
          <ChefHat className="w-10 h-10 animate-bounce duration-1000" aria-hidden="true" />
        </div>
      </div>

      <Loading size="md" text={text} />
      <span className="sr-only">Trang đang được tải, vui lòng chờ trong giây lát...</span>
    </div>
  );
}

export default PageLoader;
