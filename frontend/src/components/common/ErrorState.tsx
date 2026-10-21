import React from "react";
import { AlertCircle, RefreshCw } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { cn } from "@/lib/utils";
import { getErrorMessage } from "@/lib/errors";

export interface ErrorStateProps {
  /** Tiêu đề thông báo lỗi. */
  title?: string;
  /** Thông điệp lỗi chi tiết muốn hiển thị. */
  message?: string;
  /** Đối tượng lỗi gốc nhận được (Error, AppError hoặc unknown). */
  error?: unknown;
  /** Hàm callback để kích hoạt thử lại thao tác tải dữ liệu. */
  onRetry?: () => void;
  /** Lớp CSS tùy biến. */
  className?: string;
}

/**
 * Component ErrorState hiển thị trạng thái lỗi trực quan, thân thiện (Lab 6).
 * Tự động phân giải thông điệp tiếng Việt qua getErrorMessage nếu truyền đối tượng lỗi.
 */
export function ErrorState({
  title = "Đã xảy ra sự cố",
  message,
  error,
  onRetry,
  className,
}: ErrorStateProps) {
  const displayMessage =
    message ||
    (error ? getErrorMessage(error) : "Không thể tải dữ liệu vào lúc này. Vui lòng thử lại sau.");

  return (
    <Card
      role="alert"
      className={cn(
        "max-w-xl mx-auto border-destructive/30 bg-destructive/5 shadow-none text-center rounded-2xl",
        className
      )}
    >
      <CardContent className="flex flex-col items-center justify-center p-6 sm:p-10 gap-4 sm:gap-5">
        <div className="rounded-3xl bg-destructive/10 p-5 text-destructive border border-destructive/20 shadow-xs">
          <AlertCircle className="w-10 h-10" aria-hidden="true" />
        </div>

        <div className="space-y-2 max-w-md">
          <h3 className="text-xl font-bold tracking-tight text-destructive">
            {title}
          </h3>
          <p className="text-sm text-muted-foreground leading-relaxed">
            {displayMessage}
          </p>
        </div>

        {onRetry && (
          <div className="pt-2">
            <Button
              type="button"
              variant="outline"
              onClick={onRetry}
              className="gap-2 border-destructive/30 hover:bg-destructive/10 hover:text-destructive cursor-pointer font-medium"
            >
              <RefreshCw className="w-4 h-4" aria-hidden="true" />
              Thử lại
            </Button>
          </div>
        )}
      </CardContent>
    </Card>
  );
}

export default ErrorState;
