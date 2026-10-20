import { Loader2 } from "lucide-react";
import type { ComponentProps } from "react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

interface SubmitButtonProps extends Omit<ComponentProps<typeof Button>, "type"> {
  loading?: boolean;
  /** Chữ hiển thị khi đang gửi (mặc định giữ nguyên children). */
  loadingText?: string;
}

/** Nút gửi form (shadcn Button) có trạng thái đang xử lý: khoá nút và hiện vòng quay. */
export function SubmitButton({ loading = false, loadingText, disabled, children, className, ...rest }: SubmitButtonProps) {
  return (
    <Button
      type="submit"
      size="lg"
      className={cn("h-10 w-full px-6", className)}
      disabled={disabled || loading}
      aria-busy={loading || undefined}
      {...rest}
    >
      {loading && <Loader2 className="animate-spin" aria-hidden="true" />}
      {loading && loadingText ? loadingText : children}
    </Button>
  );
}
