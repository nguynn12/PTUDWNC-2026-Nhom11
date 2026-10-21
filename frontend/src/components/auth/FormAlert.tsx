import { CircleAlert, CircleCheck, Info, TriangleAlert, type LucideIcon } from "lucide-react";
import type { ReactNode } from "react";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { cn } from "@/lib/utils";

export type FormAlertVariant = "error" | "success" | "warning" | "info";

const VARIANTS: Record<FormAlertVariant, { icon: LucideIcon; className: string }> = {
  error: { icon: CircleAlert, className: "border-destructive/40 bg-destructive/5 text-destructive" },
  success: { icon: CircleCheck, className: "border-secondary/40 bg-secondary/10 text-secondary" },
  warning: { icon: TriangleAlert, className: "border-amber-300 bg-amber-50 text-amber-900" },
  info: { icon: Info, className: "border-primary/30 bg-primary/5 text-foreground" },
};

interface FormAlertProps {
  variant?: FormAlertVariant;
  title?: ReactNode;
  /** Nút hoặc liên kết đặt dưới nội dung (vd. "Gửi lại email"). */
  action?: ReactNode;
  className?: string;
  children: ReactNode;
}

/** Thông báo trong form (shadcn Alert). Lỗi dùng role="alert" để trình đọc màn hình đọc ngay. */
export function FormAlert({ variant = "error", title, action, className, children }: FormAlertProps) {
  const { icon: Icon, className: tone } = VARIANTS[variant];
  return (
    <Alert
      variant={variant === "error" ? "destructive" : "default"}
      role={variant === "error" ? "alert" : "status"}
      className={cn(tone, className)}
    >
      <Icon />
      {title && <AlertTitle>{title}</AlertTitle>}
      <AlertDescription className="text-inherit [&_a]:font-semibold [&_a]:underline">
        <div>{children}</div>
        {action && <div className="mt-1">{action}</div>}
      </AlertDescription>
    </Alert>
  );
}
