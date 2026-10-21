import { Loader2 } from "lucide-react";
import { cn } from "@/lib/utils";

/** Trạng thái đang tải của module Xác thực (kiểm tra phiên, tải hồ sơ). */
export function AuthLoading({ message = "Đang tải...", className }: { message?: string; className?: string }) {
  return (
    <div
      className={cn("flex flex-col items-center justify-center gap-3 py-10 text-center", className)}
      role="status"
      aria-live="polite"
    >
      <Loader2 className="size-7 animate-spin text-primary" aria-hidden="true" />
      <p className="text-sm text-muted-foreground">{message}</p>
    </div>
  );
}
