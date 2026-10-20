import React from 'react';
import Link from 'next/link';
import { AlertCircle, RotateCcw, Home } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import { cn } from '@/lib/utils';
import { getErrorMessage, isAppError } from '@/lib/errors';

export interface ErrorStateProps {
  /** Đối tượng lỗi tùy chọn (AppError, AxiosError, Error thông thường). */
  error?: unknown;
  /** Tiêu đề lỗi chính. Mặc định là 'Đã xảy ra lỗi'. */
  title?: string;
  /** Thông điệp chi tiết mô tả lỗi. Nếu không truyền sẽ tự động giải mã qua getErrorMessage(error). */
  message?: string;
  /** Mã lỗi hệ thống chuẩn hóa (RFC 7807 hoặc mã nghiệp vụ, ví dụ: RATE_LIMIT_EXCEEDED, FORBIDDEN). */
  errorCode?: string;
  /** Hàm callback thực thi khi người dùng nhấn nút 'Thử lại'. */
  onRetry?: () => void;
  /** Nhãn của nút thử lại. Mặc định là 'Thử lại'. */
  retryLabel?: string;
  /** Tùy chọn hiển thị nút 'Về trang chủ'. Mặc định là true. */
  showBackHome?: boolean;
  /** Lớp CSS tùy biến. */
  className?: string;
}

/**
 * Component ErrorState hiển thị lỗi thân thiện theo chuẩn RFC 7807 (Lab 6 - TV3).
 * Tích hợp hàm giải mã getErrorMessage() hiển thị thông báo tiếng Việt trực quan,
 * tránh hoàn toàn hiện tượng sập màn hình trắng khi API backend hoặc kết nối mạng gặp sự cố.
 */
export function ErrorState({
  error,
  title = 'Đã xảy ra lỗi',
  message,
  errorCode,
  onRetry,
  retryLabel = 'Thử lại',
  showBackHome = true,
  className,
}: ErrorStateProps) {
  const displayMessage =
    message ??
    (error
      ? getErrorMessage(error)
      : 'Không thể tải được nội dung vào lúc này. Vui lòng kiểm tra kết nối mạng và thử lại.');

  const displayCode =
    errorCode ?? (isAppError(error) ? error.code : undefined);
  return (
    <Card
      role="alert"
      aria-live="assertive"
      className={cn(
        'max-w-xl mx-auto border-destructive/30 bg-destructive/5 shadow-md overflow-hidden transition-all',
        className
      )}
    >
      <CardContent className="flex flex-col items-center text-center p-4 sm:p-8 gap-4 sm:gap-5">
        <div className="relative">
          <div className="absolute -inset-2 rounded-full bg-destructive/15 animate-ping duration-1000" />
          <div className="relative rounded-full bg-destructive/10 p-4 text-destructive border border-destructive/20 shadow-sm">
            <AlertCircle className="w-10 h-10" aria-hidden="true" />
          </div>
        </div>

        <div className="space-y-2 max-w-md">
          <div className="flex flex-wrap items-center justify-center gap-2">
            <h3 className="text-xl font-bold tracking-tight text-foreground">
              {title}
            </h3>
            {displayCode && (
              <Badge variant="outline" className="border-destructive/40 text-destructive font-mono text-xs">
                {displayCode}
              </Badge>
            )}
          </div>
          <p className="text-sm text-muted-foreground leading-relaxed">
            {displayMessage}
          </p>
        </div>

        <div className="flex flex-wrap items-center justify-center gap-3 pt-2">
          {onRetry && (
            <Button
              type="button"
              variant="default"
              onClick={onRetry}
              className="gap-2 shadow-sm font-medium"
            >
              <RotateCcw className="w-4 h-4" aria-hidden="true" />
              {retryLabel}
            </Button>
          )}

          {showBackHome && (
            <Button
              asChild
              type="button"
              variant="outline"
              className="gap-2 border-border/80 font-medium hover:bg-background"
            >
              <Link href="/">
                <Home className="w-4 h-4" aria-hidden="true" />
                Về trang chủ
              </Link>
            </Button>
          )}
        </div>
      </CardContent>
    </Card>
  );
}

export default ErrorState;
