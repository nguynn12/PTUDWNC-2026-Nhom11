import type { ComponentProps } from "react";
import type { FieldError } from "react-hook-form";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { cn } from "@/lib/utils";

interface FieldMessageProps {
  id: string;
  error?: FieldError;
  hint?: string;
}

/** Dòng lỗi (ưu tiên) hoặc gợi ý dưới một ô nhập. */
export function FieldMessage({ id, error, hint }: FieldMessageProps) {
  if (error?.message) {
    return (
      <p id={`${id}-error`} className="text-sm text-destructive">
        {error.message}
      </p>
    );
  }
  if (!hint) return null;
  return (
    <p id={`${id}-hint`} className="text-xs text-muted-foreground">
      {hint}
    </p>
  );
}

export function describedBy(id: string, error?: FieldError, hint?: string): string | undefined {
  return error ? `${id}-error` : hint ? `${id}-hint` : undefined;
}

interface FieldProps extends ComponentProps<"input"> {
  label: string;
  error?: FieldError;
  /** Gợi ý hiển thị dưới ô khi chưa có lỗi. */
  hint?: string;
}

/** Ô nhập có nhãn (shadcn Label + Input), gợi ý và lỗi; dùng trực tiếp với register() của react-hook-form. */
export function Field({ label, error, hint, id, className, ...rest }: FieldProps) {
  const inputId = id ?? rest.name ?? label;
  return (
    <div className="grid gap-2">
      <Label htmlFor={inputId}>{label}</Label>
      <Input
        id={inputId}
        className={cn("h-10", className)}
        aria-invalid={error ? true : undefined}
        aria-describedby={describedBy(inputId, error, hint)}
        {...rest}
      />
      <FieldMessage id={inputId} error={error} hint={hint} />
    </div>
  );
}
