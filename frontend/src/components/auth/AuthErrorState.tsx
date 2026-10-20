import { CircleAlert, RotateCcw } from "lucide-react";
import { Button } from "@/components/ui/button";

interface AuthErrorStateProps {
  title?: string;
  message?: string;
  onRetry?: () => void;
  retryLabel?: string;
}

/** Trạng thái lỗi khi không tải được dữ liệu của module Xác thực (vd. hồ sơ). */
export function AuthErrorState({
  title = "Đã có lỗi xảy ra",
  message,
  onRetry,
  retryLabel = "Thử lại",
}: AuthErrorStateProps) {
  return (
    <div className="flex flex-col items-center gap-3 py-8 text-center" role="alert">
      <span className="flex size-12 items-center justify-center rounded-full bg-destructive/10 text-destructive">
        <CircleAlert className="size-6" aria-hidden="true" />
      </span>
      <p className="font-semibold">{title}</p>
      {message && <p className="max-w-sm text-sm text-muted-foreground">{message}</p>}
      {onRetry && (
        <Button variant="outline" size="sm" onClick={onRetry}>
          <RotateCcw /> {retryLabel}
        </Button>
      )}
    </div>
  );
}
